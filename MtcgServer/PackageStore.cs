using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static MtcgServer.Helpers.Random;

namespace MtcgServer
{
    internal class PackageStore
    {
        private readonly IDatabase _db;
        private List<CardPackage> _cache;

        public bool Initialized => _cache is not null;

#pragma warning disable CS8618
        public PackageStore(IDatabase db)
#pragma warning restore CS8618
        {
            _db = db;
            // Usage of this class without checking Initialized is unexpected
        }

        /// <summary>
        /// Updates the cached list of packages.
        /// </summary>
        public async Task Update()
        {
            // no locking needed as assignment of reference types is atomic
            _cache = (await _db.GetPackages()).ToList();
        }

        /// <summary>
        /// Adds a new package to the package store.
        /// </summary>
        /// <param name="package">The package.</param>
        public async Task RegisterPackage(CardPackage package)
        {
            await _db.AddToPackages(package);
            await Update();
        }

        /// <summary>
        /// Gets all registered packages.
        /// </summary>
        /// <returns>A collection of all packages.</returns>
        public IReadOnlyCollection<CardPackage> GetPackages()
            => _cache.AsReadOnly();

        /// <summary>
        /// Retrieves the specified number of randomly chosen cards from the existing packages.
        /// </summary>
        /// <param name="count"></param>
        /// <returns>The randomly chosen cards.</returns>
        public List<ICard> GetRandomCards(int count)
        {
            if (_cache.Count == 0)
                throw new InvalidOperationException("Tried to retrieve randomly chosen cards when none were defined.");

            var allCards = _cache.SelectMany(p => p.Cards).ToList();
            var cards = new List<ICard>(count);

            for (int i = 0; i < count; ++i)
                cards.Add(ChooseRandomCard(allCards));

            return cards;
        }

        /// <summary>
        /// Gets a specific package.
        /// </summary>
        /// <param name="packageId">The ID of the package.</param>
        /// <returns>The appropriate package or <see langword="null"/>.</returns>
        public CardPackage? GetPackage(Guid packageId)
            => _cache.Find(p => p.Id == packageId);

        /// <summary>
        /// Gets all packages below a certain price.
        /// </summary>
        /// <param name="maxCoins">The maximum price.</param>
        /// <returns>An array of the affordable packages.</returns>
        public CardPackage[] GetAffordablePackages(int maxCoins)
            => _cache.Where(p => p.Price <= maxCoins).ToArray();

        /// <summary>
        /// Gets a randomly chosen package below a certain price.
        /// </summary>
        /// <param name="maxCoins">The maximum price.</param>
        /// <returns>A randomly chosen affordable package.</returns>
        public CardPackage? GetRandomAffordablePackage(int maxCoins)
            => !_cache.Any() ? null : ChooseRandom(GetAffordablePackages(maxCoins));
    }
}
