using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PolygonApiClient.ExtendedClient
{
    public enum TradeSide
    {
        sell = -1,
        unknown = 0,
        buy = 1
    }

    public enum QuoteType
    {
        bid = -1,
        midpoint = 0,
        ask = 1
    }
}
