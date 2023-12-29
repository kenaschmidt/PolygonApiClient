using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PolygonApiClient
{
    public enum ConnectionEndpoint
    {
        stocks = 0,
        options = 2
    }
    public enum PolygonTimespan
    {
        second = 0,
        minute = 1,
        hour = 2,
        day = 3,
        week = 4,
        month = 5,
        quarter = 6,
        year = 7
    }
    public enum PolygonSort
    {
        asc = 0,
        desc = 1
    }
    public enum PolygonTickerSort
    {
        ticker = 0,
        name = 1,
        market = 2,
        locale = 3,
        primary_exchange = 4,
        type = 5,
        currency_symbol = 6,
        currency_name = 7,
        base_currency_symbol = 8,
        base_currency_name = 9,
        cik = 10,
        composite_figi = 11,
        share_class_figi = 12,
        last_updated_utc = 13,
        delisted_utc = 14
    }
    public enum PolygonOrder
    {
        asc = 0,
        desc = 1
    }
    public enum PolygonFilterParams
    {
        none = 0,
        gt = 1,
        gte = 2,
        lt = 3,
        lte = 4
    }
    public enum PolygonGainersLosers
    {
        losers = -1,
        NotSet = 0,
        gainers = 1
    }
    public enum PolygonAggregateSeriesType
    {
        close = 0,
        open = 1,
        high = 2,
        low = 3
    }
    public enum PolygonMarket
    {
        None = 0,
        stocks = 1,
        crypto = 2,
        fx = 3,
        otc = 4,
        indices = 5
    }

}
