using System;

namespace MtcgServer.CardRequirements
{
    public class MinimumDamageRequirement : ICardRequirement
    {
        public int MinimumDamage { get; init; }

        public bool CheckRequirement(ICard card)
            => card.Damage >= MinimumDamage;

        public string RequirementAsString
            => $"Card must deal at least {MinimumDamage} damage.";
    }
}
