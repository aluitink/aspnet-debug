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
                            ExecutionParameters parameters = message as ExecutionParameters;

                            if (parameters != null)
                            {
                                _logger.Debug("DebugContent Received");

                                var tempSolutionPath = Path.Combine(Path.GetTempPath(), "Solution.zip");
                                File.WriteAllBytes(tempSolutionPath, (byte[])parameters.Payload);
                                _logger.DebugFormat("Extracting to {0}", Directory.GetCurrentDirectory());
                                ZipFile.ExtractToDirectory(tempSolutionPath, Directory.GetCurrentDirectory());
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