//------------------------------------------------------------------------------
// <copyright file="AttachDebuggerCommand.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using aspnet_debug.Debugger.VisualStudio;
using aspnet_debug.Extension.Views;
using aspnet_debug.Shared.Communication;
using aspnet_debug.Shared.Server;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Command = aspnet_debug.Shared.Communication.Command;

namespace aspnet_debug.Extension
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class AttachDebuggerCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("f41b2136-3990-4f7d-96eb-04e6db30963c");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly Package package;

        /// <summary>
        /// Initializes a new instance of the <see cref="AttachDebuggerCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private AttachDebuggerCommand(Package package)
        {
            if (package == null)
            {
                throw new ArgumentNullException("package");
            }

            this.package = package;

            OleMenuCommandService commandService = this.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                var menuCommandID = new CommandID(CommandSet, CommandId);
                var menuItem = new OleMenuCommand(MenuItemCallback, menuCommandID);
                menuItem.BeforeQueryStatus += MenuItemOnBeforeQueryStatus;
                commandService.AddCommand(menuItem);
            }
        }

        private void MenuItemOnBeforeQueryStatus(object sender, EventArgs eventArgs)
        {
            var command = sender as OleMenuCommand;
            if (command != null)
                command.Text = "Attach Mono Debugger...";
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static AttachDebuggerCommand Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private IServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static void Initialize(Package package)
        {
            Instance = new AttachDebuggerCommand(package);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void MenuItemCallback(object sender, EventArgs e)
        {
            GetActiveIde().ExecuteCommand("File.SaveAll");

            var projects = GetProjects().Select(p => new ProjectDefinition() { Name = p.Name, Path = p.UniqueName}).ToList();

            var viewModel = new ProjectSelectorViewModel();
            viewModel.Projects = projects;

            DebugDefinition debugDefinition = null;
            using (ProjectSelector ps = new ProjectSelector(viewModel))
            {
                if (ps.ShowDialog().GetValueOrDefault(false))
                {
                    debugDefinition = ps.DebugDefinition;
                }
            }


            if (debugDefinition != null)
            {
                var solution = GetSolution();
                var tempFile = Path.Combine(Path.GetTempPath(), string.Format("{0}.zip", Path.GetFileName(solution.FileName)));
                
                if(File.Exists(tempFile))
                    File.Delete(tempFile);

                //Test Endpoint
                //Package Solution
                ZipFile.CreateFromDirectory(Path.GetDirectoryName(solution.FullName), tempFile);
                ExecutionParameters parameters = new ExecutionParameters();
                parameters.Command = Command.DebugContent;
                parameters.ProjectPath = debugDefinition.Project.Path;
                parameters.ExecutionCommand = debugDefinition.Command;
                parameters.Payload = File.ReadAllBytes(tempFile);

                Shared.Client.Client client = new Shared.Client.Client(debugDefinition.Endpoint, Server.ServicePort);

                client.Send(parameters);
                client.WaitForAnswer();
                //Transmit Package

                var dte = (DTE)Package.GetGlobalService(typeof(DTE));

                IntPtr pInfo = GetDebugInfo(debugDefinition.Endpoint, debugDefinition.Project.Path, debugDefinition.Project.Path);
                var sp = new ServiceProvider((Microsoft.VisualStudio.OLE.Interop.IServiceProvider)dte);
                try
                {
                    var dbg = (IVsDebugger)sp.GetService(typeof(SVsShellDebugger));
                    int hr = dbg.LaunchDebugTargets(1, pInfo);
                    Marshal.ThrowExceptionForHR(hr);

                    DebuggedProcess.Instance.AssociateDebugSession(client);
                }
                catch (Exception ex)
                {
                    //logger.Error(ex);
                    string msg;
                    var sh = (IVsUIShell)sp.GetService(typeof(SVsUIShell));
                    sh.GetErrorInfo(out msg);

                    if (!string.IsNullOrWhiteSpace(msg))
                    {
                        //logger.Error(msg);
                    }
                    throw;
                }
                finally
                {
                    if (pInfo != IntPtr.Zero)
                        Marshal.FreeCoTaskMem(pInfo);
                }

            }

            //string message = string.Format(CultureInfo.CurrentCulture, "Inside {0}.MenuItemCallback()", this.GetType().FullName);
            //string title = "AttachDebuggerCommand";

            //// Show a message box to prove we were here
            //VsShellUtilities.ShowMessageBox(
            //    this.ServiceProvider,
            //    message,
            //    title,
            //    OLEMSGICON.OLEMSGICON_INFO,
            //    OLEMSGBUTTON.OLEMSGBUTTON_OK,
            //    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }

        private IntPtr GetDebugInfo(string args, string targetExe, string outputDirectory)
        {
            var info = new VsDebugTargetInfo();
            info.cbSize = (uint)Marshal.SizeOf(info);
            info.dlo = DEBUG_LAUNCH_OPERATION.DLO_CreateProcess;

            info.bstrExe = Path.Combine(outputDirectory, targetExe);
            info.bstrCurDir = outputDirectory;
            info.bstrArg = args; // no command line parameters
            info.bstrRemoteMachine = null; // debug locally
            info.grfLaunch = (uint)__VSDBGLAUNCHFLAGS.DBGLAUNCH_StopDebuggingOnEnd;
            info.fSendStdoutToOutputWindow = 0;
            info.clsidCustom = AD7Guids.EngineGuid;
            info.grfLaunch = 0;

            IntPtr pInfo = Marshal.AllocCoTaskMem((int)info.cbSize);
            Marshal.StructureToPtr(info, pInfo, false);
            return pInfo;
        }

        private DTE2 GetActiveIde()
        {
            // Get an instance of currently running Visual Studio IDE.
            DTE2 dte2 = Package.GetGlobalService(typeof(DTE)) as DTE2;
            return dte2;
        }

        private IList<EnvDTE.Project> GetProjects()
        {
            Projects projects = GetActiveIde().Solution.Projects;
            List<EnvDTE.Project> list = new List<EnvDTE.Project>();
            var item = projects.GetEnumerator();
            while (item.MoveNext())
            {
                var project = item.Current as EnvDTE.Project;
                if (project == null)
                {
                    continue;
                }

                if (project.Kind == ProjectKinds.vsProjectKindSolutionFolder)
                {
                    list.AddRange(GetSolutionFolderProjects(project));
                }
                else
                {
                    list.Add(project);
                }
            }

            return list;
        }

        private EnvDTE.Solution GetSolution()
        {
            return GetActiveIde().Solution;
        }

        private IEnumerable<EnvDTE.Project> GetSolutionFolderProjects(EnvDTE.Project solutionFolder)
        {
            List<EnvDTE.Project> list = new List<EnvDTE.Project>();
            for (var i = 1; i <= solutionFolder.ProjectItems.Count; i++)
            {
                var subProject = solutionFolder.ProjectItems.Item(i).SubProject;
                if (subProject == null)
                {
                    continue;
                }

                // If this is another solution folder, do a recursive call, otherwise add
                if (subProject.Kind == ProjectKinds.vsProjectKindSolutionFolder)
                {
                    list.AddRange(GetSolutionFolderProjects(subProject));
                }
                else
                {
                    list.Add(subProject);
                }
            }
            return list;
        }
    }
}
