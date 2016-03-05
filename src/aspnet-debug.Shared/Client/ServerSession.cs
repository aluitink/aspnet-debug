using System.Net.Sockets;
using System.Threading.Tasks;
using aspnet_debug.Shared.Communication;

namespace aspnet_debug.Shared.Client
{
    public class ServerSession
    {
        private readonly Protocol _protocol;
        
        public ServerSession(Socket socket)
        {
            _protocol = new Protocol(socket);
        }
        
        public void Send(Command command, object payload)
        {
            _protocol.Send(command, payload);
        }

        public MessageBase Receive()
        {
            return _protocol.Receive();
        }

        public Task<MessageBase> ReceiveAsync()
        {
            return _protocol.ReceiveAsync();
        }
    }
}