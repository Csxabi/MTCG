using System;

namespace MtcgServer.Cards.MonsterCards
{
    public class Goblin : MonsterCard
    {
        public override ElementType ElementType => ElementType.Normal;

        public override int Damage { get; init; } = 20;

        protected override int _CalculateDamage(in ICard other)
            => other is Dragon
                ? 0
                : base._CalculateDamage(other);
    }
}
