using MtcgServer;
using MtcgServer.BattleHandlers;
using MtcgServer.Databases.Postgres;
using NUnit.Framework;
using System;
using System.Linq;
using static MtcgServerTests.Constants;

namespace MtcgServerTests
{
    public class MtcgModifyTest
    {
        private MtcgServer.MtcgServer _server;
        private Session _session;

        [SetUp]
        public void Setup()
        {
            var db = new PostgreSqlDatabase("Server=127.0.0.1; Port=5432; Database=mtcg; User Id=mtcg_user; Password=mtcg_pass");
            _server = new MtcgServer.MtcgServer(db, new CardImplHandler(db));
            _session = _server.Login(DemoUser2Name, DemoUser2Pass).Result;
        }

        [Test]
        public void TestEditPlayerName()
        {
            var name = DemoUser2Name; // max 8 chars
            Assert.IsTrue(_server.EditPlayer(_session, name: name).Result);
            Assert.AreEqual(name, _server.GetPlayer(_session).Result.Name);
        }

        [Test]
        public void TestEditPlayerStatusText()
        {
            var statusText = new String('A', 80); // max 80 chars
            Assert.IsTrue(_server.EditPlayer(_session, statusText: statusText).Result);
            Assert.AreEqual(statusText, _server.GetPlayer(_session).Result.StatusText);
        }

        [Test]
        public void TestEditPlayerEmoticonText()
        {
            var emoticonText = ">_<"; // max 8 chars
            Assert.IsTrue(_server.EditPlayer(_session, emoticon: emoticonText).Result);
            Assert.AreEqual(emoticonText, _server.GetPlayer(_session).Result.EmoticonText);
        }

        [Test]
        public void TestEditPlayerPassword()
        {
            var password = DemoUser2Pass; // no max length
            Assert.IsTrue(_server.EditPlayer(_session, pass: password).Result);
            // cannot assert correctness of password hash as helper functions are not public
        }

        [Test]
        public void TestEditPlayerMultiple()
        {
            var name = DemoUser2Name;
            var statusText = "testet deine Arbeit";
            var emoticonText = "-.-\"";
            var password = DemoUser2Pass;

            Assert.IsTrue(_server.EditPlayer(_session, name, statusText, emoticonText, password).Result);

            var player = _server.GetPlayer(_session).Result;
            Assert.NotNull(player);
            Assert.AreEqual(name, player.Name);
            Assert.AreEqual(statusText, player.StatusText);
            Assert.AreEqual(emoticonText, player.EmoticonText);
            // no comparison for password
        }

        [Test]
        public void TestSetDeck()
        {
            var player = _server.GetPlayer(_session).Result;

            // update deck and check for success
            var newDeck = player.Stack.Take(4).Select(c => c.Id).ToArray();
            Assert.IsTrue(_server.SetDeck(_session, newDeck).Result);

            // re-check updated deck
            var updatedPlayer = _server.GetPlayer(_session).Result;
            Assert.IsTrue(updatedPlayer.Deck.Select(c => c.Id).SequenceEqual(newDeck));
        }
    }
}
