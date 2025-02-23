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
using TradingCalendar;
using static PolygonApiClient.ExtendedClient.ExtendedHelpers;


namespace PolygonApiClient.ExtendedClient
{
    /// <summary>
    /// Provides extended and simplified methods to interact with Polygon API data
    /// </summary>
    public class PolygonExtendedClient : PolygonClient, ISecurityDataProvider
    {
        #region Events

        public event EventHandler<bool> Working;
        private void OnWorking(bool working)
        {
            Working?.Invoke(this, working);
        }

        #endregion

        #region Constructor

        public PolygonExtendedClient(string apiKey, PolygonSubscriptionSettings subscriptionSettings) : base(apiKey, subscriptionSettings)
        {
            _init();
        }

        private async void _init()
        {
            Console.WriteLine("Connect Polygon");
            await ConnectAllSocketsAsync();

            Console.WriteLine("Load condition codes");
            await Load_Condition_Codes();

            Ready = true;
        }

        public bool Ready { get; private set; } = false;

        private int readyTimeoutMs = 10000;
        public async Task WhenReady()
        {
            while (!Ready && readyTimeoutMs > 0)
            {
                await Task.Delay(1000);
                readyTimeoutMs -= 1000;
            }

            if (readyTimeoutMs <= 0)
                throw new Exception("Client Timeout");
        }

        #endregion

        #region Security Creation and Validation

        Dictionary<string, Stock> ActiveStocks = new Dictionary<string, Stock>();
        Dictionary<string, Index> ActiveIndices = new Dictionary<string, Index>();

        public Stock GetStock(string symbol)
        {
            if (ActiveStocks.TryGetValue(symbol, out var stock))
                return stock;

            return ActiveStocks.AddAndReturn(symbol, new Stock(symbol, this));
        }
        public async Task<bool> ValidateStock(string symbol)
        {
            OnWorking(true);
            var ret = ((await Tickers_Async(symbol)).Length == 1);
            OnWorking(false);
            return ret;
        }
        public Index GetIndex(string symbol)
        {
            symbol = symbol.AppendIndexIdentifier();

            if (ActiveIndices.TryGetValue(symbol, out var index))
                return index;

            return ActiveIndices.AddAndReturn(symbol, new Index(symbol, this));
        }

        #endregion

        #region Company/Ticker Details

        public async Task<RestTickerDetail_Result> Ticker_Details_Async(Stock me)
        {
            return await Ticker_Details_Async(me.Symbol);
        }

        #endregion

        #region Options Chain

        public async Task Load_Options_Chain_Async(IHasOptions me)
        {
            OnWorking(true);
            me.AddOptions(await Options_Chain_Async(me.Symbol, null, null, null, null, null, null, 250, null));
            OnWorking(false);
        }

        public async Task Load_Options_Chain_Expired_Async(IHasOptions me, DateTime lookBackStart)
        {
            OnWorking(true);
            me.AddOptions(await Options_Contracts_Async(me.Symbol, null, null, lookBackStart, PolygonFilterParams.gte, null, null, null, true, null, 1000, null));
            OnWorking(false);
        }

        public async Task Load_Options_Expiry_Async(IHasOptions me, DateTime expiry)
        {
            OnWorking(true);
            me.AddOptions(await Options_Chain_Async(me.Symbol, null, null, expiry, null, null, null, null, null));
            OnWorking(false);
        }

        #endregion

        #region Aggregate Bars

        public async Task<Bar> Previous_Close_Async(Security me)
        {
            OnWorking(true);
            var ret = (await Previous_Close_Async(me.Symbol)).SingleOrDefault().ToBar(PolygonTimespan.day, 1);
            OnWorking(false);
            return ret;
        }

        public async Task<List<Bar>> Get_Bars_Async(Security me, PolygonTimespan barTimespan, int barMultiplier, DateTime day)
        {
            OnWorking(true);
            var ret = (await Aggregates_Bars_Async(me.Symbol, barMultiplier, barTimespan, day.Date, day.Date.AddDays(1).AddTicks(-1), null, null, 50000)).ToBars(barTimespan, barMultiplier);
            OnWorking(false);
            return ret;
        }
        public async Task<List<Bar>> Get_Bars_Async(Security me, PolygonTimespan barTimespan, int barMultiplier, DateTime from, DateTime to)
        {
            OnWorking(true);
            var ret = (await Aggregates_Bars_Async(me.Symbol, barMultiplier, barTimespan, from, to, null, null, 50000)).ToBars(barTimespan, barMultiplier);
            OnWorking(false);
            return ret;
        }

        #endregion

        #region REST Quotes and Trades

        public async Task Load_Quotes_And_Trades_Async(Security me, DateTime day)
        {
            OnWorking(true);

            Console.WriteLine("Requesting quotes and trades");
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


        public async Task<Trade> Trade_Async(Security me, DateTime? asOf = null)
        {
            if (asOf.HasValue)
            {
                OnWorking(true);
                var trade = await trade_AsOf_Async(me, asOf.Value);
                var quote = await Quote_Async(me, trade.Timestamp.EST.AddTicks(-1));
                trade.SetTradeSide(quote);
                OnWorking(false);
                return trade;
            }
            else
            {
                OnWorking(true);
                var trade = await trade_Latest_Async(me);
                var quote = await Quote_Async(me, trade.Timestamp.EST.AddTicks(-1));
                trade.SetTradeSide(quote);
                OnWorking(false);
                return trade;
            }
        }
        private async Task<Trade> trade_Latest_Async(Security me)
        {
            return (await this.Last_Trade_Async(me.Symbol))?.ToTrade() ?? Trade.EmptyTrade();
        }
        private async Task<Trade> trade_AsOf_Async(Security me, DateTime asOf)
        {
            return (await this.Last_Trade_Async(me.Symbol, asOf))?.ToTrade() ?? Trade.EmptyTrade();
        }

        public async Task<double> Value_Async(Index me, DateTime? asOf = null)
        {

            OnWorking(true);
            var ret = await value_Latest_Async(me);
            OnWorking(false);
            return ret;

        }
        private async Task<double> value_Latest_Async(Index me)
        {
            return (await this.Indices_Snapshot_Async(me.Symbol)).SingleOrDefault()?.Value ?? -1;
        }

        #endregion

        #region Socket Quotes, Trades, and Aggregate Bars

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
        public async Task<PolygonSocketHandler> Stream_Values(Index me, bool subscribe)
        {
            return await Value_Streaming_Async(me.Symbol, me.Endpoint(), subscribe);
        }

        #endregion

        #region REST Streaming Snapshots

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
                var quotes = (await this.Quotes_Async(
                    security.Symbol,
                    requestStart,
                    PolygonFilterParams.gt,
                    PolygonOrder.asc,
                    50000,
                    PolygonTradeQuoteSort.timestamp)).ToQuotes();

                // Request Trades
                var trades = (await this.Trades_Async(
                    security.Symbol,
                    requestStart,
                    PolygonFilterParams.gt,
                    PolygonOrder.asc,
                    50000,
                    PolygonTradeQuoteSort.timestamp)).ToTrades();

                if (quotes.Count == 0 || trades.Count == 0)
                    requestStart = requestStart.AddSeconds(secondsInterval);
                else
                    requestStart = Math.Max(quotes.Max(x => x.Timestamp.Nanoseconds), trades.Max(x => x.Timestamp.Nanoseconds)).UnixNanosecondsToEST();

                security.AddQuotesAndTrades(quotes, trades);

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
        public RestSnapshotHandler Stream_Greeks_Snapshots(Option option, int secondsInterval = 5)
        {
            System.Timers.Timer timer = new System.Timers.Timer() { Interval = secondsInterval * 1000.0 };

            DateTime requestTime = Calendar.CurrentTimeEst;

            timer.Elapsed += async (s, e) =>
            {
                option.Greeks = await option.Greeks_Async(requestTime);
                requestTime = requestTime.AddSeconds(secondsInterval);
            };

            timer.Start();

            return new RestSnapshotHandler(timer);
        }

        #endregion

        #region Condition Code References

        public static Dictionary<int, RestConditions_Result> Trade_Conditions_Reference { get; protected set; } = new Dictionary<int, RestConditions_Result>();

        private async Task Load_Condition_Codes()
        {
            if (Trade_Conditions_Reference.Count != 0)
                return;

            var stockTradeCodes = await this.Conditions_Async(PolygonMarket.stocks, PolygonConditionCodeDataType.trade);

            foreach (var result in stockTradeCodes)
            {
                if (Trade_Conditions_Reference.ContainsKey(result.ID))
                    continue;
                Trade_Conditions_Reference.Add(result.ID, result);
            }

            Console.WriteLine("Stock trade condition codes loaded");

            var optionTradeCodes = await this.Conditions_Async(PolygonMarket.options, PolygonConditionCodeDataType.trade);

            foreach (var result in optionTradeCodes)
            {
                if (Trade_Conditions_Reference.ContainsKey(result.ID))
                    continue;
                Trade_Conditions_Reference.Add(result.ID, result);
            }

            Console.WriteLine("Option trade condition codes loaded");
        }


        #endregion

    }
}
