using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
using System.Text;
using aspnet_debug.Shared.Communication;
using aspnet_debug.Shared.Logging;
using aspnet_debug.Shared.Utils;
using log4net;

namespace aspnet_debug.Shared.Server
{
    //Used by the server
    public class ClientSession
    {
        private readonly ILog _logger = Log.GetLogger(typeof (ClientSession));
        private readonly Protocol _protocol;
        private readonly IPAddress _remoteAddress;

        public ClientSession(Socket socket)
        {
            _remoteAddress = ((IPEndPoint)socket.RemoteEndPoint).Address;
            _protocol = new Protocol(socket);
        }

        public void SessionHandler()
        {
            try
            {
                _logger.DebugFormat("New session from {0}", _remoteAddress);
                while (_protocol.IsConnected)
                {
                    MessageBase message = _protocol.Receive();

                    switch (message.Command)
                    {
                        case Command.DebugContent:
                            _logger.Debug("DebugContent Received");
                            ExecutionParameters parameters = message as ExecutionParameters;

                            if (parameters != null)
                            {
                                _logger.Debug("ExecutionParameters resolved");

                                var tempSolutionPath = Path.Combine(Path.GetTempPath(), "Solution.zip");
                                File.WriteAllBytes(tempSolutionPath, (byte[]) parameters.Payload);

                                var tempPath = Path.GetTempPath();
                                var solutionPath = Path.Combine(tempPath, "solution");

                                if (Directory.Exists(solutionPath))
                                    Directory.Delete(solutionPath, true);

                                Directory.CreateDirectory(solutionPath);
                                _logger.DebugFormat("Extracting to {0}", solutionPath);

                                //@@@ Needs work.. POC.
                                using (ZipArchive zip = ZipFile.OpenRead(tempSolutionPath))
                                {
                                    foreach (ZipArchiveEntry zipArchiveEntry in zip.Entries)
                                    {
                                        var path = zipArchiveEntry.FullName;
                                        var filePath = Path.Combine(solutionPath, path);
                                        filePath = filePath.Replace('\\', Path.DirectorySeparatorChar);

                                        var directoryPath = Path.GetDirectoryName(filePath);
                                        if (directoryPath != null)
                                            Directory.CreateDirectory(directoryPath);

                                        var fileName = Path.GetFileName(filePath);
                                        if (!string.IsNullOrWhiteSpace(fileName))
                                            zipArchiveEntry.ExtractToFile(filePath, true);
                                    }
                                }

                                var projectPath = parameters.ProjectPath.Replace('\\', Path.DirectorySeparatorChar);

                                var projectDirectory = Path.GetDirectoryName(projectPath);

                                var dir = Path.Combine(solutionPath, projectDirectory);

                                var temp = Path.GetTempPath();
                                var aspnetDebugPath = Path.Combine(temp, "aspnet-debug");
                                if (!Directory.Exists(aspnetDebugPath))
                                    Directory.CreateDirectory(aspnetDebugPath);
                                
                                BuildPdbs(dir);


                                BuildMdbs(Path.Combine(dir));


                                string command = parameters.ExecutionCommand;
                                StringBuilder stringBuilder = new StringBuilder();
                                Process process = new Process();
                                ProcessStartInfo startInfo = new ProcessStartInfo();
                                startInfo.UseShellExecute = true;
                                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                                startInfo.FileName = "dnx";
                                startInfo.Arguments = command;
                                startInfo.WorkingDirectory = dir;
                                //startInfo.RedirectStandardOutput = true;
                                //startInfo.RedirectStandardError = true;

                                process.StartInfo = startInfo;

                                process.OutputDataReceived += (sender, args) =>
                                {
                                    _logger.DebugFormat(args.Data);
                                };
                                process.ErrorDataReceived += (sender, args) =>
                                {
                                    _logger.ErrorFormat(args.Data);
                                };
                                process.Exited += (sender, args) =>
                                {
                                    _logger.DebugFormat("Process has exited - {0}", args.ToString());
                                };

                                _logger.DebugFormat("startInfo.WorkingDirectory: {0}", startInfo.WorkingDirectory);
                                _logger.DebugFormat("Running Command: {0}", command);
                                process.Start();

                                _logger.DebugFormat("Process running: {0}", !process.HasExited);

                                process.WaitForExit();

                                _logger.DebugFormat("-DNX-");
                                _logger.DebugFormat(stringBuilder.ToString());
                            }

                            break;
                        case Command.Started:
                            _logger.Debug("Started");
                            break;
                        case Command.Stopped:
                            _logger.Debug("Stopped");
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
            catch (Exception e)
            {
                _logger.ErrorFormat(e.ToString());
                throw;
            }
            finally
            {
            }
        }

        private void BuildMdbs(string pdbPath)
        {
            var generator = new Pdb2MdbGenerator();
            generator.GeneratePdb2Mdb(pdbPath);
        }

        private void BuildPdbs(string projectDirectory)
        {
            Environment.SetEnvironmentVariable("DNX_BUILD_PORTABLE_PDB", true.ToString());

            string command = string.Format("build");

            StringBuilder stringBuilder = new StringBuilder();
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.UseShellExecute = true;
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.FileName = "dnu";
            startInfo.Arguments = command;
            startInfo.WorkingDirectory = projectDirectory;
            //startInfo.RedirectStandardOutput = true;
            //startInfo.RedirectStandardError = true;

            process.StartInfo = startInfo;

            process.OutputDataReceived += (sender, args) =>
            {
                _logger.DebugFormat(args.Data);
            };
            process.ErrorDataReceived += (sender, args) =>
            {
                _logger.ErrorFormat(args.Data);
            };
            process.Exited += (sender, args) =>
            {
                _logger.DebugFormat("Process has exited - {0}", args.ToString());
            };

            _logger.DebugFormat("startInfo.WorkingDirectory: {0}", startInfo.WorkingDirectory);
            _logger.DebugFormat("Running Command: {0}", command);
            process.Start();

            _logger.DebugFormat("Process running: {0}", !process.HasExited);

            process.WaitForExit();

            _logger.DebugFormat("-DNX-");
            _logger.DebugFormat(stringBuilder.ToString());

        }
    }
}