﻿//------------------------------------------------------------------------------
// <copyright file="AttachDebuggerCommandPackage.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using aspnet_debug.Debugger.VisualStudio;
using aspnet_debug.Shared.Logging;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Win32;

namespace aspnet_debug.Extension
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(AttachDebuggerCommandPackage.PackageGuidString)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    public sealed class AttachDebuggerCommandPackage : Package
    {
        /// <summary>
        /// AttachDebuggerCommandPackage GUID string.
        /// </summary>
        public const string PackageGuidString = "f62eba81-56cf-4402-8a08-f1565274d852";

        /// <summary>
        /// Initializes a new instance of the <see cref="AttachDebuggerCommand"/> class.
        /// </summary>
        public AttachDebuggerCommandPackage()
        {
            // Inside this method you can place any initialization code that does not require
            // any Visual Studio service because at this point the package object is created but
            // not sited yet inside Visual Studio environment. The place to do all the other
            // initialization is the Initialize method.
        }

        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            AttachDebuggerCommand.Initialize(this);

            TryRegisterAssembly();

            base.Initialize();
        }


        private void TryRegisterAssembly()
        {
            try
            {
                RegistryKey regKey = Registry.ClassesRoot.OpenSubKey(@"CLSID\{8BF3AB9F-3864-449A-93AB-E7B0935FC8F5}");

                if (regKey != null)
                    return;

                string location = typeof(DebuggedProcess).Assembly.Location;

                string regasm = @"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\RegAsm.exe";
                if (!Environment.Is64BitOperatingSystem)
                    regasm = @"C:\Windows\Microsoft.NET\Framework\v4.0.30319\RegAsm.exe";

                var p = new ProcessStartInfo(regasm, location);
                p.Verb = "runas";
                p.RedirectStandardOutput = true;
                p.UseShellExecute = false;
                p.CreateNoWindow = true;

                Process proc = Process.Start(p);
                while (!proc.HasExited)
                {
                    string txt = proc.StandardOutput.ReadToEnd();
                }

                using (RegistryKey config = VSRegistry.RegistryRoot(__VsLocalRegistryType.RegType_Configuration))
                {
                    MonoDebuggerInstaller.RegisterDebugEngine(location, config);
                }
            }
            catch (UnauthorizedAccessException)
            {
                //MessageBox.Show(
                //    "Failed finish installation of MonoRemoteDebugger - Please run Visual Studio once as Administrator...",
                //    "MonoRemoteDebugger", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                //logger.Error(ex);
            }
        }
        #endregion
    }
}
