using System;
using System.IO;
using System.Net;

namespace RestWebServer
{
    public interface ITcpClient
    {
        public EndPoint RemoteEndPoint { get; }

        public Stream GetReadStream();

        public Stream GetWriteStream();

        public void Close();
    }
}
