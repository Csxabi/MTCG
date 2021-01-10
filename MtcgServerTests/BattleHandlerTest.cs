using MtcgServer;
using MtcgServer.BattleHandlers;
using MtcgServer.Cards.SpellCards;
using NUnit.Framework;
using System;

namespace MtcgServerTests
{
    public class BattleHandlerTest
    {
        private IBattleHandler _handler;

        [SetUp]
        public void Setup()
        {
            _handler = new CardImplHandler(new NullDb());
        }

        [Test]
        public void TestWinner()
        {
            ICard[] // decks limited to one card to avoid possible infinite loops
                cards1 = { new NormalSpell { Damage = 10 } },
                cards2 = { new NormalSpell { Damage = 1 } };

            Player
                player1 = new Player(Guid.Empty, "Player 1", Array.Empty<byte>(), String.Empty, String.Empty, 0, Array.Empty<ICard>(), cards1, 1000, 0, 0),
                player2 = new Player(Guid.Empty, "Player 2", Array.Empty<byte>(), String.Empty, String.Empty, 0, Array.Empty<ICard>(), cards2, 1000, 0, 0);

            var result = _handler.RunBattle(player1, player2);
            Assert.NotNull(result);
            Assert.IsInstanceOf<BattleResult.Winner>(result);
            var win = (BattleResult.Winner)result;
            Assert.AreEqual(player1, win.WinningPlayer);
            Assert.AreEqual(player2, win.LosingPlayer);
        }

        [Test]
        public void TestDraw()
        {
            ICard[] // same damage so as to constantly trigger draws
                cards1 = { new NormalSpell { Damage = 1 } },
                cards2 = { new NormalSpell { Damage = 1 } };

            Player
                player1 = new Player(Guid.Empty, "Player 1", Array.Empty<byte>(), String.Empty, String.Empty, 0, Array.Empty<ICard>(), cards1, 1000, 0, 0),
                player2 = new Player(Guid.Empty, "Player 2", Array.Empty<byte>(), String.Empty, String.Empty, 0, Array.Empty<ICard>(), cards2, 1000, 0, 0);

            var result = _handler.RunBattle(player1, player2);
            Assert.NotNull(result);
            Assert.IsInstanceOf<BattleResult.Draw>(result);
        }
    }
}
