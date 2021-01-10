using System;

namespace MtcgServer.Scoreboards
{
    public class LeastLosses : IScoreboard
    {
        public int Compare(Player? x, Player? y)
            => x?.Losses.CompareTo(y?.Losses) ?? default;
    }
}
