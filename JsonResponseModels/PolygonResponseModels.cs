using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace DataFarm_Polygon.Models
{
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
        public long ExchangeTimestamp_Ms { get; set; }

        [JsonProperty("q")]
        public long SequenceNumber { get; set; }

        [JsonProperty("trft")]
        public long TRFTimestamp_Ms { get; set; }

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

    #region REST Objects

    public class RestTrades_Response : IEquatable<RestTrades_Response>
    {
        public string next_url { get; set; }
        public string request_id { get; set; }
        public string status { get; set; }

        public RestTrades_Result[] results { get; set; }

        public override bool Equals(object obj)
        {
            return Equals(obj as RestTrades_Response);
        }
        public bool Equals(RestTrades_Response other)
        {
            return !(other is null) &&
                   next_url == other.next_url &&
                   request_id == other.request_id &&
                   status == other.status &&
                   EqualityComparer<RestTrades_Result[]>.Default.Equals(results, other.results);
        }
        public override int GetHashCode()
        {
            int hashCode = 23865236;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(next_url);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(request_id);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(status);
            hashCode = hashCode * -1521134295 + EqualityComparer<RestTrades_Result[]>.Default.GetHashCode(results);
            return hashCode;
        }
        public static bool operator ==(RestTrades_Response left, RestTrades_Response right)
        {
            return EqualityComparer<RestTrades_Response>.Default.Equals(left, right);
        }
        public static bool operator !=(RestTrades_Response left, RestTrades_Response right)
        {
            return !(left == right);
        }
    }
    public class RestTrades_Result : IEquatable<RestTrades_Result>
    {
        // User provided
        public string Symbol { get; set; }

        public int[] conditions { get; set; }
        public int correction { get; set; }
        public int exchange { get; set; }
        public string id { get; set; }
        public long participant_timestamp { get; set; }
        public double price { get; set; }
        public long sequence_number { get; set; }
        public long sip_timestamp { get; set; }
        public double size { get; set; }
        public int tape { get; set; }

        public override bool Equals(object obj)
        {
            return Equals(obj as RestTrades_Result);
        }

        public bool Equals(RestTrades_Result other)
        {
            return !(other is null) &&
                   Symbol == other.Symbol &&
                   EqualityComparer<int[]>.Default.Equals(conditions, other.conditions) &&
                   correction == other.correction &&
                   exchange == other.exchange &&
                   id == other.id &&
                   participant_timestamp == other.participant_timestamp &&
                   price == other.price &&
                   sequence_number == other.sequence_number &&
                   sip_timestamp == other.sip_timestamp &&
                   size == other.size &&
                   tape == other.tape;
        }

        public override int GetHashCode()
        {
            int hashCode = -1049928727;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Symbol);
            hashCode = hashCode * -1521134295 + EqualityComparer<int[]>.Default.GetHashCode(conditions);
            hashCode = hashCode * -1521134295 + correction.GetHashCode();
            hashCode = hashCode * -1521134295 + exchange.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(id);
            hashCode = hashCode * -1521134295 + participant_timestamp.GetHashCode();
            hashCode = hashCode * -1521134295 + price.GetHashCode();
            hashCode = hashCode * -1521134295 + sequence_number.GetHashCode();
            hashCode = hashCode * -1521134295 + sip_timestamp.GetHashCode();
            hashCode = hashCode * -1521134295 + size.GetHashCode();
            hashCode = hashCode * -1521134295 + tape.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(RestTrades_Result left, RestTrades_Result right)
        {
            return EqualityComparer<RestTrades_Result>.Default.Equals(left, right);
        }

        public static bool operator !=(RestTrades_Result left, RestTrades_Result right)
        {
            return !(left == right);
        }
    }

    public class RestQuotes_Response
    {
        public string next_url { get; set; }
        public string request_id { get; set; }
        public string status { get; set; }

        public RestQuotes_Result[] results { get; set; }
    }
    public class RestQuotes_Result
    {
        // User provided
        public string Symbol { get; set; }

        public int ask_exchange { get; set; }
        public double ask_price { get; set; }
        public double ask_size { get; set; }

        public int bid_exchange { get; set; }
        public double bid_price { get; set; }
        public double bid_size { get; set; }

        public int[] conditions { get; set; }
        public int[] indicators { get; set; }

        public long participant_timestamp { get; set; }
        public long sip_timestamp { get; set; }
        public long sequence_number { get; set; }
        public int tape { get; set; }
    }

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

    public class RestTickers_Response
    {
        public int count { get; set; }
        public string next_url { get; set; }
        public string request_id { get; set; }
        public string status { get; set; }

        public RestTickers_Result[] results { get; set; }
    }
    public class RestTickers_Result
    {
        public bool active { get; set; }
        public string cik { get; set; }
        public string composite_figi { get; set; }
        public string currency_name { get; set; }
        public string delisted_utc { get; set; }
        public string last_updated_utc { get; set; }
        public string locale { get; set; }
        public string market { get; set; }
        public string name { get; set; }
        public string primary_exchange { get; set; }
        public string share_class_figi { get; set; }
        public string ticker { get; set; }
        public string type { get; set; }
    }

    public class RestAggregates_Response
    {
        public string ticker { get; set; }
        public string status { get; set; }
        public int queryCount { get; set; }
        public int resultsCount { get; set; }
        public bool adjusted { get; set; }
        public string request_id { get; set; }

        public RestAggregates_Result[] results { get; set; }
    }
    public class RestAggregates_Result
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
        public double NumberOfTrades { get; set; }
        [JsonProperty("t")]
        public long Timestamp { get; set; }
        [JsonProperty("vw")]
        public double VWAP { get; set; }

        [JsonProperty("otc")]
        public bool IsOTC { get; set; }
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

    #endregion
}
