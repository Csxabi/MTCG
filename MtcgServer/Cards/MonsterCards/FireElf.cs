using System;

namespace MtcgServer.Cards.MonsterCards
{
    public class FireElf : MonsterCard
    {
        public override ElementType ElementType => ElementType.Fire;

        public override int Damage { get; init; } = 12;
    }
}
