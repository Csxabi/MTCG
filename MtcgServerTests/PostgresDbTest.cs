using MtcgServer.Cards.SpellCards;
using MtcgServer.Databases.Postgres;
using NUnit.Framework;
using System;
using static MtcgServerTests.Constants;

namespace MtcgServerTests
{
    public class PostgresDbTest
    {
        private MtcgServer.IDatabase _db;

        [SetUp]
        public void Setup()
        {
            _db = new PostgreSqlDatabase("Server=127.0.0.1; Port=5432; Database=mtcg; User Id=mtcg_user; Password=mtcg_pass");
        }

        [Test]
        public void TestSearchPlayer()
        {
            var playerId = _db.SearchPlayer(DemoUser1Name).Result;

            Assert.AreNotEqual(Guid.Empty, playerId);
            Assert.AreEqual(DemoUser1Id, playerId);
        }

        [Test]
        public void TestReadPlayerById()
        {
            var player = _db.ReadPlayer(DemoUser1Id).Result;

            Assert.NotNull(player);
            Assert.AreEqual(DemoUser1Id, player.Id);
            Assert.AreEqual(DemoUser1Name, player.Name);
        }

        [Test]
        public void TestCreateCard()
        {
            var card = new NormalSpell() { Id = DemoCard1Id, Damage = 24 };

            try
            {
                _db.CreateCard(card).RunSynchronously();
            }
            catch (Npgsql.PostgresException)
            {
                // may happen when database has not been reset as card has already been inserted before
                Assert.Warn("Did not create demo card as ID already exists.");
            }
        }

        [Test]
        public void TestReadCard()
        {
            var card = _db.ReadCard(DemoCard1Id).Result;

            Assert.NotNull(card);
            Assert.AreEqual(DemoCard1Id, card.Id);
        }
    }
}
