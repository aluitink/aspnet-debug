using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
using aspnet_debug.Shared.Communication;
using aspnet_debug.Shared.Logging;
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
                                File.WriteAllBytes(tempSolutionPath, (byte[])parameters.Payload);

                                var tempPath = Path.GetTempPath();
                                var solutionPath = Path.Combine(tempPath, "solution");

                                if (Directory.Exists(solutionPath))
                                    Directory.Delete(solutionPath);

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
                                        if(directoryPath != null)
                                            Directory.CreateDirectory(directoryPath);

                                        var fileName = Path.GetFileName(filePath);
                                        if(!string.IsNullOrWhiteSpace(fileName))
                                            zipArchiveEntry.ExtractToFile(filePath, true);
                                    }
                                }
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
            catch (Exception)
            {
                throw;
            }
            finally
            {
            }
        }
    }
}