using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MtcgServer
{
    /// <summary>
    /// Specifies the operations for database used by MTCG.
    /// </summary>
    public interface IDatabase
    {
        /// <summary>
        /// Creates a new player.
        /// </summary>
        /// <param name="player">The player.</param>
        Task CreatePlayer(Player player);

        /// <summary>
        /// Uses a player's name to search for their <see cref="Player.Id"/>.
        /// </summary>
        /// <param name="name">The player's name.</param>
        /// <returns>The player's <see cref="Player.Id"/>.</returns>
        Task<Guid> SearchPlayer(string name);

        /// <summary>
        /// Reads the player's data and instantiates a <see cref="Player"/> object.
        /// </summary>
        /// <param name="id">The player's <see cref="Player.Id"/>.</param>
        /// <returns>An object representing the player.</returns>
        Task<Player?> ReadPlayer(Guid id);

        /// <summary>
        /// Reads all players' data and instantiates object for all players.
        /// Does not include the players' cards.
        /// </summary>
        /// <returns>A collection of all players.</returns>
        Task<ICollection<Player>> ListPlayers();

        /// <summary>
        /// Saves the marked changes to the database.
        /// </summary>
        /// <param name="player">The player object.</param>
        /// <param name="changes">Changes made since read.</param>
        Task SavePlayer(Player player, PlayerChange changes);

        /// <summary>
        /// Finds the owner of a card.
        /// </summary>
        /// <param name="card">The card.</param>
        /// <returns>The owner of the card.</returns>
        Task<Player> FindOwner(ICard card);

        /// <summary>
        /// Reads information about a card and instantiates an implementing object.
        /// </summary>
        /// <param name="id">The card's <see cref="ICard.Id"/>.</param>
        Task<ICard?> ReadCard(Guid id);

        /// <summary>
        /// Saves a new card to the database.
        /// The card must have a unique <see cref="ICard.Id"/>.
        /// </summary>
        /// <param name="card">The card.</param>
        Task CreateCard(ICard card);

        /// <summary>
        /// Reads all entries currently contained in the card store.
        /// </summary>
        /// <returns>A collection of cards and their trading requirements.</returns>
        Task<ICollection<CardStoreEntry>> ReadStore();

        /// <summary>
        /// Adds a new card to the store to mark it as available for trading.
        /// </summary>
        /// <param name="owner">The owner of the card.</param>
        /// <param name="card">The card itself.</param>
        /// <param name="requirements">Requirements that must be met when trading.</param>
        Task AddToStore(Player owner, ICard card, ICollection<ICardRequirement> requirements);

        /// <summary>
        /// Removes a card from the store.
        /// This happens when a card is traded or retracted.
        /// </summary>
        /// <param name="card">The card.</param>
        Task RemoveFromStore(ICard card);

        /// <summary>
        /// Adds a new package to the database.
        /// </summary>
        /// <param name="package">The package.</param>
        Task AddToPackages(CardPackage package);

        /// <summary>
        /// Gets a collection of all currently defined packages.
        /// </summary>
        /// <returns>List of packages.</returns>
        Task<ICollection<CardPackage>> GetPackages();
    }
}
