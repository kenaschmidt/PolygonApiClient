using System.Text.Json.Serialization;
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
    // 3) All fields are tagged with JsonPropertyName attributes with their actual return name values, since many are not intuitive and some returned values differ in naming despite being the
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
        [JsonPropertyName("count")]
        public int Count { get; set; }

        [JsonPropertyName("request_id")]
        public string Request_ID { get; set; }

        [JsonPropertyName("next_url")]
        public string Next_URL { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("results")]
        public virtual T Results { get; set; }
    }

    #region REST Market Data Endpoint Objects

    //
    // Stock Aggregate Bars
    //
    public class RestAggregatesBars_Response : Rest_Response<RestAggregatesBars_Result[]>
    {
        [JsonPropertyName("ticker")]
        public string Symbol { get; set; }

        [JsonPropertyName("queryCount")]
        public int QueryCount { get; set; }

        [JsonPropertyName("resultsCount")]
        public new int Count { get; set; }

        [JsonPropertyName("adjusted")]
        public bool Adjusted { get; set; }

    }
    public class RestAggregatesBars_Result : IPolygonBar
    {
        [JsonPropertyName("o")]
        public double Open { get; set; }

        [JsonPropertyName("h")]
        public double High { get; set; }

        [JsonPropertyName("l")]
        public double Low { get; set; }

        [JsonPropertyName("c")]
        public double Close { get; set; }

        [JsonPropertyName("v")]
        public long Volume { get; set; }

        [JsonPropertyName("n")]
        public int Number_Of_Trades { get; set; }

        [JsonPropertyName("t")]
        public long Timestamp_Start_Ms { get; set; }

        [JsonPropertyName("vw")]
        public double VWAP { get; set; }

        [JsonPropertyName("otc")]
        public bool IsOTC { get; set; }
    }

    //
    // Stock Grouped Daily Bars
    //
    public class RestGroupedDailyBars_Response : Rest_Response<RestGroupedDailyBars_Result[]>
    {
        [JsonPropertyName("adjusted")]
        public bool Adjusted { get; set; }

        [JsonPropertyName("queryCount")]
        public int QueryCount { get; set; }

        [JsonPropertyName("resultsCount")]
        public new int Count { get; set; }
    }
    public class RestGroupedDailyBars_Result
    {
        [JsonPropertyName("T")]
        public string Symbol { get; set; }

        [JsonPropertyName("o")]
        public double Open { get; set; }

        [JsonPropertyName("h")]
        public double High { get; set; }

        [JsonPropertyName("l")]
        public double Low { get; set; }

        [JsonPropertyName("c")]
        public double Close { get; set; }

        [JsonPropertyName("v")]
        public double Volume { get; set; }

        [JsonPropertyName("n")]
        public int Number_Of_Trades { get; set; }

        [JsonPropertyName("t")]
        public long Timestamp_End_Ms { get; set; }

        [JsonPropertyName("vw")]
        public double VWAP { get; set; }

        [JsonPropertyName("otc")]
        public bool IsOTC { get; set; }
    }

    //
    // Stock Daily Open/Close
    //
    public class RestDailyOpenClose_Result
    {
        [JsonPropertyName("afterhours")]
        public double AfterHoursClose { get; set; }

        [JsonPropertyName("close")]
        public double Close { get; set; }

        [JsonPropertyName("from")]
        public string RequestDate { get; set; }

        [JsonPropertyName("high")]
        public double High { get; set; }

        [JsonPropertyName("low")]
        public double Low { get; set; }

        [JsonPropertyName("open")]
        public double Open { get; set; }

        [JsonPropertyName("otc")]
        public bool IsOTC { get; set; }

        [JsonPropertyName("preMarket")]
        public double PremarketOpen { get; set; }

        [JsonPropertyName("symbol")]
        public string Symbol { get; set; }

        [JsonPropertyName("volume")]
        public long Volume { get; set; }
    }

    //
    // Stock Previous Close
    //
    public class RestPreviousClose_Response : Rest_Response<RestPreviousClose_Result[]>
    {
        [JsonPropertyName("adjusted")]
        public bool Adjusted { get; set; }

        [JsonPropertyName("queryCount")]
        public int QueryCount { get; set; }

        [JsonPropertyName("resultsCount")]
        public new int Count { get; set; }

        [JsonPropertyName("ticker")]
        public string Symbol { get; set; }
    }
    public class RestPreviousClose_Result : IPolygonBar
    {
        [JsonPropertyName("T")]
        public string Symbol { get; set; }

        [JsonPropertyName("o")]
        public double Open { get; set; }

        [JsonPropertyName("h")]
        public double High { get; set; }

        [JsonPropertyName("l")]
        public double Low { get; set; }

        [JsonPropertyName("c")]
        public double Close { get; set; }

        [JsonPropertyName("v")]
        public long Volume { get; set; }

        [JsonPropertyName("n")]
        public int Number_Of_Trades { get; set; }

        [JsonPropertyName("t")]
        public long Timestamp_Start_Ms { get; set; }

        [JsonPropertyName("vw")]
        public double VWAP { get; set; }
    }

    //
    // Stock Trades
    //
    public class RestTrades_Response : Rest_Response<RestTrades_Result[]>
    {
    }
    public class RestTrades_Result : IPolygonTrade
    {
        [JsonPropertyName("conditions")]
        public int[] Trade_Conditions { get; set; }

        [JsonPropertyName("correction")]
        public int Correction { get; set; }

        [JsonPropertyName("exchange")]
        public int Exchange { get; set; }

        [JsonPropertyName("id")]
        public string Trade_ID { get; set; } // Only for Stocks

        [JsonPropertyName("participant_timestamp")]
        public long Participant_Timestamp_Ns { get; set; }

        [JsonPropertyName("price")]
        public double Price { get; set; }

        [JsonPropertyName("sequence_number")]
        public long Sequence_Number { get; set; } // Only for Stocks

        [JsonPropertyName("sip_timestamp")]
        public long SIP_Timestamp_Ns { get; set; }

        [JsonPropertyName("trf_timestamp")]
        public long TRF_Timestamp_Ns { get; set; }

        [JsonPropertyName("trf_id")]
        public int TRF_ID { get; set; }

        [JsonPropertyName("size")]
        public long Size { get; set; }

        [JsonPropertyName("tape")]
        public int Tape { get; set; } // Only for Stocks
    }

    //
    // Stock Last Trade
    //
    public class RestLastTrade_Response : Rest_Response<RestLastTrade_Result>
    {
    }
    public class RestLastTrade_Result : IPolygonTrade
    {
        [JsonPropertyName("T")]
        public string Symbol { get; set; }

        [JsonPropertyName("c")]
        public int[] Trade_Conditions { get; set; }

        [JsonPropertyName("e")]
        public int Correction { get; set; }

        [JsonPropertyName("f")]
        public long TRF_Timestamp_Ns { get; set; }

        [JsonPropertyName("y")]
        public long Participant_Timestamp_Ns { get; set; }

        [JsonPropertyName("t")]
        public long SIP_Timestamp_Ns { get; set; }

        [JsonPropertyName("x")]
        public int Exchange { get; set; }

        [JsonPropertyName("i")]
        public string Trade_ID { get; set; } // Only for Stocks

        [JsonPropertyName("p")]
        public double Price { get; set; }

        [JsonPropertyName("q")]
        public long Sequence_Number { get; set; } // Only for Stocks

        [JsonPropertyName("s")]
        public long Size { get; set; }

        [JsonPropertyName("z")]
        public int Tape { get; set; } // Only for Stocks

        [JsonPropertyName("r")]
        public int TRF_ID { get; set; }
    }

    //
    // Stock Quotes
    //
    public class RestQuotes_Response : Rest_Response<RestQuotes_Result[]>
    {
    }
    public class RestQuotes_Result : IPolygonQuote
    {
        [JsonPropertyName("ask_exchange")]
        public int Ask_Exchange { get; set; }

        [JsonPropertyName("ask_price")]
        public double Ask_Price { get; set; }

        [JsonPropertyName("ask_size")]
        public long Ask_Size { get; set; }

        [JsonPropertyName("bid_exchange")]
        public int Bid_Exchange { get; set; }

        [JsonPropertyName("bid_price")]
        public double Bid_Price { get; set; }

        [JsonPropertyName("bid_size")]
        public long Bid_Size { get; set; }

        [JsonPropertyName("conditions")]
        public int[] Conditions { get; set; }

        [JsonPropertyName("indicators")]
        public int[] Indicators { get; set; }

        [JsonPropertyName("participant_timestamp")]
        public long Participant_Timestamp_Ns { get; set; }

        [JsonPropertyName("sip_timestamp")]
        public long SIP_Timestamp_Ns { get; set; }

        [JsonPropertyName("sequence_number")]
        public long Sequence_Number { get; set; }

        [JsonPropertyName("tape")]
        public int Tape { get; set; }

        [JsonPropertyName("trf_timestamp")]
        public long TRF_Timestamp_Ns { get; set; }
    }

    //
    // Stock Last Quote
    //
    public class RestLastQuote_Response : Rest_Response<RestLastQuote_Result>
    {
    }
    public class RestLastQuote_Result : IPolygonQuote
    {
        [JsonPropertyName("P")]
        public double Ask_Price { get; set; }

        [JsonPropertyName("S")]
        public long Ask_Size { get; set; }

        [JsonPropertyName("T")]
        public string Symbol { get; set; }

        [JsonPropertyName("X")]
        public int Ask_Exchange { get; set; }

        [JsonPropertyName("c")]
        public int[] Conditions { get; set; }

        [JsonPropertyName("f")]
        public long TRF_Timestamp_Ns { get; set; }

        [JsonPropertyName("i")]
        public int[] Indicators { get; set; }

        [JsonPropertyName("p")]
        public double Bid_Price { get; set; }

        [JsonPropertyName("s")]
        public long Bid_Size { get; set; }

        [JsonPropertyName("q")]
        public long Sequence_Number { get; set; }

        [JsonPropertyName("t")]
        public long SIP_Timestamp_Ns { get; set; }

        [JsonPropertyName("x")]
        public int Bid_Exchange { get; set; }

        [JsonPropertyName("y")]
        public long Participant_Timestamp_Ns { get; set; }

        [JsonPropertyName("z")]
        public int Tape { get; set; }
    }

    //
    // Snapshots - These models are used for 'All Tickers', 'Gainers/Losers', and 'Ticker'
    //
    public class RestTickerSnapshot_Response : Rest_Response<RestTickerSnapshot_Result[]>
    {
        [JsonPropertyName("tickers")]
        public override RestTickerSnapshot_Result[] Results { get; set; }
    }
    public class RestTickerSnapshot_Result
    {
        [JsonPropertyName("ticker")]
        public string Symbol { get; set; }

        [JsonPropertyName("todaysChange")]
        public double Todays_Change { get; set; }

        [JsonPropertyName("todaysChangePerc")]
        public double Todays_Change_Percent { get; set; }

        [JsonPropertyName("updated")]
        public long Last_Update_Timestamp_Ns { get; set; }

        [JsonPropertyName("fmv")]
        public double FairMarketValue { get; set; } // Only returned on Business plans

        [JsonPropertyName("day")]
        public RestTickerSnapshot_Day Last_Daily_Bar { get; set; }

        [JsonPropertyName("lastQuote")]
        public RestTickerSnapshot_LastQuote Last_Quote { get; set; }

        [JsonPropertyName("lastTrade")]
        public RestTickerSnapshot_LastTrade Last_Trade { get; set; }

        [JsonPropertyName("min")]
        public RestTickerSnapshot_LastMinuteBar Last_Minute_Bar { get; set; }

        [JsonPropertyName("prevDay")]
        public RestTickerSnapshot_PrevDayBar Previous_Day_Bar { get; set; }
    }
    public class RestTickerSnapshot_Day
    {
        [JsonPropertyName("o")]
        public double Open { get; set; }

        [JsonPropertyName("h")]
        public double High { get; set; }

        [JsonPropertyName("l")]
        public double Low { get; set; }

        [JsonPropertyName("c")]
        public double Close { get; set; }

        [JsonPropertyName("v")]
        public double Volume { get; set; }

        [JsonPropertyName("vw")]
        public double VWAP { get; set; }

        [JsonPropertyName("otc")]
        public bool IsOTC { get; set; }
    }
    public class RestTickerSnapshot_LastQuote
    {
        [JsonPropertyName("P")]
        public double Ask_Price { get; set; }

        [JsonPropertyName("S")]
        public int Ask_Size_Lots { get; set; }

        [JsonPropertyName("p")]
        public double Bid_Price { get; set; }

        [JsonPropertyName("s")]
        public int Bid_Size_Lots { get; set; }

        [JsonPropertyName("t")]
        public long SIP_Timestamp_Ns { get; set; }
    }
    public class RestTickerSnapshot_LastTrade
    {
        [JsonPropertyName("c")]
        public int[] Conditions { get; set; }

        [JsonPropertyName("t")]
        public long SIP_Timestamp_Ns { get; set; }

        [JsonPropertyName("x")]
        public int Exchange { get; set; }

        [JsonPropertyName("i")]
        public string Trade_ID { get; set; } // Only for Stocks

        [JsonPropertyName("p")]
        public double Price { get; set; }

        [JsonPropertyName("s")]
        public int Size { get; set; }

    }
    public class RestTickerSnapshot_LastMinuteBar
    {
        [JsonPropertyName("o")]
        public double Open { get; set; }

        [JsonPropertyName("h")]
        public double High { get; set; }

        [JsonPropertyName("l")]
        public double Low { get; set; }

        [JsonPropertyName("c")]
        public double Close { get; set; }

        [JsonPropertyName("v")]
        public double Volume { get; set; }

        [JsonPropertyName("av")]
        public long Accumulated_Volume { get; set; }

        [JsonPropertyName("n")]
        public int Number_Of_Trades { get; set; }

        [JsonPropertyName("t")]
        public long Timestamp_Start_Ms { get; set; }

        [JsonPropertyName("vw")]
        public double VWAP { get; set; }

        [JsonPropertyName("otc")]
        public bool IsOTC { get; set; }
    }
    public class RestTickerSnapshot_PrevDayBar
    {
        [JsonPropertyName("o")]
        public double Open { get; set; }

        [JsonPropertyName("h")]
        public double High { get; set; }

        [JsonPropertyName("l")]
        public double Low { get; set; }

        [JsonPropertyName("c")]
        public double Close { get; set; }

        [JsonPropertyName("v")]
        public double Volume { get; set; }

        [JsonPropertyName("vw")]
        public double VWAP { get; set; }

        [JsonPropertyName("otc")]
        public bool IsOTC { get; set; }
    }

    //
    // Needed to designate a few objects for special treatment in processing due to inconsistencies in formatting
    //
    public interface IResultSpecial { }

    // 
    // Technical Indicators - SMA and EMA
    //
    public class RestMovingAverage_Response : Rest_Response<RestMovingAverage_Result>, IResultSpecial
    {
    }
    public class RestMovingAverage_Result
    {
        [JsonPropertyName("underlying")]
        public RestTechnicalIndicator_Underlying Underlying_Aggregates { get; set; }

        [JsonPropertyName("values")]
        public RestMovingAverage_Values[] Indicator_Values { get; set; }
    }
    public class RestMovingAverage_Values
    {
        [JsonPropertyName("timestamp")]
        public long Timestamp_Start_Ms { get; set; }

        [JsonPropertyName("value")]
        public double Indicator_Value { get; set; }
    }

    //
    // Technical Indicators - MACD
    //
    public class RestMACD_Response : Rest_Response<RestMACD_Result>, IResultSpecial
    {
    }
    public class RestMACD_Result
    {
        [JsonPropertyName("underlying")]
        public RestTechnicalIndicator_Underlying Underlying_Aggregates { get; set; }

        [JsonPropertyName("values")]
        public RestMACD_Values[] Indicator_Values { get; set; }
    }
    public class RestMACD_Values
    {
        [JsonPropertyName("timestamp")]
        public long Timestamp_Start_Ms { get; set; }

        [JsonPropertyName("value")]
        public double MACD_Value { get; set; }

        [JsonPropertyName("signal")]
        public double Signal_Value { get; set; }

        [JsonPropertyName("histogram")]
        public double Histogram_Value { get; set; }
    }

    //
    // Technical Indictor - Relative Strength Index
    //
    public class RestRSI_Response : Rest_Response<RestRSI_Result>, IResultSpecial
    {
    }
    public class RestRSI_Result
    {
        [JsonPropertyName("underlying")]
        public RestTechnicalIndicator_Underlying Underlying_Aggregates { get; set; }

        [JsonPropertyName("values")]
        public RestRSI_Values[] Indicator_Values { get; set; }
    }
    public class RestRSI_Values
    {
        [JsonPropertyName("timestamp")]
        public long Timestamp_Start_Ms { get; set; }

        [JsonPropertyName("value")]
        public double MACD_Value { get; set; }
    }

    //
    // Technical Indicators common structures
    //
    public class RestTechnicalIndicator_Underlying
    {
        [JsonPropertyName("aggregates")]
        public RestTechnicalIndicator_Aggregates[] Aggregates { get; set; }

        [JsonPropertyName("url")]
        public string URL_Aggregates_Request { get; set; }
    }
    public class RestTechnicalIndicator_Aggregates
    {
        [JsonPropertyName("T")]
        public string Symbol { get; set; }

        [JsonPropertyName("o")]
        public double Open { get; set; }

        [JsonPropertyName("h")]
        public double High { get; set; }

        [JsonPropertyName("l")]
        public double Low { get; set; }

        [JsonPropertyName("c")]
        public double Close { get; set; }

        [JsonPropertyName("v")]
        public double Volume { get; set; }

        [JsonPropertyName("n")]
        public int Number_Of_Trades { get; set; }

        [JsonPropertyName("t")]
        public long Timestamp_Start_Ms { get; set; }

        [JsonPropertyName("vw")]
        public double VWAP { get; set; }

        [JsonPropertyName("otc")]
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
        [JsonPropertyName("active")]
        public bool Active { get; set; }

        [JsonPropertyName("cik")]
        public string CIK { get; set; }

        [JsonPropertyName("composite_figi")]
        public string Composite_FIGI { get; set; }

        [JsonPropertyName("currency_name")]
        public string Currency_Name { get; set; }

        [JsonPropertyName("delisted_utc")]
        public string Delisted_UTC { get; set; }

        [JsonPropertyName("last_updated_utc")]
        public string Last_Updated_UTC { get; set; }

        [JsonPropertyName("locale")]
        public string Locale { get; set; }

        [JsonPropertyName("market")]
        public string Market { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("primary_exchange")]
        public string Primary_Exchange { get; set; }

        [JsonPropertyName("share_class_figi")]
        public string Share_Class_FIGI { get; set; }

        [JsonPropertyName("ticker")]
        public string Symbol { get; set; }

        [JsonPropertyName("type")]
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
        [JsonPropertyName("active")]
        public bool Active { get; set; }

        [JsonPropertyName("address")]
        public RestTickerDetail_Address Address { get; set; }

        [JsonPropertyName("branding")]
        public RestTickerDetails_Branding Branding { get; set; }

        [JsonPropertyName("cik")]
        public string CIK { get; set; }

        [JsonPropertyName("composite_figi")]
        public string Composite_FIGI { get; set; }

        [JsonPropertyName("currency_name")]
        public string Currency_Name { get; set; }

        [JsonPropertyName("delisted_utc")]
        public string Delisted_UTC { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("homepage_url")]
        public string Homepage_URL { get; set; }

        [JsonPropertyName("list_date")]
        public string List_Date { get; set; }

        [JsonPropertyName("locale")]
        public string Locale { get; set; }

        [JsonPropertyName("market")]
        public string Market { get; set; }

        [JsonPropertyName("market_cap")]
        public long Market_Cap { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("phone_number")]
        public string Phone_Number { get; set; }

        [JsonPropertyName("primary_exchange")]
        public string Primary_Exchange { get; set; }

        [JsonPropertyName("round_lot")]
        public int Round_Lot_Size { get; set; }

        [JsonPropertyName("share_class_figi")]
        public string Share_Class_FIGI { get; set; }

        [JsonPropertyName("share_class_shares_outstanding")]
        public int Share_Class_Shares_Outstanding { get; set; }

        [JsonPropertyName("sic_code")]
        public string SIC_Code { get; set; }

        [JsonPropertyName("sic_description")]
        public string SIC_Description { get; set; }

        [JsonPropertyName("ticker")]
        public string Symbol { get; set; }

        [JsonPropertyName("ticker_root")]
        public string Symbol_Root { get; set; }

        [JsonPropertyName("ticker_suffix")]
        public string Symbol_Suffix { get; set; }

        [JsonPropertyName("total_employees")]
        public int Total_Employees { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("weighted_shares_outstanding")]
        public int Weighted_Shares_Outstanding { get; set; }
    }
    public class RestTickerDetail_Address
    {
        [JsonPropertyName("address1")]
        public string StreetAddress { get; set; }

        [JsonPropertyName("city")]
        public string City { get; set; }

        [JsonPropertyName("postal_code")]
        public string PostalCode { get; set; }

        [JsonPropertyName("state")]
        public string State { get; set; }
    }
    public class RestTickerDetails_Branding
    {
        [JsonPropertyName("icon_url")]
        public string Icon_URL { get; set; }

        [JsonPropertyName("logo_url")]
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
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("events")]
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
        [JsonPropertyName("amp_url")]
        public string AcceleratedMobilePage_URL { get; set; }

        [JsonPropertyName("article_url")]
        public string Article_URL { get; set; }

        [JsonPropertyName("author")]
        public string Author { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("id")]
        public string ID { get; set; }

        [JsonPropertyName("image_url")]
        public string Image_URL { get; set; }

        [JsonPropertyName("keywords")]
        public string[] Keywords { get; set; }

        [JsonPropertyName("published_utc")]
        public string Published_UTC { get; set; }

        [JsonPropertyName("publisher")]
        public RestTickerNews_Publisher Publisher { get; set; }

        [JsonPropertyName("tickers")]
        public string[] Tickers { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

    }
    public class RestTickerNews_Publisher
    {
        [JsonPropertyName("favicon_url")]
        public string Favicon_URL { get; set; }

        [JsonPropertyName("homepage_url")]
        public string Homepage_URL { get; set; }

        [JsonPropertyName("logo_url")]
        public string Logo_URL { get; set; }

        [JsonPropertyName("name")]
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

        [JsonPropertyName("asset_class")]
        public string Asset_Class { get; set; }

        [JsonPropertyName("code")]
        public string Code { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("locale")]
        public string Locale { get; set; }
    }

    //
    // Market Holidays
    //
    public class RestMarketHolidays_Result
    {
        [JsonPropertyName("close")]
        public string Closing_Time { get; set; }

        [JsonPropertyName("date")]
        public string Date { get; set; }

        [JsonPropertyName("exchange")]
        public string Exchange { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("open")]
        public string Opening_Time { get; set; }

        [JsonPropertyName("status")]
        public string Market_Status { get; set; }
    }

    //
    // Market Status
    //
    public class RestMarketStatus_Response
    {
        [JsonPropertyName("afterHours")]
        public bool Is_After_Hours { get; set; }

        [JsonPropertyName("earlyHours")]
        public bool Is_Premarket { get; set; }

        [JsonPropertyName("currencies")]
        public RestMarketStatus_Currencies Currency_Markets_Status { get; set; }

        [JsonPropertyName("exchanges")]
        public RestMarketStatus_Exchanges Exchanges { get; set; }

        [JsonPropertyName("indicesGroups")]
        public RestMarketStatus_IndicesGroups Indices { get; set; }

        [JsonPropertyName("market")]
        public string Market_Status { get; set; }

        [JsonPropertyName("mserverTime")]
        public string Server_Time { get; set; }

    }
    public class RestMarketStatus_Currencies
    {

        [JsonPropertyName("crypto")]
        public string Crypto { get; set; }

        [JsonPropertyName("fx")]
        public string FOREX { get; set; }
    }
    public class RestMarketStatus_Exchanges
    {

        [JsonPropertyName("nasdaq")]
        public string NASDAQ { get; set; }

        [JsonPropertyName("nyse")]
        public string NYSE { get; set; }

        [JsonPropertyName("otc")]
        public string OTC { get; set; }
    }
    public class RestMarketStatus_IndicesGroups
    {

        [JsonPropertyName("s_and_p")]
        public string S_and_P { get; set; }

        [JsonPropertyName("societe_generale")]
        public string Societe_Generale { get; set; }

        [JsonPropertyName("msci")]
        public string MSCI { get; set; }

        [JsonPropertyName("ftse_russell")]
        public string FTSE_Russell { get; set; }

        [JsonPropertyName("mstar")]
        public string Morningstar { get; set; }

        [JsonPropertyName("mstarc")]
        public string Morningstar_Customer { get; set; }

        [JsonPropertyName("cccy")]
        public string CBOE_Crypto { get; set; }

        [JsonPropertyName("nasdaq")]
        public string NASDAQ { get; set; }

        [JsonPropertyName("dow_jones")]
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
        [JsonPropertyName("execution_date")]
        public string Execution_Date { get; set; }

        [JsonPropertyName("split_from")]
        public double Split_From { get; set; }

        [JsonPropertyName("split_to")]
        public double Split_To { get; set; }

        [JsonPropertyName("ticker")]
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
        [JsonPropertyName("cash_amount")]
        public double Cash_Amount { get; set; }

        [JsonPropertyName("declaration_date")]
        public string Declaration_Date { get; set; }

        [JsonPropertyName("dividend_type")]
        public string Dividend_Type { get; set; }

        [JsonPropertyName("ex_dividend_date")]
        public string Ex_Dividend_Date { get; set; }

        [JsonPropertyName("frequency")]
        public int Frequency { get; set; }

        [JsonPropertyName("pay_date")]
        public string Pay_Date { get; set; }

        [JsonPropertyName("record_date")]
        public string Record_Date { get; set; }

        [JsonPropertyName("ticker")]
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
        public long value { get; set; }
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
        [JsonPropertyName("abbreviation")]
        public string Abbreviation { get; set; }

        [JsonPropertyName("asset_class")]
        public string Asset_Class { get; set; }

        [JsonPropertyName("data_types")]
        public string[] Data_Types { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("exchange")]
        public int Exchange { get; set; }

        [JsonPropertyName("id")]
        public int ID { get; set; }

        [JsonPropertyName("legacy")]
        public bool Legacy { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("sip_mapping")]
        public RestConditions_SipMapping SIP_Mapping { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("update_rules")]
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
        [JsonPropertyName("acronym")]
        public string Acronym { get; set; }

        [JsonPropertyName("asset_class")]
        public string Asset_Class { get; set; }

        [JsonPropertyName("id")]
        public int ID { get; set; }

        [JsonPropertyName("locale")]
        public string Locale { get; set; }

        [JsonPropertyName("mic")]
        public string Market_Identifier_Code_MIC { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("operating_mic")]
        public string Operating_MIC { get; set; }

        [JsonPropertyName("participant_id")]
        public string Participant_ID { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("url")]
        public string Website_URL { get; set; }

    }

    //
    // Option Contract (without an S - Market Data Endpoint)
    //
    public class RestOptionContract_Response : Rest_Response<RestOptionContract_Result>
    {
    }
    public class RestOptionContract_Result : IPolygonOptionData
    {
        [JsonPropertyName("break_even_price")]
        public double Break_Even_Price { get; set; }

        [JsonPropertyName("open_interest")]
        public int Open_Interest { get; set; }

        [JsonPropertyName("implied_volatility")]
        public double Implied_Volatility { get; set; }

        [JsonPropertyName("day")]
        public RestOptionContract_Day Day { get; set; }

        [JsonPropertyName("details")]
        public RestOptionContract_Details Details { get; set; }

        [JsonPropertyName("greeks")]
        public RestOptionContract_Greeks Greeks { get; set; }

        [JsonPropertyName("last_quote")]
        public RestOptionContract_LastQuote Last_Quote { get; set; }

        [JsonPropertyName("last_trade")]
        public RestOptionContract_LastTrade Last_Trade { get; set; }

        [JsonPropertyName("underlying_asset")]
        public RestOptionContract_UnderlyingAsset Underlying_Asset { get; set; }
    }
    public class RestOptionContract_Day
    {
        [JsonPropertyName("change")]
        public double Change { get; set; }

        [JsonPropertyName("change_percent")]
        public double Change_Percent { get; set; }

        [JsonPropertyName("close")]
        public double Close { get; set; }

        [JsonPropertyName("high")]
        public double High { get; set; }

        [JsonPropertyName("last_updated")]
        public long Last_Updated_Ns { get; set; }

        [JsonPropertyName("low")]
        public double Low { get; set; }

        [JsonPropertyName("open")]
        public double Open { get; set; }

        [JsonPropertyName("previous_close")]
        public double Previous_Close { get; set; }

        [JsonPropertyName("volume")]
        public int Volume { get; set; }

        [JsonPropertyName("vwap")]
        public double VWAP { get; set; }
    }
    public class RestOptionContract_Details
    {
        [JsonPropertyName("contract_type")]
        public string Contract_Type { get; set; }

        [JsonPropertyName("exercise_style")]
        public string Exercise_Style { get; set; }

        [JsonPropertyName("expiration_date")]
        public string Expiration_Date { get; set; }

        [JsonPropertyName("shares_per_contract")]
        public int Shares_Per_Contract { get; set; }

        [JsonPropertyName("strike_price")]
        public double Strike_Price { get; set; }

        [JsonPropertyName("ticker")]
        public string Symbol { get; set; }
    }
    public class RestOptionContract_Greeks
    {
        [JsonPropertyName("delta")]
        public double Delta { get; set; }

        [JsonPropertyName("gamma")]
        public double Gamma { get; set; }

        [JsonPropertyName("theta")]
        public double Theta { get; set; }

        [JsonPropertyName("vega")]
        public double Vega { get; set; }
    }
    public class RestOptionContract_LastQuote
    {
        [JsonPropertyName("ask")]
        public double Ask { get; set; }

        [JsonPropertyName("ask_exchange")]
        public int Ask_Exchange { get; set; }

        [JsonPropertyName("ask_size")]
        public int Ask_Size { get; set; }

        [JsonPropertyName("bid")]
        public double Bid { get; set; }

        [JsonPropertyName("bid_exchange")]
        public int Bid_Exchange { get; set; }

        [JsonPropertyName("bid_size")]
        public int Bid_Size { get; set; }

        [JsonPropertyName("last_updated")]
        public long Last_Updated_Ns { get; set; }

        [JsonPropertyName("midpoint")]
        public double Midpoint { get; set; }

        [JsonPropertyName("timeframe")]
        public string Timeframe { get; set; }

    }
    public class RestOptionContract_LastTrade
    {
        [JsonPropertyName("conditions")]
        public int[] Condition_Codes { get; set; }

        [JsonPropertyName("exchange")]
        public int Exchange { get; set; }

        [JsonPropertyName("price")]
        public double Price { get; set; }

        [JsonPropertyName("sip_timestamp")]
        public long SIP_Timestamp_Ns { get; set; }

        [JsonPropertyName("size")]
        public int Size { get; set; }

        [JsonPropertyName("timeframe")]
        public string Timeframe { get; set; }
    }
    public class RestOptionContract_UnderlyingAsset
    {
        [JsonPropertyName("change_to_break_even")]
        public double Change_To_BreakEven { get; set; }

        [JsonPropertyName("last_updated")]
        public long Last_Updated_Ns { get; set; }

        [JsonPropertyName("price")]
        public double Price { get; set; }

        [JsonPropertyName("ticker")]
        public string Symbol { get; set; }

        [JsonPropertyName("timeframe")]
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
        [JsonPropertyName("additional_underlyings")]
        public RestOptionsContract_AdditionalUnderlyings[] Additional_Underlyings { get; set; }

        [JsonPropertyName("cfi")]
        public string CFI { get; set; }

        [JsonPropertyName("contract_type")]
        public string Contract_Type { get; set; }

        [JsonPropertyName("exercise_style")]
        public string Exercise_Style { get; set; }

        [JsonPropertyName("expiration_date")]
        public string Expiration_Date { get; set; }

        [JsonPropertyName("primary_exchange")]
        public string Primary_Exchange { get; set; }

        [JsonPropertyName("shares_per_contract")]
        public int Shares_Per_Contract { get; set; }

        [JsonPropertyName("strike_price")]
        public double Strike_Price { get; set; }

        [JsonPropertyName("ticker")]
        public string Symbol { get; set; }

        [JsonPropertyName("underlying_ticker")]
        public string Underlying_Ticker { get; set; }

        [JsonPropertyName("correction")]
        public int Correction { get; set; }
    }
    public class RestOptionsContract_AdditionalUnderlyings
    {
        [JsonPropertyName("amount")]
        public int Amount { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("underlying")]
        public string Underlying { get; set; }
    }

    //
    // Options Chain
    //

    public class RestOptionsChain_Response : Rest_Response<RestOptionsChain_Result[]>
    {
    }
    public class RestOptionsChain_Result : IPolygonOptionData
    {
        [JsonPropertyName("break_even_price")]
        public double Break_Even_Price { get; set; }

        [JsonPropertyName("open_interest")]
        public int Open_Interest { get; set; }

        [JsonPropertyName("implied_volatility")]
        public double Implied_Volatility { get; set; }

        [JsonPropertyName("day")]
        public RestOptionContract_Day Day { get; set; }

        [JsonPropertyName("details")]
        public RestOptionContract_Details Details { get; set; }

        [JsonPropertyName("greeks")]
        public RestOptionContract_Greeks Greeks { get; set; }

        [JsonPropertyName("last_quote")]
        public RestOptionContract_LastQuote Last_Quote { get; set; }

        [JsonPropertyName("underlying_asset")]
        public RestOptionContract_UnderlyingAsset Underlying_Asset { get; set; }
    }

    #endregion

    #region Market Data Channels (Socket) Objects

    public class Socket_Base
    {
        [JsonPropertyName("ev")]
        public string Event { get; set; }
    }
    public class Socket_Message : Socket_Base
    {
        [JsonPropertyName("status")]
        public string Status { get; set; }
        [JsonPropertyName("message")]
        public string Message { get; set; }
    }
    public class Socket_Trade : Socket_Base, IPolygonTrade
    {
        [JsonPropertyName("sym")]
        public string Symbol { get; set; }

        [JsonPropertyName("x")]
        public int Exchange { get; set; }

        [JsonPropertyName("i")]
        public string Trade_ID { get; set; }

        [JsonPropertyName("z")]
        public int Tape { get; set; }

        [JsonPropertyName("p")]
        public double Price { get; set; }

        [JsonPropertyName("s")]
        public long Size { get; set; }

        [JsonPropertyName("c")]
        public int[] Trade_Conditions { get; set; }

        [JsonPropertyName("t")]
        public long Timestamp_Ms { get; set; }

        [JsonPropertyName("q")]
        public long Sequence_Number { get; set; }

        [JsonPropertyName("trft")]
        public long TRF_Timestamp_Ms { get; set; }

        [JsonPropertyName("trfi")]
        public int TRF_ID { get; set; }
    }
    public class Socket_Aggregate : Socket_Base, IPolygonBar
    {
        [JsonPropertyName("sym")]
        public string Symbol { get; set; }

        [JsonPropertyName("v")]
        public long Volume { get; set; }

        [JsonPropertyName("av")]
        public int Accumulated_Volume { get; set; }

        [JsonPropertyName("op")]
        public double Day_Open { get; set; }

        [JsonPropertyName("vw")]
        public double VWAP { get; set; }

        [JsonPropertyName("o")]
        public double Open { get; set; }

        [JsonPropertyName("c")]
        public double Close { get; set; }

        [JsonPropertyName("h")]
        public double High { get; set; }

        [JsonPropertyName("l")]
        public double Low { get; set; }

        [JsonPropertyName("a")]
        public double Day_VWAP { get; set; }

        [JsonPropertyName("z")]
        public double Average_Trade_Size { get; set; }

        [JsonPropertyName("s")]
        public long Timestamp_Start_Ms { get; set; }

        [JsonPropertyName("e")]
        public long Timestamp_End_Ms { get; set; }
    }
    public class Socket_Quote : Socket_Base, IPolygonQuote
    {
        [JsonPropertyName("sym")]
        public string Symbol { get; set; }

        [JsonPropertyName("bx")]
        public int Bid_Exchange { get; set; }

        [JsonPropertyName("bp")]
        public double Bid_Price { get; set; }

        [JsonPropertyName("bs")]
        public long Bid_Size { get; set; }

        [JsonPropertyName("ax")]
        public int Ask_Exchange { get; set; }

        [JsonPropertyName("ap")]
        public double Ask_Price { get; set; }

        [JsonPropertyName("as")]
        public long Ask_Size { get; set; }

        [JsonPropertyName("c")]
        public int Quote_Condition_Code { get; set; }

        [JsonPropertyName("i")]
        public int[] Quote_Indicator_Codes { get; set; }

        [JsonPropertyName("t")]
        public long Timestamp_Ms { get; set; }

        [JsonPropertyName("q")]
        public long Sequence_Number { get; set; }

        [JsonPropertyName("z")]
        public int Tape { get; set; }
    }

    #endregion

}
