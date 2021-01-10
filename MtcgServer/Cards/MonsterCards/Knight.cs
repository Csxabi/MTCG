using System;

namespace MtcgServer.Cards.MonsterCards
{
    public class Knight : MonsterCard
    {
        public override ElementType ElementType => ElementType.Normal;

        public override int Damage { get; init; } = 20;
    }
}
