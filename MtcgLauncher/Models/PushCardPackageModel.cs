using System;

namespace MtcgLauncher.Models
{
    internal class PushCardPackageModel
    {
        public int Price { get; set; }

        public DefineCardModel[] Cards { get; set; } = Array.Empty<DefineCardModel>();
    }
}
