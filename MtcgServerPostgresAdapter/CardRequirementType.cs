using System;

namespace MtcgServer.Databases.Postgres
{
    internal enum CardRequirementType
    {
        ElementType,
        IsMonsterCard,
        IsSpellCard,
        MinimumDamage
    }
}
