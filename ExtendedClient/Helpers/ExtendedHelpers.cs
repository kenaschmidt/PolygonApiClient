using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PolygonApiClient.ExtendedClient
{
    /// <summary>
    /// This helper class provides a collection of methods to extend the functionality of the basic client 
    /// </summary>
    public static class ExtendedHelpers
    {
        public static void SetTradeSide(this Trade me, Quote quote)
        {


            if (me.TradePrice <= quote.BidPrice + (quote.Spread / 3))
                me.TradeSide = TradeSide.sell;
            else if (me.TradePrice >= quote.AskPrice - (quote.Spread / 3))
                me.TradeSide = TradeSide.buy;
            else
                me.TradeSide = TradeSide.unknown;
        }

        public static void SetTradeSides(this List<Trade> me, List<Quote> quotes)
        {
            // Create a combined list
            var tickList = me.Cast<Tick>().ToList();
            tickList.AddRange(quotes.Cast<Tick>());

            // Sort based on timestamp
            tickList = tickList.OrderBy(x => x.Timestamp).ToList();

            Quote lastQuote = null;
            foreach (var tick in tickList)
            {
                if (tick is Quote q)
                    lastQuote = q;
                if (tick is Trade t && lastQuote != null)
                    t.SetTradeSide(lastQuote);
            }
        }

        public static List<Trade> ToTrades(this RestTrades_Result[] me)
        {
            var ret = new List<Trade>();
            foreach (var t in me)
            {
                ret.Add(new Trade(t));
            }
            return ret;
        }

        public static List<Quote> ToQuotes(this RestQuotes_Result[] me)
        {
            var ret = new List<Quote>();
            foreach (var q in me)
            {
                ret.Add(new Quote(q));
            }
            return ret;
        }
    }
}
