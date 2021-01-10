using System;

namespace MtcgServer.Cards.SpellCards
{
    public class WaterSpell : SpellCard
    {
        public override ElementType ElementType => ElementType.Water;

        public override int Damage { get; init; } = 20;

        protected override int _CalculateDamage(in ICard other)
            => other is MonsterCards.Knight
                ? 9999 // drown knights instantly
                : base._CalculateDamage(other);
    }
}
