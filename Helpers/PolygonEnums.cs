using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Threading.Tasks;

namespace PolygonApiClient
{
    public enum PolygonSubscription
    {
        basic = 0,
        starter = 1,
        developer = 2,
        advanced = 3
    }
    public enum PolygonConnectionEndpoint
    {
        stocks = 0,
        options = 1,
        indices = 2,
        forex = 3,
        crypto = 4
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
    public enum PolygonOrder
    {
        asc = 0,
        desc = 1
    }
    public enum PolygonFilterParams
    {
        NotSet = 0,
        gt = 1,
        gte = 2,
        lt = 3,
        lte = 4
    }
    public enum PolygonCompanyNameFilter
    {
        NotSet = 0,
        search = 1
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
    public enum PolygonLocale
    {
        None = 0,
        us = 1,
        global = 2
    }
    public enum PolygonDividendFrequency
    {
        NotSet = -1,
        one_time = 0,
        annually = 1,
        bi_annually = 2,
        quarterly = 4,
        monthly = 12
    }
    public enum PolygonDividendType
    {
        NotSet = 0,
        CD = 1,
        SC = 2,
        LT = 3,
        ST = 4
    }
    public enum PolygonSort
    {
        asc = 0,
        desc = 1
    }
    public enum PolygonTickerSort
    {
        NotSet = 0,
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
        delisted_utc = 14,
        ticker = 15
    }
    public enum PolygonStockSplitSort
    {
        NotSet = 0,
        execution_date = 1,
        ticker = 2
    }
    public enum PolygonDividendSort
    {
        NotSet = 0,
        ex_dividend_date = 1,
        pay_date = 2,
        declaration_date = 3,
        record_date = 4,
        cash_amount = 5,
        ticker = 6
    }
    public enum PolygonFinancialsSort
    {
        NotSet = 0,
        filing_date = 1,
        period_of_report_date = 2
    }
    public enum PolygonConditionsSort
    {
        NotSet = 0,
        asset_class = 1,
        id = 2,
        type = 3,
        name = 4,
        data_types = 5,
        legacy = 6
    }
    public enum PolygonOptionsChainSort
    {
        NotSet = 0,
        ticker = 1,
        expiration_date = 2,
        strike_Price = 3
    }
    public enum PolygonNewsSort
    {
        NotSet = 0,
        published_utc = 1
    }
    public enum PolygonOptionsContractsSort
    {
        NotSet = 0,
        ticker = 1,
        underlying_ticker = 2,
        expiration_date = 3,
        strike_Price = 4
    }
    public enum PolygonTradeQuoteSort
    {
        NotSet = 0,
        timestamp = 1
    }
    public enum PolygonFinancialsTimeframe
    {
        NotSet = 0,
        annual = 1,
        quarterly = 2,
        ttm = 3
    }
    public enum PolygonConditionCodeDataType
    {
        None = 0,
        trade = 1,
        bbo = 2,
        nbbo = 3
    }
    public enum PolygonSocketEvent
    {
        NotSet = 0,
        [Description("AggregateMinute")] AM = 1,
        [Description("AggregateSecond")] A = 2,
        [Description("Trade")] T = 3,
        [Description("Quote")] Q = 4,
        [Description("FairMarketValue")] FMV = 5
    }
}
