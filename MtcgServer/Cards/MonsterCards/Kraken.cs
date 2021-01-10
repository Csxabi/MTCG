using System;

namespace MtcgServer.Cards.MonsterCards
{
    public class Kraken : MonsterCard
    {
        public override ElementType ElementType => ElementType.Water;

        public override int Damage { get; init; } = 16;
    }
}
