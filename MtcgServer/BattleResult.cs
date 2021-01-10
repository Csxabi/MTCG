using System;
using System.Collections.Generic;
using System.Text;

namespace MtcgServer
{
    /// <summary>
    /// Contains the result of a battle.
    /// </summary>
    public abstract class BattleResult
    {
        /// <summary>
        /// Contains descriptions for each step that happend during the battle.
        /// </summary>
        public IList<string> LogEntries { get; }

        protected BattleResult(IList<string> log)
            => LogEntries = log;

        /// <summary>
        /// Combines all entries in <see cref="LogEntries"/> into a single string.
        /// </summary>
        /// <returns>Combined log entries.</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();

            foreach (var entry in LogEntries)
                sb.AppendLine(entry);

            sb.Length -= Environment.NewLine.Length;
            return sb.ToString();
        }

        /// <summary>
        /// A battle result containing a winning and a losing player.
        /// </summary>
        public class Winner : BattleResult
        {
            public Player WinningPlayer { get; }

            public Player LosingPlayer { get; }

            public Winner(Player winningPlayer, Player losingPlayer, IList<string> log) : base(log)
            {
                WinningPlayer = winningPlayer;
                LosingPlayer = losingPlayer;
            }

            public override string ToString()
                => $"{WinningPlayer.Name} won this battle. {LosingPlayer.Name} lost this battle."
                    + Environment.NewLine + base.ToString();
        }

        /// <summary>
        /// A battle result caused by a draw.
        /// </summary>
        public class Draw : BattleResult
        {
            public Draw(IList<string> log) : base(log)
            {
            }

            public override string ToString()
                => "This battle resulted in a draw." + Environment.NewLine + base.ToString();
        }

        /// <summary>
        /// A battle that was cancelled.
        /// </summary>
        public class Cancelled : BattleResult
        {
            public Cancelled(string reason) : base(new List<string>() { reason })
            {
            }

            public override string ToString()
                => "The battle was cancelled: " + LogEntries[0];
        }
    }
}
