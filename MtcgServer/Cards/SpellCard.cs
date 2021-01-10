using MtcgServer.Cards.MonsterCards;
using System;

namespace MtcgServer.Cards
{
    public abstract class SpellCard : CardBase
    {
        protected override int _CalculateDamage(in ICard other)
            => other is Kraken
                ? 0
                : base._CalculateDamage(other);
    }
}
