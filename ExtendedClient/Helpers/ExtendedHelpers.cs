using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradingCalendar;

namespace PolygonApiClient.ExtendedClient
{
    /// <summary>
    /// This helper class provides a collection of methods to extend the functionality of the basic client 
    /// </summary>
    public static class ExtendedHelpers
    {

        public static PolygonConnectionEndpoint Endpoint(this Security me)
        {
            if (me is Stock _)
                return PolygonConnectionEndpoint.stocks;
            if (me is Option _)
                return PolygonConnectionEndpoint.options;
            if (me is Index _)
                return PolygonConnectionEndpoint.indices;

            throw new NotImplementedException();
        }

        /// <summary>
        /// Sets a trade 'side' based on execution price relative to the last current quote. No timestamp analysis is done, it is assumed that the quote is applicable.
        /// </summary>
        /// <param name="me"></param>
        /// <param name="quote"></param>
        public static void SetTradeSide(this Trade me, Quote quote)
        {
            if (me.TradePrice <= quote.BidPrice + (quote.Spread / 3))
                me.TradeSide = TradeSide.sell;
            else if (me.TradePrice >= quote.AskPrice - (quote.Spread / 3))
                me.TradeSide = TradeSide.buy;
            else
                me.TradeSide = TradeSide.unknown;

            me.EffectiveQuote = quote;
        }

        /// <summary>
        /// Sets trades as BUY or SELL based on cotemperaneous quotes
        /// </summary>
        /// <param name="me"></param>
        /// <param name="quotes"></param>
        public static void SetTradeSides(this List<Trade> me, List<Quote> quotes)
        {
            if (me.Count == 0)
                return;

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

        /// <summary>
        /// Converts a collection of RestTrade_Results returned from a REST query into a list of local Trade objects which can integrate with Socket trades
        /// </summary>
        /// <param name="me"></param>
        /// <returns></returns>
        public static List<Trade> ToTrades(this IPolygonTrade[] me)
        {
            return new List<IPolygonTrade>(me).ConvertAll<Trade>(t => new Trade(t));
        }
        public static Trade ToTrade(this IPolygonTrade me)
        {
            return new Trade(me);
        }

        /// <summary>
        /// Converts a collection of RestQuote_Results returned from a REST query into a list of local Quote objects which can integrate with Socket quotes
        /// </summary>
        /// <param name="me"></param>
        /// <returns></returns>
        public static List<Quote> ToQuotes(this IPolygonQuote[] me)
        {
            return new List<IPolygonQuote>(me).ConvertAll<Quote>(q => new Quote(q));
        }
        public static Quote ToQuote(this IPolygonQuote me)
        {
            return new Quote(me);
        }

        /// <summary>
        /// Converts a collection of IPolygonBars returned from REST or Socket endpoints to local Bar objects
        /// </summary>
        /// <param name="me"></param>
        /// <param name="barTimespan"></param>
        /// <param name="timespanMultiplier"></param>
        /// <returns></returns>
        public static List<Bar> ToBars(this IPolygonBar[] me, PolygonTimespan barTimespan, int timespanMultiplier)
        {
            var ret = new List<Bar>();
            foreach (var b in me)
            {
                ret.Add(new Bar(b, barTimespan, timespanMultiplier));
            }
            return ret;
        }

        /// <summary>
        /// Converts an IPolygonBar returned from REST or Socket endpoints to local Bar object
        /// </summary>
        /// <param name="me"></param>
        /// <param name="barTimespan"></param>
        /// <param name="timespanMultiplier"></param>
        /// <returns></returns>
        public static Bar ToBar(this IPolygonBar me, PolygonTimespan barTimespan, int timespanMultiplier)
        {
            return (new Bar(me, barTimespan, timespanMultiplier));
        }

        /// <summary>
        /// Converts a collection of quote ticks to bar objects.  Assumes contiguous ticks and fills in missing dat with last good value
        /// </summary>
        /// <param name="me"></param>
        /// <param name="barTimespan"></param>
        /// <param name="timespanMultiplier"></param>
        /// <returns></returns>
        public static List<Bar> ToBars(this List<Quote> me, PolygonTimespan barTimespan, int timespanMultiplier, QuoteType useQuote = QuoteType.midpoint)
        {
            var ret = new List<Bar>();

            if (me.Count == 0)
                return ret;

            var startDateTime = me.Min(q => q.Timestamp.EST);
            startDateTime = startDateTime.BarStartValue(barTimespan, timespanMultiplier);
            DateTime endDateTime = startDateTime.Increment(barTimespan, timespanMultiplier);

            DateTime final = me.Max(q => q.Timestamp.EST);

            while (final > endDateTime)
            {
                // Collect the ticks within this bar's timespan
                var ticks = me.Where(q => q.Timestamp.EST >= startDateTime && q.Timestamp.EST < endDateTime).ToList();

                // Create a bar
                if (ticks.Count > 0)
                    ret.Add(ticks.ToBar(startDateTime, barTimespan, timespanMultiplier));

                // Increment timespans
                startDateTime = startDateTime.Increment(barTimespan, timespanMultiplier);
                endDateTime = endDateTime.Increment(barTimespan, timespanMultiplier);
            }

            return ret;
        }

        /// <summary>
        /// Converts a list of quotes to a single price bar at the given interval
        /// </summary>
        /// <param name="me"></param>
        /// <param name="barStart"></param>
        /// <param name="barTimespan"></param>
        /// <param name="timespanMultiplier"></param>
        /// <param name="useQuote"></param>
        /// <returns></returns>
        public static Bar ToBar(this List<Quote> me, DateTime barStart, PolygonTimespan barTimespan, int timespanMultiplier)
        {
            double high = me.Max(q => q.MidpointPrice);
            double low = me.Min(q => q.MidpointPrice);
            double open = me.First().MidpointPrice;
            double close = me.Last().MidpointPrice;
            long volume = 0;

            return new Bar(barStart, open, high, low, close, volume, barTimespan, timespanMultiplier);
        }

        /// <summary>
        /// Returns a list of all buy trades
        /// </summary>
        /// <param name="me"></param>
        /// <returns></returns>
        public static List<Trade> Buys(this List<Trade> me)
        {
            return me.Where(x => x.TradeSide == TradeSide.buy).ToList();
        }

        /// <summary>
        /// Returns a list of all sell trades
        /// </summary>
        /// <param name="me"></param>
        /// <returns></returns>
        public static List<Trade> Sells(this List<Trade> me)
        {
            return me.Where(x => x.TradeSide == TradeSide.sell).ToList();
        }

        public static List<Trade> GetLast(this List<Trade> me, int milliseconds)
        {
            return me.Where(x => x.Timestamp.Milliseconds > Calendar.CurrentTimeEst.Millisecond - milliseconds).ToList();
        }

        /// <summary>
        /// Returns a list of all trades or quotes marked with today's date (EST)
        /// </summary>
        /// <param name="me"></param>
        /// <returns></returns>
        public static List<T> Today<T>(this List<T> me) where T : Tick
        {
            return me.Where(x => x.Timestamp.EST >= DateTime.Today).ToList();
        }

        /// <summary>
        /// Returns a list of ticks that fall between the provided datetimes; inclusive of start, exclusive of end
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="me"></param>
        /// <param name="start_inclusive"></param>
        /// <param name="end_exclusive"></param>
        /// <returns></returns>
        public static List<T> Between<T>(this List<T> me, DateTime start_inclusive, DateTime end_exclusive) where T : Tick
        {
            // Flip the timestamps if input backwards
            if (start_inclusive > end_exclusive)
            {
                var t = end_exclusive;
                end_exclusive = start_inclusive;
                start_inclusive = t;
            }

            return me.Where(t =>
            t.Timestamp.EST >= start_inclusive &&
            t.Timestamp.EST < end_exclusive).ToList();
        }

        /// <summary>
        /// Returns a list of ticks that fall between the provided datetimes; inclusive of start, exclusive of end
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="me"></param>
        /// <param name="start_inclusive"></param>
        /// <param name="end_exclusive"></param>
        /// <returns></returns>
        public static List<T> Between<T>(this ParallelQuery<T> me, DateTime start_inclusive, DateTime end_exclusive) where T : Tick
        {
            // Flip the timestamps if input backwards
            if (start_inclusive > end_exclusive)
            {
                var t = end_exclusive;
                end_exclusive = start_inclusive;
                start_inclusive = t;
            }

            return me.Where(t =>
            t.Timestamp.EST >= start_inclusive &&
            t.Timestamp.EST < end_exclusive).ToList();
        }



        /// <summary>
        /// Returns a sum size of all buy trades
        /// </summary>
        /// <param name="me"></param>
        /// <returns></returns>
        public static long BuyVolume(this List<Trade> me)
        {
            return me.Buys().Sum(x => x.TradeSize);
        }

        /// <summary>
        /// Returns a sum size of all sell trades
        /// </summary>
        /// <param name="me"></param>
        /// <returns></returns>
        public static long SellVolume(this List<Trade> me)
        {
            return me.Sells().Sum(x => x.TradeSize);
        }

        /// <summary>
        /// Absolute value total of all trades (buys + sells)
        /// </summary>
        /// <param name="me"></param>
        /// <returns></returns>
        public static long GrossVolume(this List<Trade> me)
        {
            return me.Sum(t => t.TradeSize);
        }

        /// <summary>
        /// Net total of all trade activity (buys - sells)
        /// </summary>
        /// <param name="me"></param>
        /// <returns></returns>
        public static long NetVolume(this List<Trade> me)
        {
            return (me.BuyVolume() - me.SellVolume());
        }

        /// <summary>
        /// Returns a list of Calls present in a list of Options
        /// </summary>
        /// <param name="me"></param>
        /// <returns></returns>
        public static List<Option> Calls(this List<Option> me)
        {
            return me.Where(x => x.OptionType == OptionType.call).ToList();
        }

        /// <summary>
        /// Use when you know you have a list that's filtered to only one call and one put
        /// </summary>
        /// <param name="me"></param>
        /// <returns></returns>
        public static Option Call(this List<Option> me)
        {
            return me.Calls().SingleOrDefault();
        }

        /// <summary>
        /// Returns a list of Puts present in a list of Options
        /// </summary>
        /// <param name="me"></param>
        /// <returns></returns>
        public static List<Option> Puts(this List<Option> me)
        {
            return me.Where(x => x.OptionType == OptionType.put).ToList();
        }

        /// <summary>
        /// Use when you know you have a list that's filtered to only one put and one call
        /// </summary>
        /// <param name="me"></param>
        /// <returns></returns>
        public static Option Put(this List<Option> me)
        {
            return me.Puts().SingleOrDefault();
        }

        /// <summary>
        /// Returns a list of Options with the indicated Strike
        /// </summary>
        /// <param name="me"></param>
        /// <param name="strike"></param>
        /// <returns></returns>
        public static List<Option> ByStrike(this List<Option> me, double strike)
        {
            return me.Where(x => x.Strike == strike).ToList();
        }

        /// <summary>
        /// Returns a list of Options with the indicated expiry
        /// </summary>
        /// <param name="me"></param>
        /// <param name="expiry"></param>
        /// <returns></returns>
        public static List<Option> ByExpiry(this List<Option> me, DateTime expiry)
        {
            return me.Where(x => x.Expiry == expiry).ToList();
        }

        /// <summary>
        /// Indicates whether or not a trade was executed on a dark pool
        /// </summary>
        /// <param name="me"></param>
        /// <returns></returns>
        public static bool IsDarkpoolTrade(this Trade me)
        {
            if (me.TickBase.Exchange == 4 && me.TickBase.TRF_ID != 0)
                return true;

            return false;
        }

        public static bool IsSweepTrade(this Trade me)
        {
            return (me.HasConditionCode(TradeConditions._14) || me.HasConditionCode(TradeConditions._219));
        }

        public static bool IsOptionsFloorTrade(this Trade me)
        {
            return (me.HasConditionCode(TradeConditions._235) ||
                me.HasConditionCode(TradeConditions._239) ||
                me.HasConditionCode(TradeConditions._245) ||
                me.HasConditionCode(TradeConditions._242) ||
                me.HasConditionCode(TradeConditions._246) ||
                me.HasConditionCode(TradeConditions._231));
        }

        public static bool IsNotUpdateVolumeTrade(this Trade me)
        {
            foreach (var code in me.Conditions)
            {
                if (PolygonExtendedClient.Trade_Conditions_Reference[(int)code].Update_Rules != null &&
                    PolygonExtendedClient.Trade_Conditions_Reference[(int)code].Update_Rules.consolidated.updates_volume == false)
                    return true;
            }

            return false;
        }

        public static List<Trade> ExcludeCodes(this List<Trade> me, params TradeConditions[] codes)
        {
            return me.Where(t => !t.Conditions.Any(x => codes.Contains(x))).ToList();
        }

        public static bool HasConditionCode(this Trade me, TradeConditions code)
        {
            return me.TickBase?.Trade_Conditions?.Contains((int)code) ?? false;
        }

        /// <summary>
        /// Filters a list of trades to only those executed on a darkpool
        /// </summary>
        /// <param name="me"></param>
        /// <returns></returns>
        public static List<Trade> DarkpoolTrades(this List<Trade> me)
        {
            return me.Where(t => t.IsDarkpoolTrade() == true).ToList();
        }

        /// <summary>
        /// Returns true if all bars in a list of bars have the same timespan and multiplier
        /// </summary>
        /// <param name="me"></param>
        /// <returns></returns>
        public static bool SameBarSizeAll(this List<Bar> me)
        {
            if (me.Count == 0)
                return true;

            if (me.Any(
                b => b.BarTimespan != me.First().BarTimespan ||
                b.BarTimespanMultiplier != me.First().BarTimespanMultiplier))
            {
                return false;
            }

            return true;
        }


    }
}
