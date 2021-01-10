using System;

namespace MtcgServer.Scoreboards
{
    public class HighestELO : IScoreboard
    {
        public int Compare(Player? x, Player? y)
            => -x?.ELO.CompareTo(y?.ELO) ?? default;
    }
}
