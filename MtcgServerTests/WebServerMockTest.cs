using Moq;
using NUnit.Framework;
using RestWebServer;
using RestWebServerLauncher;
using System;
using System.IO;
using System.Text;
using System.Threading;

namespace MtcgServerTests
{
    [TestFixture]
    public class WebServerMockTest
    {
        [Test]
        public void Test()
        {
            const string incomingRequest = "GET /messages/1 HTTP/1.1\r\nHost: localhost: 2200\r\n";
            var rs = new MemoryStream(Encoding.UTF8.GetBytes(incomingRequest));
            var ws = new MemoryStream();
            var client = new Mock<ITcpClient>();
            client.Setup(c => c.GetReadStream()).Returns(rs);
            client.Setup(c => c.GetWriteStream()).Returns(ws);
            var listener = new Mock<ITcpListener>();
            listener.SetupSequence(l => l.AcceptTcpClient())
                .Returns(client.Object)
                .Returns(() => { Thread.CurrentThread.Join(); return default; });
            var web = new MessageServer(new WebServer(listener.Object));
            web.Start();

            Thread.Sleep(500); // ToDo: make this nicer
            Assert.AreEqual("HTTP/1.1 404 NotFound\r\n", Encoding.UTF8.GetString(ws.ToArray()));
        }
    }
}
