using System;

namespace MtcgServer.Cards.MonsterCards
{
    public class Dragon : MonsterCard
    {
        public override ElementType ElementType => ElementType.Fire;

        public override int Damage { get; init; } = 20;

        protected override int _CalculateDamage(in ICard other)
            => other is FireElf
                ? 0
                : base._CalculateDamage(other);
    }
}
