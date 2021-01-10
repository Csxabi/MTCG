using System;

namespace MtcgServer.Databases.Postgres
{
    internal struct CardRequirement
    {
        internal CardRequirementType ReqType;

        internal int ReqValue;
    }
}
