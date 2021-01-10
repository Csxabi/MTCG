using System;
using System.Net.Sockets;

namespace RestWebServer
{
    internal class TcpListenerWrapper : ITcpListener
    {
        private readonly TcpListener _listener;

        internal TcpListenerWrapper(TcpListener listener)
            => _listener = listener;

        public ITcpClient AcceptTcpClient()
            => new TcpClientWrapper(_listener.AcceptTcpClient());

        public void Start()
            => _listener.Start();

        public void Stop()
            => _listener.Stop();
    }
}
