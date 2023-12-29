using Newtonsoft.Json;

namespace DataFarm_Polygon.Models
{
    #region REST Market Data Endpoint Objects

    //
    // Stock Aggregate Bars
    //
    public class RestAggregatesBars_Response
    {
        [JsonProperty("ticker")]
        public string Ticker { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("queryCount")]
        public int QueryCount { get; set; }

        [JsonProperty("resultsCount")]
        public int ResultsCount { get; set; }

        [JsonProperty("adjusted")]
        public bool Adjusted { get; set; }

        [JsonProperty("request_id")]
        public string Request_ID { get; set; }

        [JsonProperty("results")]
        public RestAggregatesBars_Result[] Results { get; set; }

        [JsonProperty("next_url")]
        public string Next_URL { get; set; }
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
    public class RestGroupedDailyBars_Response
    {
        [JsonProperty("adjusted")]
        public bool Adjusted { get; set; }

        [JsonProperty("queryCount")]
        public int QueryCount { get; set; }

        [JsonProperty("request_id")]
        public string Request_ID { get; set; }

        [JsonProperty("resultsCount")]
        public int ResultsCount { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("results")]
        public RestGroupedDailyBars_Result[] Results { get; set; }

    }
    public class RestGroupedDailyBars_Result
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
        public long Timestamp_End_Ms { get; set; }

        [JsonProperty("vw")]
        public double VWAP { get; set; }

        [JsonProperty("otc")]
        public bool IsOTC { get; set; }
    }

    //
    // Stock Daily Open/Close
    //
    public class DailyOpenClose_Response
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

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("symbol")]
        public string Symbol { get; set; }

        [JsonProperty("volume")]
        public long Volume { get; set; }
    }

    //
    // Stock Previous Close
    //
    public class PreviousClose_Response
    {
        [JsonProperty("adjusted")]
        public bool Adjusted { get; set; }

        [JsonProperty("queryCount")]
        public int QueryCount { get; set; }

        [JsonProperty("request_id")]
        public string Request_ID { get; set; }

        [JsonProperty("results")]
        public PreviousClose_Result[] Results { get; set; }

        [JsonProperty("resultsCount")]
        public int ResultsCount { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("ticker")]
        public string Symbol { get; set; }
    }
    public class PreviousClose_Result
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
    }

    //
    // Stock Trades
    //
    public class RestTrades_Response
    {
        [JsonProperty("request_id")]
        public string Request_ID { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("next_url")]
        public string Next_URL { get; set; }

        [JsonProperty("results")]
        public RestTrades_Result[] Results { get; set; }
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
        public string TradeID { get; set; } // Only for Stocks

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
        public int Size { get; set; }

        [JsonProperty("tape")]
        public int Tape { get; set; } // Only for Stocks
    }

    //
    // Stock Last Trade
    //
    public class RestLastTrade_Response
    {
        [JsonProperty("request_id")]
        public string Request_ID { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("results")]
        public RestLastTrade_Result[] Results { get; set; }
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
        public string TradeID { get; set; } // Only for Stocks

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
    public class RestQuotes_Response
    {
        [JsonProperty("request_id")]
        public string Request_ID { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("next_url")]
        public string Next_URL { get; set; }

        [JsonProperty("results")]
        public RestQuotes_Result[] Results { get; set; }
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
    public class RestLastQuote_Response
    {
        [JsonProperty("request_id")]
        public string Request_ID { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("results")]
        public RestLastQuote_Result[] Results { get; set; }
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
    public class RestTickerSnapshot_Response
    {
        [JsonProperty("count")]
        public int Count { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("tickers")]
        public RestTickerSnapshot_Ticker[] Tickers { get; set; }
    }
    public class RestTickerSnapshot_Ticker
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
        public string TradeID { get; set; } // Only for Stocks

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
    // Technical Indicators - SMA and EMA
    //
    public class RestMovingAverage_Response
    {
        [JsonProperty("next_url")]
        public string Next_URL { get; set; }

        [JsonProperty("request_id")]
        public string Request_ID { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("results")]
        public RestMovingAverage_Result[] Results { get; set; }
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
    public class RestMACD_Response
    {
        [JsonProperty("next_url")]
        public string Next_URL { get; set; }

        [JsonProperty("request_id")]
        public string Request_ID { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("results")]
        public RestMACD_Result[] Results { get; set; }
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
    public class RestRSI_Response
    {
        [JsonProperty("next_url")]
        public string Next_URL { get; set; }

        [JsonProperty("request_id")]
        public string Request_ID { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("results")]
        public RestRSI_Result[] Results { get; set; }
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
    public class RestTickers_Response
    {
        [JsonProperty("count")]
        public int Count { get; set; }

        [JsonProperty("next_url")]
        public string Next_URL { get; set; }

        [JsonProperty("request_id")]
        public string Request_ID { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        public RestTickers_Result[] Results { get; set; }
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
    public class RestTickerDetail_Response
    {
        [JsonProperty("request_id")]
        public string Request_ID { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        public RestTickerDetail_Result[] Results { get; set; }
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
        public int SIC_Code { get; set; }

        [JsonProperty("sic_description")]
        public int SIC_Description { get; set; }

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

    #endregion

    //
    //
    //
    //
    // Still need to go through below
    //
    //
    //
    //

    public class RestExchange_Response
    {
        public int count { get; set; }
        public string request_id { get; set; }
        public string status { get; set; }

        public RestExchange_Result[] results { get; set; }
    }
    public class RestExchange_Result
    {
        public string acronym { get; set; }
        public string asset_class { get; set; }
        public int id { get; set; }
        public string locale { get; set; }
        public string mic { get; set; }
        public string name { get; set; }
        public string operating_mic { get; set; }
        public string participant_id { get; set; }
        public string type { get; set; }
        public string url { get; set; }

    }


    public class RestTradeConditionCodes_Response
    {
        public int count { get; set; }
        public string next_url { get; set; }
        public string request_id { get; set; }
        public string status { get; set; }

        public RestTradeConditionCodes_Result[] results { get; set; }
    }
    public class RestTradeConditionCodes_Result
    {
        public string abbreviation { get; set; }
        public string asset_class { get; set; }
        public string[] data_types { get; set; }
        public string description { get; set; }
        public int exchange { get; set; }
        public int id { get; set; }
        public bool legacy { get; set; }
        public string name { get; set; }
        public RestTradeConditionCodes_SipMapping sip_mapping { get; set; }
        public string type { get; set; }
        public RestTradeConditionCodes_UpdateRules update_rules { get; set; }

    }
    public class RestTradeConditionCodes_SipMapping
    {
        public string CTA { get; set; }
        public string UTP { get; set; }
        public string OPRA { get; set; }
    }
    public class RestTradeConditionCodes_UpdateRules
    {
        public RestTradeConditionCodes_Consolidated consolidated { get; set; }
        public RestTradeConditionCodes_MarketCenter market_center { get; set; }
    }
    public class RestTradeConditionCodes_Consolidated
    {
        public bool updates_high_low { get; set; }
        public bool updates_open_close { get; set; }
        public bool updates_volume { get; set; }
    }
    public class RestTradeConditionCodes_MarketCenter
    {
        public bool updates_high_low { get; set; }
        public bool updates_open_close { get; set; }
        public bool updates_volume { get; set; }
    }

    public class RestOptionsChain_Response
    {
        public string request_id { get; set; }
        public RestOptionsChain_Result[] results { get; set; }
        public string status { get; set; }
        public string next_url { get; set; }
    }
    public class RestOptionsChain_Result
    {
        public double break_even_price { get; set; }
        public int open_interest { get; set; }
        public double implied_volatility { get; set; }

        public RestOptionsChain_Day day { get; set; }
        public RestOptionsChain_Details details { get; set; }
        public RestOptionsChain_Greeks greeks { get; set; }
        public RestOptionsChain_Quote last_quote { get; set; }
        public RestOptionsChain_UnderlyingAsset underlying_asset { get; set; }
    }
    public class RestOptionsChain_Day
    {
        public double change { get; set; }
        public double change_percent { get; set; }
        public double close { get; set; }
        public double high { get; set; }
        public long last_updated { get; set; }
        public double low { get; set; }
        public double open { get; set; }
        public double previous_close { get; set; }
        public int volume { get; set; }
        public double vwap { get; set; }
    }
    public class RestOptionsChain_Details
    {
        public string contract_type { get; set; }
        public string exercise_style { get; set; }
        public string expiration_date { get; set; }
        public int shares_per_contract { get; set; }
        public double strike_price { get; set; }
        public string ticker { get; set; }
    }
    public class RestOptionsChain_Greeks
    {
        public double delta { get; set; }
        public double gamma { get; set; }
        public double theta { get; set; }
        public double vega { get; set; }
    }
    public class RestOptionsChain_Quote
    {
        public double ask { get; set; }
        public int ask_size { get; set; }
        public double bid { get; set; }
        public int bid_size { get; set; }
        public long last_updated { get; set; }
        public double midpoint { get; set; }
    }
    public class RestOptionsChain_UnderlyingAsset
    {
        public double change_to_breakeven { get; set; }
        public long last_updated { get; set; }
        public double price { get; set; }
        public string ticker { get; set; }
        public string timeframe { get; set; }
    }

    public class RestOptionsChainExpired_Response
    {
        public string request_id { get; set; }
        public RestOptionsChainExpired_Result[] results { get; set; }
        public string status { get; set; }
    }
    public class RestOptionsChainExpired_Result
    {
        public RestOptionsChainExpired_AdditionalUnderlyings[] additional_underlyings { get; set; }

        public string cfi { get; set; }
        public string contract_type { get; set; }
        public int correction { get; set; }
        public string exercise_style { get; set; }
        public string expiration_date { get; set; }
        public string primary_exchange { get; set; }
        public int shares_per_contract { get; set; }
        public double strike_price { get; set; }
        public string ticker { get; set; }
        public string underlying_ticker { get; set; }
    }
    public class RestOptionsChainExpired_AdditionalUnderlyings
    {
        public int amount { get; set; }
        public string type { get; set; }
        public string underlying { get; set; }

    }




    #region Socket Objects

    public class SocketBase
    {
        [JsonProperty("ev")]
        public string Event { get; set; }
    }
    public class SocketMessage : SocketBase
    {
        [JsonProperty("status")]
        public string Status { get; set; }
        [JsonProperty("message")]
        public string Message { get; set; }
    }
    public class SocketTrade : SocketBase
    {
        [JsonProperty("sym")]
        public string Symbol { get; set; }

        [JsonProperty("x")]
        public int ExchangeId { get; set; }

        [JsonProperty("i")]
        public string TradeId { get; set; }

        [JsonProperty("z")]
        public int Tape { get; set; }

        [JsonProperty("p")]
        public double Price { get; set; }

        [JsonProperty("s")]
        public int Size { get; set; }

        [JsonProperty("c")]
        public int[] TradeConditions { get; set; }

        [JsonProperty("t")]
        public long Timestamp_Ms { get; set; }

        [JsonProperty("q")]
        public long SequenceNumber { get; set; }

        [JsonProperty("trft")]
        public long TRF_Timestamp_Ms { get; set; }

        [JsonProperty("trfi")]
        public long TRF_Identification { get; set; }
    }
    public class SocketAggregate : SocketBase
    {
        [JsonProperty("sym")]
        public string Symbol { get; set; }

        [JsonProperty("v")]
        public int Volume { get; set; }

        [JsonProperty("av")]
        public int TotalVolume { get; set; }

        [JsonProperty("op")]
        public double DayOpen { get; set; }

        [JsonProperty("vw")]
        public double TickVWAP { get; set; }

        [JsonProperty("o")]
        public double Open { get; set; }

        [JsonProperty("c")]
        public double Close { get; set; }

        [JsonProperty("h")]
        public double High { get; set; }

        [JsonProperty("l")]
        public double Low { get; set; }

        [JsonProperty("a")]
        public double DayVWAP { get; set; }

        [JsonProperty("z")]
        public double AverageTradeSize { get; set; }

        [JsonProperty("s")]
        public long TimeStamp_Start_Ms { get; set; }

        [JsonProperty("e")]
        public long TimeStamp_End_Ms { get; set; }
    }
    public class SocketQuote : SocketBase
    {
        [JsonProperty("sym")]
        public string Symbol { get; set; }

        [JsonProperty("bx")]
        public int Bid_Exchange { get; set; }

        [JsonProperty("bp")]
        public double Bid_Price { get; set; }

        [JsonProperty("bs")]
        public double Bid_Size { get; set; }

        [JsonProperty("ax")]
        public int Ask_Exchange { get; set; }

        [JsonProperty("ap")]
        public double Ask_Price { get; set; }

        [JsonProperty("as")]
        public double Ask_Size { get; set; }

        [JsonProperty("c")]
        public int ConditionCode { get; set; }

        [JsonProperty("i")]
        public int[] IndicatorCodes { get; set; }

        [JsonProperty("t")]
        public long Timestamp_Ms { get; set; }

        [JsonProperty("q")]
        public long SequenceNumber { get; set; }

        [JsonProperty("z")]
        public int Tape { get; set; }
    }

    #endregion

}
