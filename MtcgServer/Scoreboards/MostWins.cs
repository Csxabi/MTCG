using System;

namespace MtcgServer.Scoreboards
{
    public class MostWins : IScoreboard
    {
        public int Compare(Player? x, Player? y)
            => -x?.Wins.CompareTo(y?.Wins) ?? default;
    }
}
