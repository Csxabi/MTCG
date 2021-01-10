using System;

namespace MtcgServer
{
    /// <summary>
    /// Tracks the changes made to a player.
    /// </summary>
    [Flags]
    public enum PlayerChange : ushort
    {
        // main values
        Name =          1 << 0,
        Password =      1 << 1,
        StatusText =    1 << 2,
        EmoticonText =  1 << 3,
        Stack =         1 << 4,
        Deck =          1 << 5,
        Coins =         1 << 6,
        ELO =           1 << 7,
        Wins =          1 << 8,
        Losses =        1 << 9,

        // combinations for usual events
        CreateAccount = Name | Password,
        AfterGame = ELO | Wins | Losses,
        AfterBuyPackage = Stack | Coins,
        AfterTrade = Stack,
        EditedProfile = Name | Password | StatusText | EmoticonText,

        // additional helpers
        None = 0,
        Everything = (1 << 10) - 1,
        UsersMask = Everything ^ Stack ^ Deck
    }
}
