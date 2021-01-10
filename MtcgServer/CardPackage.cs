using System;
using System.Collections.Generic;

namespace MtcgServer
{
    public record CardPackage (Guid Id, int Price, IList<ICard> Cards);
}
