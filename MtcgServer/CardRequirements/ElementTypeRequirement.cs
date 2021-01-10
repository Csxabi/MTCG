using System;

namespace MtcgServer.CardRequirements
{
    public class ElementTypeRequirement : ICardRequirement
    {
        public ElementType Type { get; init; }

        public bool CheckRequirement(ICard card)
            => card.ElementType == Type;

        public string RequirementAsString
            => $"Element type must be {Type}.";
    }
}
