using System;

namespace MtcgServer
{
    /// <summary>
    /// The common interface for all cards.
    /// </summary>
    public interface ICard
    {
        /// <summary>
        /// The unique ID of the card.
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// The element type of the card.
        /// </summary>
        ElementType ElementType { get; }

        /// <summary>
        /// Type of the card as string.
        /// </summary>
        string CardType { get; }

        /// <summary>
        /// How much damage this card deals.
        /// </summary>
        int Damage { get; }

        /// <summary>
        /// Calculates how much damage this card deals to a specific other card.
        /// </summary>
        /// <param name="other">The opponent card.</param>
        /// <returns>The dealt damage.</returns>
        int CalculateDamage(in ICard other);

        /// <summary>
        /// Creates a copy of this card with a different ID.
        /// </summary>
        /// <returns>Unique copy of this card.</returns>
        ICard CollissionlessDuplicate();
    }
}
