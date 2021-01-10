using MtcgServer;
using MtcgServer.Scoreboards;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace MtcgServerTests
{
    public class ScoreboardTest
    {
        private List<Player> unorderedPlayers;

        [SetUp]
        public void Setup()
        {
            unorderedPlayers = new List<Player>
            {
                new Player(Guid.Empty, "Player A", new byte[0], string.Empty, string.Empty, 0, new ICard[0], new ICard[0], 200, 10, 9), // highest ELO
                new Player(Guid.Empty, "Player B", new byte[0], string.Empty, string.Empty, 0, new ICard[0], new ICard[0], 150, 30, 5), // most wins
                new Player(Guid.Empty, "Player C", new byte[0], string.Empty, string.Empty, 0, new ICard[0], new ICard[0], 100, 20, 0)  // least losses
            };
        }

        [Test]
        public void TestHighestELO()
        {
            var players = new List<Player>(unorderedPlayers);

            players.Sort(new HighestELO());

            Assert.AreEqual(unorderedPlayers[0], players[0]);
            Assert.AreEqual(unorderedPlayers[1], players[1]);
            Assert.AreEqual(unorderedPlayers[2], players[2]);
        }

        [Test]
        public void TestMostWins()
        {
            var players = new List<Player>(unorderedPlayers);

            players.Sort(new MostWins());

            Assert.AreEqual(unorderedPlayers[1], players[0]);
            Assert.AreEqual(unorderedPlayers[2], players[1]);
            Assert.AreEqual(unorderedPlayers[0], players[2]);
        }

        [Test]
        public void TestLeastLosses()
        {
            var players = new List<Player>(unorderedPlayers);

            players.Sort(new LeastLosses());

            Assert.AreEqual(unorderedPlayers[2], players[0]);
            Assert.AreEqual(unorderedPlayers[1], players[1]);
            Assert.AreEqual(unorderedPlayers[0], players[2]);
        }
    }
}
