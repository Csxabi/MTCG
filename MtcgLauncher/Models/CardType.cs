using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace MtcgLauncher.Models
{
    [JsonConverter(typeof(StringEnumConverter))]
    internal enum CardType
    {
        Dragon,
        FireElf,
        Goblin,
        Knight,
        Kraken,
        Ork,
        Wizard,
        NormalSpell,
        WaterSpell,
        FireSpell
    }
}
