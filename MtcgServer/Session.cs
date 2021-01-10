using System;

namespace MtcgServer
{
    public class Session
    {
        public Guid Token { get; }

        public Session()
            => Token = Guid.NewGuid();

        public Session(Guid token)
            => Token = token;

        public override bool Equals(object? other)
        {
            if (other is Session otherSession)
                return Token.Equals(otherSession.Token);

            return false;
        }

        public override int GetHashCode()
            => Token.GetHashCode();
    }
}
