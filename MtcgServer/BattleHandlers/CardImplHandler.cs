using System;
using System.Collections.Generic;
using System.Linq;
using static MtcgServer.Helpers.Random;
using static MtcgServer.Helpers.Ranges;

namespace MtcgServer.BattleHandlers
{
    public class CardImplHandler : IBattleHandler
    {
        private const int MaxRounds = 100;
        private readonly IDatabase _db;

        public CardImplHandler(IDatabase db)
            => _db = db;

        public BattleResult RunBattle(Player p1, Player p2)
        {
            int round;
            List<string> log = new();
            List<ICard> // the players' decks
                p1Deck = new(p1.Deck),
                p2Deck = new(p2.Deck);

            if (!p1Deck.Any() || !p2Deck.Any())
                throw new ArgumentException("One or more players do not have any cards in their deck.");

            // while both still have cards remaining and < 100 rounds
            for (round = 0; round < MaxRounds && p1Deck.Any() && p2Deck.Any(); ++round)
            {
                log.Add("Round #" + (round + 1) + ":");
                var result = RunRound(p1Deck, p2Deck, log);

                // if not draw
                if (result.HasValue && result is var (card, from, to))
                {
                    from.Remove(card);
                    to.Add(card);
                    log.Add($"Card {card} has switched.");
                    log.Add($"{p1.Name} deck: {String.Join(", ", p1Deck.Select(c => c.ToString()))}");
                    log.Add($"{p2.Name} deck: {String.Join(", ", p2Deck.Select(c => c.ToString()))}");
                    continue;
                }

                log.Add("No cards have switched players.");
                log.Add("The decks remain the same as before.");
            }

            // only one player has cards left
            if (p1Deck.Any() != p2Deck.Any())
            {
                var (winner, loser) = p1Deck.Any() ? (p1, p2) : (p2, p1);
                var eloMod = (int)Math.Pow(1.5D, Math.Sqrt(MaxRounds - round)); // 1.5^sqrt(100-r), allows for up to 54 points

                // save changes
                _db.SavePlayer(winner with { Wins = winner.Wins + 1, ELO = FitInEloRange(winner.ELO + eloMod) }, PlayerChange.AfterGame);
                _db.SavePlayer(loser with { Losses = loser.Losses + 1, ELO = FitInEloRange(loser.ELO - eloMod) }, PlayerChange.AfterGame);

                return new BattleResult.Winner(winner, loser, log);
            }

            // both players have cards left
            log.Add("Maximum amount of rounds reached.");
            return new BattleResult.Draw(log);
        }

        private static (ICard switchingCard, List<ICard> from, List<ICard> to)? RunRound(List<ICard> p1Deck, List<ICard> p2Deck, List<string> log)
        {
            ICard // randomly chosen cards
                c1 = ChooseRandomCard(p1Deck),
                c2 = ChooseRandomCard(p2Deck);

            int // resulting damages
                c1Dmg = c1.CalculateDamage(c2),
                c2Dmg = c2.CalculateDamage(c1);

            log.Add($"{c1}: {c1Dmg} damage");
            log.Add($"{c2}: {c2Dmg} damage");

            if (c1Dmg < c2Dmg)
            {
                log.Add($"{c2} defeats {c1}.");
                return (c1, p1Deck, p2Deck);
            }

            if (c1Dmg > c2Dmg)
            {
                log.Add($"{c1} defeats {c2}.");
                return (c2, p2Deck, p1Deck);
            }

            return null; // draw
        }
    }
}
