using System;

namespace MtcgLauncher.Models
{
    internal class CardTradeModel
    {
        public Guid OwnCard { get; set; }

        public Guid OtherCard { get; set; }
    }
}
