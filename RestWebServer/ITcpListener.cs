using System;

namespace RestWebServer
{
    public interface ITcpListener
    {
        public void Start();

        public void Stop();

        public ITcpClient AcceptTcpClient();
    }
}
