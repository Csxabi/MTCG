using MtcgServer;
using MtcgServer.BattleHandlers;
using MtcgServer.Databases.Postgres;
using NUnit.Framework;
using static MtcgServerTests.Constants;

namespace MtcgServerTests
{
    public class MtcgQueryTest
    {
        private MtcgServer.MtcgServer _server;
        private Session _session;

        [SetUp]
        public void Setup()
        {
            var db = new PostgreSqlDatabase("Server=127.0.0.1; Port=5432; Database=mtcg; User Id=mtcg_user; Password=mtcg_pass");
            _server = new MtcgServer.MtcgServer(db, new CardImplHandler(db));
            _session = _server.Login(DemoUser1Name, DemoUser1Pass).Result;
        }

        [Test]
        public void TestLogin()
        {
            var session = _server.Login(DemoUser1Name, DemoUser1Pass);

            Assert.NotNull(session);
        }

        [Test]
        public void TestLogout()
        {
            var session = _server.Login(DemoUser1Name, DemoUser1Pass).Result;
            var logoutResult = _server.Logout(session);

            Assert.IsTrue(logoutResult);
        }

        [Test]
        public void TestGetPlayer()
        {
            var player = _server.GetPlayer(_session).Result;

            Assert.NotNull(player);
            Assert.AreEqual(DemoUser1Id, player.Id);
            Assert.AreEqual(DemoUser1Name, player.Name);
        }

        [Test]
        public void TestGetPlayerByName()
        {
            var player = _server.GetPlayer(DemoUser1Name).Result;

            Assert.NotNull(player);
            Assert.AreEqual(DemoUser1Id, player.Id);
            Assert.AreEqual(DemoUser1Name, player.Name);
        }
    }
}
