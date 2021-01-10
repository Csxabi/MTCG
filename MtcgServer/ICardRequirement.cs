using System;

namespace MtcgServer
{
    /// <summary>
    /// Specifies a requirement that must be met when trading a card.
    /// </summary>
    public interface ICardRequirement
    {
        /// <summary>
        /// Checks whether the card that is proposed for an exchange fulfills the requirement.
        /// </summary>
        /// <param name="card">Proposed card for exchange.</param>
        /// <returns>Whether the requirement was met or not.</returns>
        bool CheckRequirement(ICard card);

        /// <summary>
        /// Formulates the requirement as a sentence.
        /// </summary>
        string RequirementAsString { get; }
    }
}
