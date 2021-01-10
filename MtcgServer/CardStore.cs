using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MtcgServer
{
    /// <summary>
    /// Represents a store that holds tradable cards.
    /// </summary>
    internal class CardStore
    {
        private readonly IDatabase _db;
        private List<CardStoreEntry> _cache;
        private readonly SemaphoreSlim _lock;

        public bool Initialized => _cache is not null;

        public CardStore(IDatabase db)
        {
            _db = db;
            _cache = new List<CardStoreEntry>();
            _lock = new SemaphoreSlim(1);
        }

        public async Task Update()
        {
            await _lock.WaitAsync();

            _cache = (await _db.ReadStore()).ToList();

            _lock.Release();
        }

        // may only be called after init
        public IReadOnlyCollection<CardStoreEntry> GetAllCards()
            => _cache.AsReadOnly();

        // may only be called after init
        public IReadOnlyCollection<CardStoreEntry> GetEligibleCards(ICard match)
            => _cache.FindAll(e => e.Requirements.All(r => r.CheckRequirement(match)));

        public async Task<bool> PushCard(Player first, ICard card, ICollection<ICardRequirement> requirements)
        {
            // check if card is in player's stack but not their deck
            if (first.Deck.Contains(card) || !first.Stack.Contains(card))
                return false;

            // add to the card store
            await _db.AddToStore(first, card, requirements);
            await Update();
            return true;
        }

        public async Task<bool> TradeCard(Player second, ICard own, ICard other)
        {
            // check if card is in player's stack but not their deck
            if (second.Deck.Contains(own) || !second.Stack.Contains(other))
                return false;

            // get owner of other card
            var first = await _db.FindOwner(other);

            // swap cards
            second.Stack.Remove(own);
            second.Stack.Add(other);
            first.Stack.Remove(other);
            first.Stack.Add(own);

            // update database and reload
            await _db.SavePlayer(second, PlayerChange.AfterTrade);
            await _db.SavePlayer(first, PlayerChange.AfterTrade);
            await Update();
            return true;
        }
    }
}
