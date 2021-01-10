using NUnit.Framework;
using RestWebServer;
using System;
using System.Collections.Generic;

namespace MtcgServerTests
{
    [TestFixture]
    public class RequestContextTest
    {
        [Test]
        public void TestRequestContextBasic()
        {
            var ctx = new RequestContext("GET", "/test", new string[0], new Dictionary<string, string>(), string.Empty);

            Assert.AreEqual("GET", ctx.Verb);
            Assert.AreEqual("/test", ctx.Path);
            Assert.AreEqual(string.Empty, ctx.Query);
            Assert.AreEqual(string.Empty, ctx.Fragment);
            Assert.AreEqual(0, ctx.Resources.Length);
            Assert.AreEqual(0, ctx.Headers.Count);
            Assert.AreEqual(string.Empty, ctx.Payload);
        }

        [Test]
        public void TestRequestContextWithQuery()
        {
            var ctx = new RequestContext("GET", "/test?key=value", new string[0], new Dictionary<string, string>(), string.Empty);

            Assert.AreEqual("GET", ctx.Verb);
            Assert.AreEqual("/test", ctx.Path);
            Assert.AreEqual("?key=value", ctx.Query);
            Assert.AreEqual(string.Empty, ctx.Fragment);
            Assert.AreEqual(0, ctx.Resources.Length);
            Assert.AreEqual(0, ctx.Headers.Count);
            Assert.AreEqual(string.Empty, ctx.Payload);
        }

        [Test]
        public void TestRequestContextWithFragment()
        {
            var ctx = new RequestContext("GET", "/test#title", new string[0], new Dictionary<string, string>(), string.Empty);

            Assert.AreEqual("GET", ctx.Verb);
            Assert.AreEqual("/test", ctx.Path);
            Assert.AreEqual(string.Empty, ctx.Query);
            Assert.AreEqual("#title", ctx.Fragment);
            Assert.AreEqual(0, ctx.Resources.Length);
            Assert.AreEqual(0, ctx.Headers.Count);
            Assert.AreEqual(string.Empty, ctx.Payload);
        }

        [Test]
        public void TestRequestContextWithQueryAndFragment()
        {
            var ctx = new RequestContext("GET", "/test?key=value#title", new string[0], new Dictionary<string, string>(), string.Empty);

            Assert.AreEqual("GET", ctx.Verb);
            Assert.AreEqual("/test", ctx.Path);
            Assert.AreEqual("?key=value", ctx.Query);
            Assert.AreEqual("#title", ctx.Fragment);
            Assert.AreEqual(0, ctx.Resources.Length);
            Assert.AreEqual(0, ctx.Headers.Count);
            Assert.AreEqual(string.Empty, ctx.Payload);
        }
    }
}
