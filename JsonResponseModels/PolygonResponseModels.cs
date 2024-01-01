using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace PolygonApiClient
{
    // ------ Notes ------
    //
    // 1) Most 'Response' objects inherit common fields from Rest_Response, but due to inconsistency in endpoint return values, not all fields will be populated for all requests, and 
    //    several response objects hide base fields due to differing naming conventions (example: some endpoints return 'count' and some return 'resultsCount'. Some V1 Responses do not
    //    follow this pattern and return all fields in the Response object itself (see: Market Holidays, Market Status)
    //
    // 2) Not a design factor but please pay attention to timestamp suffixes, as some are returned as millisecond and some are returned as nanosecond.
    //
    // 3) All fields are tagged with JsonProperty attributes with their actual return name values, since many are not intuitive and some returned values differ in naming despite being the
    //    same value. Property names for the same values are kept mostly consistent between objects and consistent with JSON naming when no clarification is needed. One exception is 'Ticker'
    //    which has been changed to 'Symbol' [almost] everywhere for consistency.
    //
    // 4) No conversion is done here for Dates, Enums, etc. Handle elsewhere.
    //
    // 5) The 'Stock Financials' objects don't follow the same pattern, since there are so many returned objects and they are all named intuitively already, and it didn't make sense to rename everything. I used a 
    //    quick converter to generate the C# classes.
    //
    // -------------------


    //
    // Standard fields for most REST response objects; not all fields may be used in all requests. <T> defines the type of the 'Results' field
    //
    public abstract class Rest_Response<T>
    {
        [JsonProperty("count")]
        public int Count { get; set; }

        [JsonProperty("request_id")]
        public string Request_ID { get; set; }

        [JsonProperty("next_url")]
        public string Next_URL { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("results")]
        public virtual T Results { get; set; }
    }

    #region REST Market Data Endpoint Objects

    //
    // Stock Aggregate Bars
    //
    public class RestAggregatesBars_Response : Rest_Response<RestAggregatesBars_Result[]>
    {
        [JsonProperty("ticker")]
        public string Ticker { get; set; }

        [JsonProperty("queryCount")]
        public int QueryCount { get; set; }

        [JsonProperty("resultsCount")]
        public new int Count { get; set; }

        [JsonProperty("adjusted")]
        public bool Adjusted { get; set; }

    }
    public class RestAggregatesBars_Result
    {
        [JsonProperty("o")]
        public double Open { get; set; }

        [JsonProperty("h")]
        public double High { get; set; }

        [JsonProperty("l")]
        public double Low { get; set; }

        [JsonProperty("c")]
        public double Close { get; set; }

        [JsonProperty("v")]
        public double Volume { get; set; }

        [JsonProperty("n")]
        public int Number_Of_Trades { get; set; }

        [JsonProperty("t")]
        public long Timestamp_Start_Ms { get; set; }

        [JsonProperty("vw")]
        public double VWAP { get; set; }

        [JsonProperty("otc")]
        public bool IsOTC { get; set; }
    }

    //
    // Stock Grouped Daily Bars
    //
    public class RestGroupedDailyBars_Response : Rest_Response<RestGroupedDailyBars_Result[]>
    {
        [JsonProperty("adjusted")]
        public bool Adjusted { get; set; }

        [JsonProperty("queryCount")]
        public int QueryCount { get; set; }

        [JsonProperty("resultsCount")]
        public new int Count { get; set; }
    }
    public class RestGroupedDailyBars_Result
    {
        [JsonProperty("T")]
        public string Symbol { get; set; }

        [JsonProperty("o")]
        public double Open { get; set; }

        [JsonProperty("h")]
        public double High { get; set; }

        [JsonProperty("l")]
        public double Low { get; set; }

        [JsonProperty("c")]
        public double Close { get; set; }

        [JsonProperty("v")]
        public double Volume { get; set; }

        [JsonProperty("n")]
        public int Number_Of_Trades { get; set; }

        [JsonProperty("t")]
        public long Timestamp_End_Ms { get; set; }

        [JsonProperty("vw")]
        public double VWAP { get; set; }

        [JsonProperty("otc")]
        public bool IsOTC { get; set; }
    }

    //
    // Stock Daily Open/Close
    //
    public class RestDailyOpenClose_Result
    {
        [JsonProperty("afterhours")]
        public double AfterHoursClose { get; set; }

        [JsonProperty("close")]
        public double Close { get; set; }

        [JsonProperty("from")]
        public string RequestDate { get; set; }

        [JsonProperty("high")]
        public double High { get; set; }

        [JsonProperty("low")]
        public double Low { get; set; }

        [JsonProperty("open")]
        public double Open { get; set; }

        [JsonProperty("otc")]
        public bool IsOTC { get; set; }

        [JsonProperty("preMarket")]
        public double PremarketOpen { get; set; }

        [JsonProperty("symbol")]
        public string Symbol { get; set; }

        [JsonProperty("volume")]
        public long Volume { get; set; }
    }

    //
    // Stock Previous Close
    //
    public class RestPreviousClose_Response : Rest_Response<RestPreviousClose_Result[]>
    {
        [JsonProperty("adjusted")]
        public bool Adjusted { get; set; }

        [JsonProperty("queryCount")]
        public int QueryCount { get; set; }

        [JsonProperty("resultsCount")]
        public new int Count { get; set; }

        [JsonProperty("ticker")]
        public string Symbol { get; set; }
    }
    public class RestPreviousClose_Result
    {
        [JsonProperty("T")]
        public string Symbol { get; set; }

        [JsonProperty("o")]
        public double Open { get; set; }

        [JsonProperty("h")]
        public double High { get; set; }

        [JsonProperty("l")]
        public double Low { get; set; }

        [JsonProperty("c")]
        public double Close { get; set; }

        [JsonProperty("v")]
        public double Volume { get; set; }

        [JsonProperty("n")]
        public int Number_Of_Trades { get; set; }

        [JsonProperty("t")]
        public long Timestamp_Start_Ms { get; set; }

        [JsonProperty("vw")]
        public double VWAP { get; set; }
    }

    //
    // Stock Trades
    //
    public class RestTrades_Response : Rest_Response<RestTrades_Result[]>
    {
    }
    public class RestTrades_Result
    {
        [JsonProperty("conditions")]
        public int[] Conditions { get; set; }

        [JsonProperty("correction")]
        public int Correction { get; set; }

        [JsonProperty("exchange")]
        public int Exchange { get; set; }

        [JsonProperty("id")]
        public string Trade_ID { get; set; } // Only for Stocks

        [JsonProperty("participant_timestamp")]
        public long Participant_Timestamp_Ns { get; set; }

        [JsonProperty("price")]
        public double Price { get; set; }

        [JsonProperty("sequence_number")]
        public long Sequence_Number { get; set; } // Only for Stocks

        [JsonProperty("sip_timestamp")]
        public long SIP_Timestamp_Ns { get; set; }

        [JsonProperty("trf_timestamp")]
        public long TRF_Timestamp_Ns { get; set; }

        [JsonProperty("trf_id")]
        public int TRF_ID { get; set; }

        [JsonProperty("size")]
        public Int64 Size { get; set; }

        [JsonProperty("tape")]
        public int Tape { get; set; } // Only for Stocks
    }

    //
    // Stock Last Trade
    //
    public class RestLastTrade_Response : Rest_Response<RestLastTrade_Result>
    {
    }
    public class RestLastTrade_Result
    {
        [JsonProperty("T")]
        public string Symbol { get; set; }

        [JsonProperty("c")]
        public int[] Conditions { get; set; }

        [JsonProperty("e")]
        public int Correction { get; set; }

        [JsonProperty("f")]
        public long TRF_Timestamp_Ns { get; set; }

        [JsonProperty("y")]
        public long Participant_Timestamp_Ns { get; set; }

        [JsonProperty("t")]
        public long SIP_Timestamp_Ns { get; set; }

        [JsonProperty("x")]
        public int Exchange { get; set; }

        [JsonProperty("i")]
        public string Trade_ID { get; set; } // Only for Stocks

        [JsonProperty("p")]
        public double Price { get; set; }

        [JsonProperty("q")]
        public long Sequence_Number { get; set; } // Only for Stocks

        [JsonProperty("s")]
        public int Size { get; set; }

        [JsonProperty("z")]
        public int Tape { get; set; } // Only for Stocks

        [JsonProperty("r")]
        public int TRF_ID { get; set; }
    }

    //
    // Stock Quotes
    //
    public class RestQuotes_Response : Rest_Response<RestQuotes_Result[]>
    {
    }
    public class RestQuotes_Result
    {
        [JsonProperty("ask_exchange")]
        public int Ask_Exchange { get; set; }

        [JsonProperty("ask_price")]
        public double Ask_Price { get; set; }

        [JsonProperty("ask_size")]
        public double Ask_Size_Lots { get; set; }

        [JsonProperty("bid_exchange")]
        public int Bid_Exchange { get; set; }

        [JsonProperty("bid_price")]
        public double Bid_Price { get; set; }

        [JsonProperty("bid_size")]
        public double Bid_Size_Lots { get; set; }

        [JsonProperty("conditions")]
        public int[] Conditions { get; set; }

        [JsonProperty("indicators")]
        public int[] Indicators { get; set; }

        [JsonProperty("participant_timestamp")]
        public long Participant_Timestamp_Ns { get; set; }

        [JsonProperty("sip_timestamp")]
        public long SIP_Timestamp_Ns { get; set; }

        [JsonProperty("sequence_number")]
        public long Sequence_Number { get; set; }

        [JsonProperty("tape")]
        public int Tape { get; set; }

        [JsonProperty("trf_timestamp")]
        public long TRF_Timestamp_Ns { get; set; }
    }

    //
    // Stock Last Quote
    //
    public class RestLastQuote_Response : Rest_Response<RestLastQuote_Result>
    {
    }
    public class RestLastQuote_Result
    {
        [JsonProperty("P")]
        public double Ask_Price { get; set; }

        [JsonProperty("S")]
        public int Ask_Size_Lots { get; set; }

        [JsonProperty("T")]
        public string Symbol { get; set; }

        [JsonProperty("X")]
        public int Ask_Exchange { get; set; }

        [JsonProperty("c")]
        public int[] Conditions { get; set; }

        [JsonProperty("f")]
        public long TRF_Timestamp_Ns { get; set; }

        [JsonProperty("i")]
        public int[] Indicators { get; set; }

        [JsonProperty("p")]
        public double Bid_Price { get; set; }

        [JsonProperty("s")]
        public int Bid_Size_Lots { get; set; }

        [JsonProperty("q")]
        public long Sequence_Number { get; set; }

        [JsonProperty("t")]
        public long SIP_Timestamp_Ns { get; set; }

        [JsonProperty("x")]
        public int Bid_Exchange { get; set; }

        [JsonProperty("y")]
        public long Participant_Timestamp_Ns { get; set; }

        [JsonProperty("z")]
        public int Tape { get; set; }
    }

    //
    // Snapshots - These models are used for 'All Tickers', 'Gainers/Losers', and 'Ticker'
    //
    public class RestTickerSnapshot_Response : Rest_Response<RestTickerSnapshot_Result[]>
    {
        [JsonProperty("tickers")]
        public override RestTickerSnapshot_Result[] Results { get; set; }
    }
    public class RestTickerSnapshot_Result
    {
        [JsonProperty("ticker")]
        public string Symbol { get; set; }

        [JsonProperty("todaysChange")]
        public double Todays_Change { get; set; }

        [JsonProperty("todaysChangePerc")]
        public double Todays_Change_Percent { get; set; }

        [JsonProperty("updated")]
        public long Last_Update_Timestamp_Ns { get; set; }

        [JsonProperty("fmv")]
        public double FairMarketValue { get; set; } // Only returned on Business plans

        [JsonProperty("day")]
        public RestTickerSnapshot_Day Last_Daily_Bar { get; set; }

        [JsonProperty("lastQuote")]
        public RestTickerSnapshot_LastQuote Last_Quote { get; set; }

        [JsonProperty("lastTrade")]
        public RestTickerSnapshot_LastTrade Last_Trade { get; set; }

        [JsonProperty("min")]
        public RestTickerSnapshot_LastMinuteBar Last_Minute_Bar { get; set; }

        [JsonProperty("prevDay")]
        public RestTickerSnapshot_PrevDayBar Previous_Day_Bar { get; set; }
    }
    public class RestTickerSnapshot_Day
    {
        [JsonProperty("o")]
        public double Open { get; set; }

        [JsonProperty("h")]
        public double High { get; set; }

        [JsonProperty("l")]
        public double Low { get; set; }

        [JsonProperty("c")]
        public double Close { get; set; }

        [JsonProperty("v")]
        public double Volume { get; set; }

        [JsonProperty("vw")]
        public double VWAP { get; set; }

        [JsonProperty("otc")]
        public bool IsOTC { get; set; }
    }
    public class RestTickerSnapshot_LastQuote
    {
        [JsonProperty("P")]
        public double Ask_Price { get; set; }

        [JsonProperty("S")]
        public int Ask_Size_Lots { get; set; }

        [JsonProperty("p")]
        public double Bid_Price { get; set; }

        [JsonProperty("s")]
        public int Bid_Size_Lots { get; set; }

        [JsonProperty("t")]
        public long SIP_Timestamp_Ns { get; set; }
    }
    public class RestTickerSnapshot_LastTrade
    {
        [JsonProperty("c")]
        public int[] Conditions { get; set; }

        [JsonProperty("t")]
        public long SIP_Timestamp_Ns { get; set; }

        [JsonProperty("x")]
        public int Exchange { get; set; }

        [JsonProperty("i")]
        public string Trade_ID { get; set; } // Only for Stocks

        [JsonProperty("p")]
        public double Price { get; set; }

        [JsonProperty("s")]
        public int Size { get; set; }

    }
    public class RestTickerSnapshot_LastMinuteBar
    {
        [JsonProperty("o")]
        public double Open { get; set; }

        [JsonProperty("h")]
        public double High { get; set; }

        [JsonProperty("l")]
        public double Low { get; set; }

        [JsonProperty("c")]
        public double Close { get; set; }

        [JsonProperty("v")]
        public double Volume { get; set; }

        [JsonProperty("av")]
        public int Accumulated_Volume { get; set; }

        [JsonProperty("n")]
        public int Number_Of_Trades { get; set; }

        [JsonProperty("t")]
        public long Timestamp_Start_Ms { get; set; }

        [JsonProperty("vw")]
        public double VWAP { get; set; }

        [JsonProperty("otc")]
        public bool IsOTC { get; set; }
    }
    public class RestTickerSnapshot_PrevDayBar
    {
        [JsonProperty("o")]
        public double Open { get; set; }

        [JsonProperty("h")]
        public double High { get; set; }

        [JsonProperty("l")]
        public double Low { get; set; }

        [JsonProperty("c")]
        public double Close { get; set; }

        [JsonProperty("v")]
        public double Volume { get; set; }

        [JsonProperty("vw")]
        public double VWAP { get; set; }

        [JsonProperty("otc")]
        public bool IsOTC { get; set; }
    }

    //
    // Needed to designate a few 
    //
    public interface IResult_Special { }

    // 
    // Technical Indicators - SMA and EMA
    //
    public class RestMovingAverage_Response : Rest_Response<RestMovingAverage_Result>, IResult_Special
    {
    }
    public class RestMovingAverage_Result
    {
        [JsonProperty("underlying")]
        public RestTechnicalIndicator_Underlying Underlying_Aggregates { get; set; }

        [JsonProperty("values")]
        public RestMovingAverage_Values[] Indicator_Values { get; set; }
    }
    public class RestMovingAverage_Values
    {
        [JsonProperty("timestamp")]
        public long Timestamp_Start_Ms { get; set; }

        [JsonProperty("value")]
        public double Indicator_Value { get; set; }
    }

    //
    // Technical Indicators - MACD
    //
    public class RestMACD_Response : Rest_Response<RestMACD_Result>, IResult_Special
    {
    }
    public class RestMACD_Result
    {
        [JsonProperty("underlying")]
        public RestTechnicalIndicator_Underlying Underlying_Aggregates { get; set; }

        [JsonProperty("values")]
        public RestMACD_Values[] Indicator_Values { get; set; }
    }
    public class RestMACD_Values
    {
        [JsonProperty("timestamp")]
        public long Timestamp_Start_Ms { get; set; }

        [JsonProperty("value")]
        public double MACD_Value { get; set; }

        [JsonProperty("signal")]
        public double Signal_Value { get; set; }

        [JsonProperty("histogram")]
        public double Histogram_Value { get; set; }
    }

    //
    // Technical Indictor - Relative Strength Index
    //
    public class RestRSI_Response : Rest_Response<RestRSI_Result>, IResult_Special
    {
    }
    public class RestRSI_Result
    {
        [JsonProperty("underlying")]
        public RestTechnicalIndicator_Underlying Underlying_Aggregates { get; set; }

        [JsonProperty("values")]
        public RestRSI_Values[] Indicator_Values { get; set; }
    }
    public class RestRSI_Values
    {
        [JsonProperty("timestamp")]
        public long Timestamp_Start_Ms { get; set; }

        [JsonProperty("value")]
        public double MACD_Value { get; set; }
    }

    //
    // Technical Indicators common structures
    //
    public class RestTechnicalIndicator_Underlying
    {
        [JsonProperty("aggregates")]
        public RestTechnicalIndicator_Aggregates[] Aggregates { get; set; }

        [JsonProperty("url")]
        public string URL_Aggregates_Request { get; set; }
    }
    public class RestTechnicalIndicator_Aggregates
    {
        [JsonProperty("T")]
        public string Symbol { get; set; }

        [JsonProperty("o")]
        public double Open { get; set; }

        [JsonProperty("h")]
        public double High { get; set; }

        [JsonProperty("l")]
        public double Low { get; set; }

        [JsonProperty("c")]
        public double Close { get; set; }

        [JsonProperty("v")]
        public double Volume { get; set; }

        [JsonProperty("n")]
        public int Number_Of_Trades { get; set; }

        [JsonProperty("t")]
        public long Timestamp_Start_Ms { get; set; }

        [JsonProperty("vw")]
        public double VWAP { get; set; }

        [JsonProperty("otc")]
        public bool IsOTC { get; set; }
    }
    #endregion

    #region REST Reference Data Endpoint Objects

    //
    // Tickers
    //
    public class RestTickers_Response : Rest_Response<RestTickers_Result[]>
    {
    }
    public class RestTickers_Result
    {
        [JsonProperty("active")]
        public bool Active { get; set; }

        [JsonProperty("cik")]
        public string CIK { get; set; }

        [JsonProperty("composite_figi")]
        public string Composite_FIGI { get; set; }

        [JsonProperty("currency_name")]
        public string Currency_Name { get; set; }

        [JsonProperty("delisted_utc")]
        public string Delisted_UTC { get; set; }

        [JsonProperty("last_updated_utc")]
        public string Last_Updated_UTC { get; set; }

        [JsonProperty("locale")]
        public string Locale { get; set; }

        [JsonProperty("market")]
        public string Market { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("primary_exchange")]
        public string Primary_Exchange { get; set; }

        [JsonProperty("share_class_figi")]
        public string Share_Class_FIGI { get; set; }

        [JsonProperty("ticker")]
        public string Symbol { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }

    //
    // Ticker V3 - Detailed
    //
    public class RestTickerDetail_Response : Rest_Response<RestTickerDetail_Result>
    {
    }
    public class RestTickerDetail_Result
    {
        [JsonProperty("active")]
        public bool Active { get; set; }

        [JsonProperty("address")]
        public RestTickerDetail_Address Address { get; set; }

        [JsonProperty("branding")]
        public RestTickerDetails_Branding Branding { get; set; }

        [JsonProperty("cik")]
        public string CIK { get; set; }

        [JsonProperty("composite_figi")]
        public string Composite_FIGI { get; set; }

        [JsonProperty("currency_name")]
        public string Currency_Name { get; set; }

        [JsonProperty("delisted_utc")]
        public string Delisted_UTC { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("homepage_url")]
        public string Homepage_URL { get; set; }

        [JsonProperty("list_date")]
        public string List_Date { get; set; }

        [JsonProperty("locale")]
        public string Locale { get; set; }

        [JsonProperty("market")]
        public string Market { get; set; }

        [JsonProperty("market_cap")]
        public long Market_Cap { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("phone_number")]
        public string Phone_Number { get; set; }

        [JsonProperty("primary_exchange")]
        public string Primary_Exchange { get; set; }

        [JsonProperty("round_lot")]
        public int Round_Lot_Size { get; set; }

        [JsonProperty("share_class_figi")]
        public string Share_Class_FIGI { get; set; }

        [JsonProperty("share_class_shares_outstanding")]
        public int Share_Class_Shares_Outstanding { get; set; }

        [JsonProperty("sic_code")]
        public string SIC_Code { get; set; }

        [JsonProperty("sic_description")]
        public string SIC_Description { get; set; }

        [JsonProperty("ticker")]
        public string Symbol { get; set; }

        [JsonProperty("ticker_root")]
        public string Symbol_Root { get; set; }

        [JsonProperty("ticker_suffix")]
        public string Symbol_Suffix { get; set; }

        [JsonProperty("total_employees")]
        public int Total_Employees { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("weighted_shares_outstanding")]
        public int Weighted_Shares_Outstanding { get; set; }
    }
    public class RestTickerDetail_Address
    {
        [JsonProperty("address1")]
        public string StreetAddress { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("postal_code")]
        public string PostalCode { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }
    }
    public class RestTickerDetails_Branding
    {
        [JsonProperty("icon_url")]
        public string Icon_URL { get; set; }

        [JsonProperty("logo_url")]
        public string Logo_URL { get; set; }
    }

    //
    // Ticker Events (Experimental)
    //
    public class RestTickerEvents_Response : Rest_Response<RestTickerEvents_Result>
    {
    }
    public class RestTickerEvents_Result
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("events")]
        public RestTickerEvents_Events[] Events { get; set; }
    }
    public class RestTickerEvents_Events
    {
        // Undefined
    }

    //
    // Ticker News
    //
    public class RestTickerNews_Response : Rest_Response<RestTickerNews_Result[]>
    {
    }
    public class RestTickerNews_Result
    {
        [JsonProperty("amp_url")]
        public string AcceleratedMobilePage_URL { get; set; }

        [JsonProperty("article_url")]
        public string Article_URL { get; set; }

        [JsonProperty("author")]
        public string Author { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("id")]
        public string ID { get; set; }

        [JsonProperty("image_url")]
        public string Image_URL { get; set; }

        [JsonProperty("keywords")]
        public string[] Keywords { get; set; }

        [JsonProperty("published_utc")]
        public string Published_UTC { get; set; }

        [JsonProperty("publisher")]
        public RestTickerNews_Publisher Publisher { get; set; }

        [JsonProperty("tickers")]
        public string[] Tickers { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

    }
    public class RestTickerNews_Publisher
    {
        [JsonProperty("favicon_url")]
        public string Favicon_URL { get; set; }

        [JsonProperty("homepage_url")]
        public string Homepage_URL { get; set; }

        [JsonProperty("logo_url")]
        public string Logo_URL { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    //
    // Ticker Types
    //
    public class RestTickerTypes_Response : Rest_Response<RestTickerTypes_Result[]>
    {
    }
    public class RestTickerTypes_Result
    {

        [JsonProperty("asset_class")]
        public string Asset_Class { get; set; }

        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("locale")]
        public string Locale { get; set; }
    }

    //
    // Market Holidays
    //
    public class RestMarketHolidays_Result
    {
        [JsonProperty("close")]
        public string Closing_Time { get; set; }

        [JsonProperty("date")]
        public string Date { get; set; }

        [JsonProperty("exchange")]
        public string Exchange { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("open")]
        public string Opening_Time { get; set; }

        [JsonProperty("status")]
        public string Market_Status { get; set; }
    }

    //
    // Market Status
    //
    public class RestMarketStatus_Response
    {
        [JsonProperty("afterHours")]
        public bool Is_After_Hours { get; set; }

        [JsonProperty("earlyHours")]
        public bool Is_Premarket { get; set; }

        [JsonProperty("currencies")]
        public RestMarketStatus_Currencies Currency_Markets_Status { get; set; }

        [JsonProperty("exchanges")]
        public RestMarketStatus_Exchanges Exchanges { get; set; }

        [JsonProperty("indicesGroups")]
        public RestMarketStatus_IndicesGroups Indices { get; set; }

        [JsonProperty("market")]
        public string Market_Status { get; set; }

        [JsonProperty("mserverTime")]
        public string Server_Time { get; set; }

    }
    public class RestMarketStatus_Currencies
    {

        [JsonProperty("crypto")]
        public string Crypto { get; set; }

        [JsonProperty("fx")]
        public string FOREX { get; set; }
    }
    public class RestMarketStatus_Exchanges
    {

        [JsonProperty("nasdaq")]
        public string NASDAQ { get; set; }

        [JsonProperty("nyse")]
        public string NYSE { get; set; }

        [JsonProperty("otc")]
        public string OTC { get; set; }
    }
    public class RestMarketStatus_IndicesGroups
    {

        [JsonProperty("s_and_p")]
        public string S_and_P { get; set; }

        [JsonProperty("societe_generale")]
        public string Societe_Generale { get; set; }

        [JsonProperty("msci")]
        public string MSCI { get; set; }

        [JsonProperty("ftse_russell")]
        public string FTSE_Russell { get; set; }

        [JsonProperty("mstar")]
        public string Morningstar { get; set; }

        [JsonProperty("mstarc")]
        public string Morningstar_Customer { get; set; }

        [JsonProperty("cccy")]
        public string CBOE_Crypto { get; set; }

        [JsonProperty("nasdaq")]
        public string NASDAQ { get; set; }

        [JsonProperty("dow_jones")]
        public string Dow_Jones { get; set; }
    }

    //
    // Stock Splits V3
    //
    public class RestStockSplits_Response : Rest_Response<RestStockSplits_Result[]>
    {
    }
    public class RestStockSplits_Result
    {
        [JsonProperty("execution_date")]
        public string Execution_Date { get; set; }

        [JsonProperty("split_from")]
        public double Split_From { get; set; }

        [JsonProperty("split_to")]
        public double Split_To { get; set; }

        [JsonProperty("ticker")]
        public string Symbol { get; set; }
    }

    //
    // Dividends V3
    //
    public class RestDividends_Response : Rest_Response<RestDividends_Result[]>
    {
    }
    public class RestDividends_Result
    {
        [JsonProperty("cash_amount")]
        public double Cash_Amount { get; set; }

        [JsonProperty("declaration_date")]
        public string Declaration_Date { get; set; }

        [JsonProperty("dividend_type")]
        public string Dividend_Type { get; set; }

        [JsonProperty("ex_dividend_date")]
        public string Ex_Dividend_Date { get; set; }

        [JsonProperty("frequency")]
        public int Frequency { get; set; }

        [JsonProperty("pay_date")]
        public string Pay_Date { get; set; }

        [JsonProperty("record_date")]
        public string Record_Date { get; set; }

        [JsonProperty("ticker")]
        public string Symbol { get; set; }
    }

    //
    // Stock Financials (Experimental)
    //
    public class RestStockFinancials_Response : Rest_Response<RestStockFinancials_Result[]>
    {
    }
    public class RestStockFinancials_Result
    {
        public string cik { get; set; }
        public string company_name { get; set; }
        public string end_date { get; set; }
        public string filing_date { get; set; }
        public Financials financials { get; set; }
        public string fiscal_period { get; set; }
        public string fiscal_year { get; set; }
        public string source_filing_file_url { get; set; }
        public string source_filing_url { get; set; }
        public string start_date { get; set; }
    }
    public class Financials
    {
        public Balance_sheet balance_sheet { get; set; }
        public Cash_flow_statement cash_flow_statement { get; set; }
        public Comprehensive_income comprehensive_income { get; set; }
        public Income_statement income_statement { get; set; }
    }
    public class Balance_sheet
    {
        public RestStockFinancials_Datapoint assets { get; set; }
        public RestStockFinancials_Datapoint current_assets { get; set; }
        public RestStockFinancials_Datapoint current_liabilities { get; set; }
        public RestStockFinancials_Datapoint equity { get; set; }
        public RestStockFinancials_Datapoint equity_attributable_to_noncontrolling_interest { get; set; }
        public RestStockFinancials_Datapoint equity_attributable_to_parent { get; set; }
        public RestStockFinancials_Datapoint liabilities { get; set; }
        public RestStockFinancials_Datapoint liabilities_and_equity { get; set; }
        public RestStockFinancials_Datapoint noncurrent_assets { get; set; }
        public RestStockFinancials_Datapoint noncurrent_liabilities { get; set; }
    }
    public class Cash_flow_statement
    {
        public RestStockFinancials_Datapoint exchange_gains_losses { get; set; }
        public RestStockFinancials_Datapoint net_cash_flow { get; set; }
        public RestStockFinancials_Datapoint net_cash_flow_continuing { get; set; }
        public RestStockFinancials_Datapoint net_cash_flow_from_financing_activities { get; set; }
        public RestStockFinancials_Datapoint net_cash_flow_from_financing_activities_continuing { get; set; }
        public RestStockFinancials_Datapoint net_cash_flow_from_investing_activities { get; set; }
        public RestStockFinancials_Datapoint net_cash_flow_from_investing_activities_continuing { get; set; }
        public RestStockFinancials_Datapoint net_cash_flow_from_operating_activities { get; set; }
        public RestStockFinancials_Datapoint net_cash_flow_from_operating_activities_continuing { get; set; }
    }
    public class Comprehensive_income
    {
        public RestStockFinancials_Datapoint comprehensive_income_loss { get; set; }
        public RestStockFinancials_Datapoint comprehensive_income_loss_attributable_to_noncontrolling_interest { get; set; }
        public RestStockFinancials_Datapoint comprehensive_income_loss_attributable_to_parent { get; set; }
        public RestStockFinancials_Datapoint other_comprehensive_income_loss { get; set; }
        public RestStockFinancials_Datapoint other_comprehensive_income_loss_attributable_to_parent { get; set; }
    }
    public class Income_statement
    {
        public RestStockFinancials_Datapoint basic_earnings_per_share { get; set; }
        public RestStockFinancials_Datapoint benefits_costs_expenses { get; set; }
        public RestStockFinancials_Datapoint cost_of_revenue { get; set; }
        public RestStockFinancials_Datapoint costs_and_expenses { get; set; }
        public RestStockFinancials_Datapoint diluted_earnings_per_share { get; set; }
        public RestStockFinancials_Datapoint gross_profit { get; set; }
        public RestStockFinancials_Datapoint income_loss_from_continuing_operations_after_tax { get; set; }
        public RestStockFinancials_Datapoint income_loss_from_continuing_operations_before_tax { get; set; }
        public RestStockFinancials_Datapoint income_tax_expense_benefit { get; set; }
        public RestStockFinancials_Datapoint interest_expense_operating { get; set; }
        public RestStockFinancials_Datapoint net_income_loss { get; set; }
        public RestStockFinancials_Datapoint net_income_loss_attributable_to_noncontrolling_interest { get; set; }
        public RestStockFinancials_Datapoint net_income_loss_attributable_to_parent { get; set; }
        public RestStockFinancials_Datapoint net_income_loss_available_to_common_stockholders_basic { get; set; }
        public RestStockFinancials_Datapoint operating_expenses { get; set; }
        public RestStockFinancials_Datapoint operating_income_loss { get; set; }
        public RestStockFinancials_Datapoint participating_securities_distributed_and_undistributed_earnings_loss_basic { get; set; }
        public RestStockFinancials_Datapoint preferred_stock_dividends_and_other_adjustments { get; set; }
        public RestStockFinancials_Datapoint revenues { get; set; }
    }

    public class RestStockFinancials_Datapoint
    {
        public string label { get; set; }
        public int order { get; set; }
        public string unit { get; set; }
        public Int64 value { get; set; }
        public string source { get; set; }
        public string[] derived_from { get; set; }
    }

    //
    // Conditions
    //
    public class RestConditions_Response : Rest_Response<RestConditions_Result[]>
    {
    }
    public class RestConditions_Result
    {
        [JsonProperty("abbreviation")]
        public string Abbreviation { get; set; }

        [JsonProperty("asset_class")]
        public string Asset_Class { get; set; }

        [JsonProperty("data_types")]
        public string[] Data_Types { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("exchange")]
        public int Exchange { get; set; }

        [JsonProperty("id")]
        public int ID { get; set; }

        [JsonProperty("legacy")]
        public bool Legacy { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("sip_mapping")]
        public RestConditions_SipMapping SIP_Mapping { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("update_rules")]
        public RestConditions_UpdateRules Update_Rules { get; set; }

    }
    public class RestConditions_SipMapping
    {
        public string CTA { get; set; }
        public string UTP { get; set; }
        public string OPRA { get; set; }
    }
    public class RestConditions_UpdateRules
    {
        public RestConditions_Consolidated consolidated { get; set; }
        public RestConditions_MarketCenter market_center { get; set; }
    }
    public class RestConditions_Consolidated
    {
        public bool updates_high_low { get; set; }
        public bool updates_open_close { get; set; }
        public bool updates_volume { get; set; }
    }
    public class RestConditions_MarketCenter
    {
        public bool updates_high_low { get; set; }
        public bool updates_open_close { get; set; }
        public bool updates_volume { get; set; }
    }

    //
    // Exchanges
    //
    public class RestExchange_Response : Rest_Response<RestExchange_Result[]>
    {
    }
    public class RestExchange_Result
    {
        [JsonProperty("acronym")]
        public string Acronym { get; set; }

        [JsonProperty("asset_class")]
        public string Asset_Class { get; set; }

        [JsonProperty("id")]
        public int ID { get; set; }

        [JsonProperty("locale")]
        public string Locale { get; set; }

        [JsonProperty("mic")]
        public string Market_Identifier_Code_MIC { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("operating_mic")]
        public string Operating_MIC { get; set; }

        [JsonProperty("participant_id")]
        public string Participant_ID { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("url")]
        public string Website_URL { get; set; }

    }

    //
    // Option Contract (without an S - Market Data Endpoint)
    //
    public class RestOptionContract_Response : Rest_Response<RestOptionContract_Result>
    {
    }
    public class RestOptionContract_Result
    {
        [JsonProperty("break_even_price")]
        public double BreakEvenPrice { get; set; }

        [JsonProperty("day")]
        public RestOptionContract_Day Day { get; set; }

        [JsonProperty("details")]
        public RestOptionContract_Details Details { get; set; }

        [JsonProperty("greeks")]
        public RestOptionContract_Greeks Greeks { get; set; }

        [JsonProperty("implied_volatility")]
        public double Implied_Volatility { get; set; }

        [JsonProperty("last_quote")]
        public RestOptionContract_LastQuote Last_Quote { get; set; }

        [JsonProperty("last_trade")]
        public RestOptionContract_LastTrade Last_Trade { get; set; }

        [JsonProperty("open_interest")]
        public int Open_Interest { get; set; }

        [JsonProperty("underlying_asset")]
        public RestOptionContract_UnderlyingAsset Underlying_Asset { get; set; }
    }
    public class RestOptionContract_Day
    {
        [JsonProperty("change")]
        public double Change { get; set; }

        [JsonProperty("change_percent")]
        public double Change_Percent { get; set; }

        [JsonProperty("close")]
        public double Close { get; set; }

        [JsonProperty("high")]
        public double High { get; set; }

        [JsonProperty("last_updated")]
        public long Last_Updated_Ns { get; set; }

        [JsonProperty("low")]
        public double Low { get; set; }

        [JsonProperty("open")]
        public double Open { get; set; }

        [JsonProperty("previous_close")]
        public double Previous_Close { get; set; }

        [JsonProperty("volume")]
        public int Volume { get; set; }

        [JsonProperty("vwap")]
        public double VWAP { get; set; }
    }
    public class RestOptionContract_Details
    {
        [JsonProperty("contract_type")]
        public string Contract_Type { get; set; }

        [JsonProperty("exercise_style")]
        public string Exercise_Style { get; set; }

        [JsonProperty("expiration_date")]
        public string Expiration_Date { get; set; }

        [JsonProperty("shares_per_contract")]
        public int Shares_Per_Contract { get; set; }

        [JsonProperty("strike_price")]
        public double Strike_Price { get; set; }

        [JsonProperty("ticker")]
        public string Symbol { get; set; }
    }
    public class RestOptionContract_Greeks
    {
        [JsonProperty("delta")]
        public double Delta { get; set; }

        [JsonProperty("gamma")]
        public double Gamma { get; set; }

        [JsonProperty("theta")]
        public double Theta { get; set; }

        [JsonProperty("vega")]
        public double Vega { get; set; }
    }
    public class RestOptionContract_LastQuote
    {
        [JsonProperty("ask")]
        public double Ask { get; set; }

        [JsonProperty("ask_exchange")]
        public int Ask_Exchange { get; set; }

        [JsonProperty("ask_size")]
        public int Ask_Size { get; set; }

        [JsonProperty("bid")]
        public double Bid { get; set; }

        [JsonProperty("bid_exchange")]
        public int Bid_Exchange { get; set; }

        [JsonProperty("bid_size")]
        public int Bid_Size { get; set; }

        [JsonProperty("last_updated")]
        public long Last_Updated_Ns { get; set; }

        [JsonProperty("midpoint")]
        public double Midpoint { get; set; }

        [JsonProperty("timeframe")]
        public string Timeframe { get; set; }
    }
    public class RestOptionContract_LastTrade
    {
        [JsonProperty("conditions")]
        public int[] Condition_Codes { get; set; }

        [JsonProperty("exchange")]
        public int Exchange { get; set; }

        [JsonProperty("price")]
        public double Price { get; set; }

        [JsonProperty("sip_timestamp")]
        public long SIP_Timestamp_Ns { get; set; }

        [JsonProperty("size")]
        public int Size { get; set; }

        [JsonProperty("timeframe")]
        public string Timeframe { get; set; }
    }
    public class RestOptionContract_UnderlyingAsset
    {
        [JsonProperty("change_to_break_even")]
        public double Change_To_BreakEven { get; set; }

        [JsonProperty("last_updated")]
        public long Last_Updated_Ns { get; set; }

        [JsonProperty("price")]
        public double Price { get; set; }

        [JsonProperty("ticker")]
        public string Symbol { get; set; }

        [JsonProperty("timeframe")]
        public string Timeframe { get; set; }
    }

    //
    // OptionsContract and OptionsContrac[s] differ because one call returns a single Result object and the other returns an array of the same result object.
    //

    public class RestOptionsContract_Response : Rest_Response<RestOptionsContract_Result>
    {
    }
    public class RestOptionsContracts_Response : Rest_Response<RestOptionsContract_Result[]>
    {
    }
    public class RestOptionsContract_Result
    {
        [JsonProperty("additional_underlyings")]
        public RestOptionsContract_AdditionalUnderlyings[] Additional_Underlyings { get; set; }

        [JsonProperty("cfi")]
        public string CFI { get; set; }

        [JsonProperty("contract_type")]
        public string Contract_Type { get; set; }

        [JsonProperty("exercise_style")]
        public string Exercise_Style { get; set; }

        [JsonProperty("expiration_date")]
        public string Expiration_Date { get; set; }

        [JsonProperty("primary_exchange")]
        public string Primary_Exchange { get; set; }

        [JsonProperty("shares_per_contract")]
        public int Shares_Per_Contract { get; set; }

        [JsonProperty("strike_price")]
        public double Strik_Price { get; set; }

        [JsonProperty("ticker")]
        public string Symbol { get; set; }

        [JsonProperty("underlying_ticker")]
        public string Underlying_Ticker { get; set; }

        [JsonProperty("correction")]
        public int Correction { get; set; }
    }
    public class RestOptionsContract_AdditionalUnderlyings
    {
        [JsonProperty("amount")]
        public int Amount { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("underlying")]
        public string Underlying { get; set; }
    }

    //
    // Options Chain
    //

    public class RestOptionsChain_Response : Rest_Response<RestOptionsChain_Result[]>
    {
    }
    public class RestOptionsChain_Result
    {
        [JsonProperty("break_even_price")]
        public double Break_Even_Price { get; set; }

        [JsonProperty("open_interest")]
        public int Open_Interest { get; set; }

        [JsonProperty("implied_volatility")]
        public double Implied_Volatility { get; set; }

        [JsonProperty("day")]
        public RestOptionContract_Day Day { get; set; }

        [JsonProperty("details")]
        public RestOptionContract_Details Details { get; set; }

        [JsonProperty("greeks")]
        public RestOptionContract_Greeks Greeks { get; set; }

        [JsonProperty("last_quote")]
        public RestOptionsChain_Quote Last_Quote { get; set; }

        [JsonProperty("underlying_asset")]
        public RestOptionContract_UnderlyingAsset Underlying_Asset { get; set; }
    }
    public class RestOptionsChain_Quote
    {
        [JsonProperty("ask")]
        public double Ask { get; set; }

        [JsonProperty("ask_size")]
        public int Ask_Size { get; set; }

        [JsonProperty("bid")]
        public double Bid { get; set; }

        [JsonProperty("bid_size")]
        public int Bid_Size { get; set; }

        [JsonProperty("last_updated")]
        public long Last_Updated { get; set; }

        [JsonProperty("midpoint")]
        public double Midpoint { get; set; }
    }

    #endregion

    #region Market Data Channels (Socket) Objects

    public class Socket_Base
    {
        [JsonProperty("ev")]
        public string Event { get; set; }
    }
    public class Socket_Message : Socket_Base
    {
        [JsonProperty("status")]
        public string Status { get; set; }
        [JsonProperty("message")]
        public string Message { get; set; }
    }
    public class Socket_Trade : Socket_Base
    {
        [JsonProperty("sym")]
        public string Symbol { get; set; }

        [JsonProperty("x")]
        public int Exchange_ID { get; set; }

        [JsonProperty("i")]
        public string Trade_ID { get; set; }

        [JsonProperty("z")]
        public int Tape { get; set; }

        [JsonProperty("p")]
        public double Price { get; set; }

        [JsonProperty("s")]
        public int Size { get; set; }

        [JsonProperty("c")]
        public int[] Trade_Conditions { get; set; }

        [JsonProperty("t")]
        public long Timestamp_Ms { get; set; }

        [JsonProperty("q")]
        public long Sequence_Number { get; set; }

        [JsonProperty("trft")]
        public long TRF_Timestamp_Ms { get; set; }

        [JsonProperty("trfi")]
        public long TRF_Identification { get; set; }
    }
    public class Socket_Aggregate : Socket_Base
    {
        [JsonProperty("sym")]
        public string Symbol { get; set; }

        [JsonProperty("v")]
        public int Volume { get; set; }

        [JsonProperty("av")]
        public int Accumulated_Volume { get; set; }

        [JsonProperty("op")]
        public double Day_Open { get; set; }

        [JsonProperty("vw")]
        public double Tick_VWAP { get; set; }

        [JsonProperty("o")]
        public double Open { get; set; }

        [JsonProperty("c")]
        public double Close { get; set; }

        [JsonProperty("h")]
        public double High { get; set; }

        [JsonProperty("l")]
        public double Low { get; set; }

        [JsonProperty("a")]
        public double Day_VWAP { get; set; }

        [JsonProperty("z")]
        public double Average_Trade_Size { get; set; }

        [JsonProperty("s")]
        public long Timestamp_Start_Ms { get; set; }

        [JsonProperty("e")]
        public long Timestamp_End_Ms { get; set; }
    }
    public class Socket_Quote : Socket_Base
    {
        [JsonProperty("sym")]
        public string Symbol { get; set; }

        [JsonProperty("bx")]
        public int Bid_Exchange { get; set; }

        [JsonProperty("bp")]
        public double Bid_Price { get; set; }

        [JsonProperty("bs")]
        public double Bid_Size_Lots { get; set; }

        [JsonProperty("ax")]
        public int Ask_Exchange { get; set; }

        [JsonProperty("ap")]
        public double Ask_Price { get; set; }

        [JsonProperty("as")]
        public double Ask_Size_Lots { get; set; }

        [JsonProperty("c")]
        public int Quote_Condition_Code { get; set; }

        [JsonProperty("i")]
        public int[] Quote_Indicator_Codes { get; set; }

        [JsonProperty("t")]
        public long Timestamp_Ms { get; set; }

        [JsonProperty("q")]
        public long Sequence_Number { get; set; }

        [JsonProperty("z")]
        public int Tape { get; set; }
    }

    #endregion

}
