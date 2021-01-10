using System;

namespace MtcgServer.Cards.SpellCards
{
    public class FireSpell : SpellCard
    {
        public override ElementType ElementType => ElementType.Fire;

        public override int Damage { get; init; } = 20;
    }
}
