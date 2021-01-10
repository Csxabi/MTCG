using System;

namespace MtcgServer
{
    /// <summary>
    /// The shared base-implementation for all internally defined cards.
    /// </summary>
    public abstract class CardBase : ICard
    {
        private Guid _id;

        public Guid Id
        {
            get => _id;
            init => _id = value;
        }

        public abstract ElementType ElementType { get; }

        public abstract int Damage { get; init; }

        public string CardType => GetType().Name;

        public int CalculateDamage(in ICard other)
            => this is Cards.MonsterCard && other is Cards.MonsterCard
                ? _CalculateDamage(other) : (ElementType, other.ElementType) switch
                {
                    (ElementType.Water,  ElementType.Fire)   => _CalculateDamage(other) * 2,
                    (ElementType.Fire,   ElementType.Water)  => _CalculateDamage(other) / 2,
                    (ElementType.Fire,   ElementType.Normal) => _CalculateDamage(other) * 2,
                    (ElementType.Normal, ElementType.Fire)   => _CalculateDamage(other) / 2,
                    (ElementType.Normal, ElementType.Water)  => _CalculateDamage(other) * 2,
                    (ElementType.Water,  ElementType.Normal) => _CalculateDamage(other) / 2,
                    _ => _CalculateDamage(other)
                };

        public ICard CollissionlessDuplicate()
        {
            var copy = (CardBase)MemberwiseClone();
            copy._id = Guid.NewGuid();
            return copy;
        }

        protected virtual int _CalculateDamage(in ICard other)
            => Damage;

        public override string ToString()
            => GetType().Name + " (" + ElementType + ")";
    }
}
