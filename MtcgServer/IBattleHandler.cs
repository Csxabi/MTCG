using System;

namespace MtcgServer
{
    /// <summary>
    /// Specifies a handler that is invoked for battles.
    /// </summary>
    public interface IBattleHandler
    {
        /// <summary>
        /// Executes one battle between two players.
        /// </summary>
        /// <param name="p1">First player.</param>
        /// <param name="p2">Second player.</param>
        /// <returns>Result of the battle.</returns>
        BattleResult RunBattle(Player p1, Player p2);
    }
}
