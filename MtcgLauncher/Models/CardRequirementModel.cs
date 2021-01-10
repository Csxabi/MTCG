using MtcgServer;
using System;

namespace MtcgLauncher.Models
{
    internal class CardRequirementModel
    {
        public RequirementType RequirementType { get; set; }

        public int? MinimumDamage { get; set; }

        public ElementType? RequiredElement { get; set; }
    }
}
