using System;

namespace MtcgServer.CardRequirements
{
    public class IsSpellCardRequirement : ICardRequirement
    {
        public bool CheckRequirement(ICard card)
            => card is Cards.SpellCard;

        public string RequirementAsString
            => "Card must be a spell card.";
    }
}
