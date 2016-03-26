using System;
using System.Net;
using System.Net.Sockets;
using aspnet_debug.Shared.Communication;

namespace aspnet_debug.Shared.Client
{
    public class Client
    {
        private readonly TcpClient _tcpClient = new TcpClient();
        private readonly ServerSession _serverSession;

        public Client(string host, int port)
        {
            _tcpClient.Connect(IPAddress.Parse(host), port);
            _serverSession = new ServerSession(_tcpClient.Client);
        }

        public void Send(MessageBase message)
        {
            _serverSession.Send(message);
        }

        public void WaitForAnswer()
        {
            var message = _serverSession.Receive();
            if (message != null)
                return;
            throw new Exception("Cannot start debugging.");
        }
    }
}