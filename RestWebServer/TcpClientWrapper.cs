using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace RestWebServer
{
    internal class TcpClientWrapper : ITcpClient
    {
        private readonly TcpClient _client;

        internal TcpClientWrapper(TcpClient client)
            => _client = client;

        public EndPoint RemoteEndPoint
            => _client.Client.RemoteEndPoint;

        public Stream GetReadStream()
            => _client.GetStream();

        public Stream GetWriteStream()
            => _client.GetStream();

        public void Close()
            => _client.Close();
    }
}
