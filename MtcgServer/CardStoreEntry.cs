using System;
using System.Collections.Generic;

namespace MtcgServer
{
    public record CardStoreEntry(ICard Card, ICollection<ICardRequirement> Requirements);
}
