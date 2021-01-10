using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace MtcgLauncher.Models
{
    [JsonConverter(typeof(StringEnumConverter))]
    internal enum RequirementType
    {
        ElementType,
        IsMonsterCard,
        IsSpellCard,
        MinimumDamage
    }
}
