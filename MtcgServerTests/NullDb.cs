using MtcgServer;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MtcgServerTests
{
    public class NullDb : IDatabase
    {
        public Task AddToPackages(CardPackage package)
            => Task.CompletedTask;

        public Task AddToStore(Player owner, ICard card, ICollection<ICardRequirement> requirements)
            => Task.CompletedTask;

        public Task CreateCard(ICard card)
            => Task.CompletedTask;

        public Task CreatePlayer(Player player)
            => Task.CompletedTask;

        public Task<Player> FindOwner(ICard card)
            => new Task<Player>(() => null);

        public Task<ICollection<CardPackage>> GetPackages()
            => new Task<ICollection<CardPackage>>(() => null);

        public Task<ICollection<Player>> ListPlayers()
            => new Task<ICollection<Player>>(() => null);

        public Task<ICard> ReadCard(Guid id)
            => new Task<ICard>(() => null);

        public Task<Player> ReadPlayer(Guid id)
            => new Task<Player>(() => null);

        public Task<ICollection<CardStoreEntry>> ReadStore()
            => new Task<ICollection<CardStoreEntry>>(() => null);

        public Task RemoveFromStore(ICard card)
            => Task.CompletedTask;

        public Task SavePlayer(Player player, PlayerChange changes)
            => Task.CompletedTask;

        public Task<Guid> SearchPlayer(string name)
            => Task.FromResult(Guid.Empty);
    }
}
