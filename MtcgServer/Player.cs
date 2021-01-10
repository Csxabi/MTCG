using System;
using System.Collections.Generic;

namespace MtcgServer
{
    /// <summary>
    /// Represents a player, their inventory and stats.
    /// </summary>
    public record Player (Guid Id, string Name, byte[] PasswordHash, string StatusText, string EmoticonText,
        int Coins, ICollection<ICard> Stack, ICollection<ICard> Deck, int ELO, int Wins, int Losses)
    {
        // prevents serialization of password hash by Json.NET
        public bool ShouldSerializePasswordHash() => false;
    }
}
