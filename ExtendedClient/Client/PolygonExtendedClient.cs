using PolygonApiClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PolygonApiClient.ExtendedClient
{
    /// <summary>
    /// Provides extended and simplified methods to interact with Polygon API data
    /// </summary>
    public class PolygonExtendedClient : PolygonClient, ISecurityDataProvider
    {

        public PolygonExtendedClient(string apiKey, PolygonSubscriptionSettings subscriptionSettings) : base(apiKey, subscriptionSettings)
        {
            _ = ConnectAllSocketsAsync();
        }

        public async Task Load_Options_Chain_Async(Stock me)
        {
            var options = await Options_Chain_Async(me.Symbol, null, null, null, null, null, null, 250, null);
            me.AddOptions(options);
        }

        public async Task Load_Options_Chain_Expired_Async(Stock me, DateTime lookBackStart)
        {
            var expiredOptions = await Options_Contracts_Async(me.Symbol, null, null, lookBackStart, PolygonFilterParams.gte, null, null, null, true, null, 1000, null);
            me.AddOptions(expiredOptions);
        }

        public async Task Load_Bars_Async(Security me, PolygonTimespan barTimespan, int barMultiplier, DateTime day)
        {
            throw new NotImplementedException();
        }

        public async Task Load_Bars_Async(Security me, PolygonTimespan barTimespan, int barMultiplier, DateTime from, DateTime to)
        {
            throw new NotImplementedException();
        }

        public async Task Load_Quotes_And_Trades_Async(Security me, DateTime day)
        {
            throw new NotImplementedException();
        }

        public async Task Load_Quotes_And_Trades_Async(Security me, DateTime from, DateTime to)
        {
            throw new NotImplementedException();
        }

        public async Task<Quote> Quote_Async(Security me, DateTime? asOf = null)
        {
            if (asOf.HasValue)
            {
                return await quote_AsOf_Async(me, asOf.Value);
            }
            else
            {
                return await quote_Latest_Async(me);
            }
        }
        private async Task<Quote> quote_Latest_Async(Security me)
        {
            return (await this.Last_Quote_Async(me.Symbol))?.ToQuote() ?? Quote.EmptyQuote();
        }
        private async Task<Quote> quote_AsOf_Async(Security me, DateTime asOf)
        {
            return (await this.Last_Quote_Async(me.Symbol, asOf))?.ToQuote() ?? Quote.EmptyQuote();
        }


        ////
        //// Streaming REST Quotes - Experimental
        ////

        ///// <summary>
        ///// This function uses REST calls to return 1-second interval snapshots of quotes and trades, mimicking a socket connection
        ///// </summary>
        ///// <param name="security"></param>
        //public void Stream_REST_QuotesAndTrades(Security security, int secondsInterval)
        //{

        //    new Task(() =>
        //    {
        //        System.Timers.Timer timer = new System.Timers.Timer() { Interval = secondsInterval * 1000 };
        //        DateTime requestStart = DateTime.UtcNow.UTC_to_EST().AddMilliseconds(-DateTime.Now.Millisecond);
        //        timer.Elapsed += (s, e) =>
        //        {
        //            // Request Quotes
        //            var quotes = this.Quotes_Async(
        //                security.Symbol,
        //                requestStart,
        //                PolygonFilterParams.gt,
        //                PolygonOrder.asc,
        //                50000,
        //                PolygonTradeQuoteSort.timestamp);

        //            // Request Trades
        //            var trades = this.Trades_Async(
        //                security.Symbol,
        //                requestStart,
        //                PolygonFilterParams.gt,
        //                PolygonOrder.asc,
        //                50000,
        //                PolygonTradeQuoteSort.timestamp);

        //            requestStart = requestStart.AddSeconds(secondsInterval);

        //            handleStreamingSecondQuotesTradesAsync(security, quotes, trades);

        //        };
        //        timer.Start();
        //    }).Start();
        //}

        //private async void handleStreamingSecondQuotesTradesAsync(Security security, Task<RestQuotes_Result[]> quotes, Task<RestTrades_Result[]> trades)
        //{
        //    // Await receipt of quotes and trades, convert to local objects, and submit
        //    security.AddQuotesAndTrades(
        //        (await quotes).ToQuotes(),
        //        (await trades).ToTrades());
        //}

    }
}
