using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using aspnet_debug.Shared.Logging;
using log4net;

namespace aspnet_debug.Shared.Server
{
    public class Server : IDisposable
    {
        public const int ServicePort = 13001;

        private readonly ILog _logger = Log.GetLogger(typeof (Server));
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        private TcpListener _tcpListener;
        private Task _listenTask;

        public void Start()
        {
            _tcpListener = new TcpListener(IPAddress.Any, ServicePort);
            _tcpListener.Start();
            _logger.Info("Server started.");
            _listenTask = Task.Factory.StartNew(() => StartListening(_cancellationTokenSource.Token), _cancellationTokenSource.Token);
        }

        public void Stop()
        {
            _cancellationTokenSource.Cancel();
            if (_tcpListener != null && _tcpListener.Server != null)
            {
                _tcpListener.Server.Close(0);
                _tcpListener = null;
            }
            if (_listenTask != null)
                Task.WaitAll(_listenTask);
        }

        public void WaitForExit()
        {
            _listenTask.Wait();
        }

        public void Dispose()
        {
            Stop();
        }

        private void StartListening(CancellationToken cancellationToken)
        {
            _logger.Info("Listening for new connections...");
            while (!cancellationToken.IsCancellationRequested)
            {
                TcpClient client = _tcpListener.AcceptTcpClient();
                cancellationToken.ThrowIfCancellationRequested();
                var clientSession = new ClientSession(client.Client);
                Task.Factory.StartNew(clientSession.SessionHandler, cancellationToken);
            }
        }
    }
}
