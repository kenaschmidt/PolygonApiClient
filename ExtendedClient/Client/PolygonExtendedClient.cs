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
using static PolygonApiClient.ExtendedClient.ExtendedHelpers;

namespace PolygonApiClient.ExtendedClient
{
    /// <summary>
    /// Provides extended and simplified methods to interact with Polygon API data
    /// </summary>
    public class PolygonExtendedClient : PolygonClient, ISecurityDataProvider
    {
        public event EventHandler<bool> Working;
        private void OnWorking(bool working)
        {
            Working?.Invoke(this, working);
        }

        public PolygonExtendedClient(string apiKey, PolygonSubscriptionSettings subscriptionSettings) : base(apiKey, subscriptionSettings)
        {
            _ = ConnectAllSocketsAsync();
        }

        public async Task Load_Options_Chain_Async(Stock me)
        {
            OnWorking(true);
            me.AddOptions(await Options_Chain_Async(me.Symbol, null, null, null, null, null, null, 250, null));
            OnWorking(false);
        }

        public async Task Load_Options_Chain_Expired_Async(Stock me, DateTime lookBackStart)
        {
            OnWorking(true);
            me.AddOptions(await Options_Contracts_Async(me.Symbol, null, null, lookBackStart, PolygonFilterParams.gte, null, null, null, true, null, 1000, null));
            OnWorking(false);
        }

        public async Task Load_Bars_Async(Security me, PolygonTimespan barTimespan, int barMultiplier, DateTime day)
        {
            OnWorking(true);
            me.AddBars((await Aggregates_Bars_Async(me.Symbol, barMultiplier, barTimespan, day.Date, day.Date.AddDays(1).AddTicks(-1), null, null, 50000)).ToBars(barTimespan, barMultiplier));
            OnWorking(false);
        }

        public async Task Load_Bars_Async(Security me, PolygonTimespan barTimespan, int barMultiplier, DateTime from, DateTime to)
        {
            OnWorking(true);
            me.AddBars((await Aggregates_Bars_Async(me.Symbol, barMultiplier, barTimespan, from, to, null, null, 50000)).ToBars(barTimespan, barMultiplier));
            OnWorking(false);
        }

        public async Task Load_Quotes_And_Trades_Async(Security me, DateTime day)
        {
            OnWorking(true);

            var quotes = Quotes_Async(me.Symbol, day, null, null, 50000, null);
            var trades = Trades_Async(me.Symbol, day, null, null, 50000, null);

            me.AddQuotesAndTrades((await quotes).ToQuotes(), (await trades).ToTrades());

            OnWorking(false);
        }

        public async Task Load_Quotes_And_Trades_Async(Security me, DateTime from, DateTime to)
        {
            OnWorking(true);

            var quotes = Quotes_Async(me.Symbol, from, to, null, 50000, null);
            var trades = Trades_Async(me.Symbol, from, to, null, 50000, null);

            me.AddQuotesAndTrades((await quotes).ToQuotes(), (await trades).ToTrades());

            OnWorking(false);
        }

        public async Task<Quote> Quote_Async(Security me, DateTime? asOf = null)
        {
            if (asOf.HasValue)
            {
                OnWorking(true);
                var ret = await quote_AsOf_Async(me, asOf.Value);
                OnWorking(false);
                return ret;
            }
            else
            {
                OnWorking(true);
                var ret = await quote_Latest_Async(me);
                OnWorking(false);
                return ret;
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

        public async Task<PolygonSocketHandler> Stream_Quotes(Security me, bool subscribe)
        {
            return await Quotes_Streaming_Async(me.Symbol, me.Endpoint(), subscribe);
        }
        public async Task<PolygonSocketHandler> Stream_Trades(Security me, bool subscribe)
        {
            return await Trades_Streaming_Async(me.Symbol, me.Endpoint(), subscribe);
        }
        public async Task<PolygonSocketHandler> Stream_Second_Bars(Security me, bool subscribe)
        {
            return await Aggregate_Second_Bars_Streaming_Async(me.Symbol, me.Endpoint(), subscribe);
        }
        public async Task<PolygonSocketHandler> Stream_Minute_Bars(Security me, bool subscribe)
        {
            return await Aggregate_Minute_Bars_Streaming_Async(me.Symbol, me.Endpoint(), subscribe);
        }


        /// <summary>
        /// This function uses REST calls to return 1-second interval snapshots of quotes and trades, mimicking a socket connection
        /// </summary>
        /// <param name="security"></param>
        public RestSnapshotHandler Stream_Quotes_Trades_Snapshots(Security security, int secondsInterval = 1)
        {
            System.Timers.Timer timer = new System.Timers.Timer() { Interval = secondsInterval * 1000.0 };

            DateTime requestStart = Calendar.CurrentTimeEst.AddMilliseconds(-Calendar.CurrentTimeEst.Millisecond);

            timer.Elapsed += async (s, e) =>
            {
                // Request Quotes
                var quotes = this.Quotes_Async(
                    security.Symbol,
                    requestStart,
                    PolygonFilterParams.gt,
                    PolygonOrder.asc,
                    50000,
                    PolygonTradeQuoteSort.timestamp);

                // Request Trades
                var trades = this.Trades_Async(
                    security.Symbol,
                    requestStart,
                    PolygonFilterParams.gt,
                    PolygonOrder.asc,
                    50000,
                    PolygonTradeQuoteSort.timestamp);

                requestStart = requestStart.AddSeconds(secondsInterval);

                security.AddQuotesAndTrades(
                    (await quotes).ToQuotes(),
                    (await trades).ToTrades());
            };

            timer.Start();

            return new RestSnapshotHandler(timer);
        }

        /// <summary>
        /// Returns periodic updates to an option's Greeks based on the latest quote
        /// </summary>
        /// <param name="option"></param>
        /// <param name="secondsInterval"></param>
        /// <returns></returns>
        public RestSnapshotHandler Stream_Greeks_Snapshots(Option option, int secondsInterval = 10)
        {
            System.Timers.Timer timer = new System.Timers.Timer() { Interval = secondsInterval * 1000.0 };

            DateTime requestTime = Calendar.CurrentTimeEst;

            timer.Elapsed += async (s, e) =>
            {
                option.Greeks = await option.Greeks(requestTime);
                requestTime = requestTime.AddSeconds(secondsInterval);
            };

            timer.Start();

            return new RestSnapshotHandler(timer);
        }


    }
}
