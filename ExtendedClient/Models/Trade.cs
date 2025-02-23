using PolygonApiClient.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace PolygonApiClient.ExtendedClient
{
    public class Trade : Tick
    {
        public IPolygonTrade TickBase { get; }

        public double TradePrice => TickBase?.Price ?? 0;
        public long TradeSize => TickBase?.Size ?? 0;
        public TradeSide TradeSide { get; set; }
        public Quote EffectiveQuote { get; set; }

        public List<TradeConditions> Conditions
        {
            get
            {
                var ret = TickBase.Trade_Conditions?.Select<int, TradeConditions>(x => (TradeConditions)x).ToList();
                return ret ?? new List<TradeConditions>();
            }
        }

        private Trade() { }

        public Trade(IPolygonTrade polygonTradeBase)
        {
            //
            // Need to differentiate what type of tick we are getting due to timestamp accuracy differences
            //
            TickBase = polygonTradeBase;

            if (polygonTradeBase is RestTrades_Result t1)
                Timestamp = PolygonTimestamp.FromNanoseconds(t1.SIP_Timestamp_Ns);
            else if (polygonTradeBase is RestLastTrade_Result t2)
                Timestamp = PolygonTimestamp.FromNanoseconds(t2.SIP_Timestamp_Ns);
            else if (polygonTradeBase is Socket_Trade t3)
                Timestamp = PolygonTimestamp.FromMilliseconds(t3.Timestamp_Ms);
            else
                throw new ArgumentException($"Unable to create trade from object {polygonTradeBase.GetType().Name}");
        }

        public Trade(IPolygonTrade polygonTradeBase, DateTime timestamp)
        {
            TickBase = polygonTradeBase;
            Timestamp = PolygonTimestamp.FromEstDateTime(timestamp);
        }

        public static Trade EmptyTrade()
        {
            Debug.WriteLine($"*** WARNING: Empty trade generated ***");

            return new Trade() { Timestamp = PolygonTimestamp.EmptyTimestamp() };
        }
    }

}
