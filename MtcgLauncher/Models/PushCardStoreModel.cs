using MtcgServer;
using System;
using System.Collections.Generic;

namespace MtcgLauncher.Models
{
    internal class PushCardStoreModel
    {
        public Guid CardId { get; set; }

        public CardRequirementModel[] Requirements { get; set; } = Array.Empty<CardRequirementModel>();
    }
}
