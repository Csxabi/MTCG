using NUnit.Framework;
using RestWebServer;
using System;
using System.Net;
using System.Threading.Tasks;

namespace MtcgServerTests
{
    [TestFixture]
    public class RestWebServerTest
    {
        private WebServer _web;

        [SetUp]
        public void SetupWebServer()
        {
            _web = new WebServer(new IPEndPoint(IPAddress.Any, 2200));
        }

        [Test]
        public void TestRegisterStaticRoute()
        {
            _web.RegisterStaticRoute("GET", "/api/echo", requestContext => Task.FromResult(new RestResponse(HttpStatusCode.OK, requestContext.Payload)));
        }

        [Test]
        public void TestRegisterResourceRoute()
        {
            _web.RegisterResourceRoute("GET", "/api/user/%", requestContext => Task.FromResult(new RestResponse(HttpStatusCode.OK, requestContext.Resources[0])));
        }

        [Test]
        public void TestRegisterStaticRouteTwice()
        {
            Assert.Catch<ArgumentException>(() =>
            {
                _web.RegisterStaticRoute("GET", "/api/echo", requestContext => Task.FromResult(new RestResponse(HttpStatusCode.OK, requestContext.Payload)));
                _web.RegisterStaticRoute("GET", "/api/echo", requestContext => Task.FromResult(new RestResponse(HttpStatusCode.OK, requestContext.Payload)));
            });
        }

        [Test]
        public void TestRegisterResourceRouteTwice()
        {
            Assert.Catch<ArgumentException>(() =>
            {
                _web.RegisterResourceRoute("GET", "/api/user/%", requestContext => Task.FromResult(new RestResponse(HttpStatusCode.OK, requestContext.Resources[0])));
                _web.RegisterResourceRoute("GET", "/api/user/%", requestContext => Task.FromResult(new RestResponse(HttpStatusCode.OK, requestContext.Resources[0])));
            });
        }

        [Test]
        public void TestRegisterStaticRouteTwiceCaseInsensitive()
        {
            Assert.Catch<ArgumentException>(() =>
            {
                _web.RegisterStaticRoute("GET", "/api/echo", requestContext => Task.FromResult(new RestResponse(HttpStatusCode.OK, requestContext.Payload)));
                _web.RegisterStaticRoute("get", "/api/ECHO", requestContext => Task.FromResult(new RestResponse(HttpStatusCode.OK, requestContext.Payload)));
            });
        }

        [Test]
        public void TestRegisterResourceRouteTwiceCaseInsensitive()
        {
            Assert.Catch<ArgumentException>(() =>
            {
                _web.RegisterResourceRoute("GET", "/api/user/%", requestContext => Task.FromResult(new RestResponse(HttpStatusCode.OK, requestContext.Resources[0])));
                _web.RegisterResourceRoute("get", "/api/USER/%", requestContext => Task.FromResult(new RestResponse(HttpStatusCode.OK, requestContext.Resources[0])));
            });
        }

        [Test]
        public void TestRegisterStaticRouteWhileListening()
        {
            _web.Start();
            Assert.Catch<InvalidOperationException>(() => _web.RegisterStaticRoute("GET", "/api/echo", requestContext => Task.FromResult(new RestResponse(HttpStatusCode.OK, requestContext.Payload))));
        }

        [Test]
        public void TestRegisterResourceRouteWhileListening()
        {
            _web.Start();
            Assert.Catch<InvalidOperationException>(() => _web.RegisterResourceRoute("GET", "/api/user/%", requestContext => Task.FromResult(new RestResponse(HttpStatusCode.OK, requestContext.Resources[0]))));
        }

        [TearDown]
        public void Teardown()
        {
            _web.Stop();
        }
    }
}
