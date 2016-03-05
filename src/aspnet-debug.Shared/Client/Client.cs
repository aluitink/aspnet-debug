using System.Net;
using System.Net.Sockets;

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
    }
}