using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using PolygonApiClient.Helpers;
using PolygonApiClient.MasterClient;
using PolygonApiClient.Properties;
using PolygonApiClient.RESTClient;
using PolygonApiClient.WebSocketsClient;
using static PolygonApiClient.PolygonHelpers;

namespace PolygonApiClient
{
    //
    // This class controls both the Socket and REST clients and provides all top-level functionality to external clients
    //
    public class PolygonClient
    {
        // REST client
        protected PolygonRestClient restClient { get; set; }

        // Socket clients for Stocks, Options, Indices, Forex, and Crypto
        protected Dictionary<PolygonConnectionEndpoint, PolygonSocketClient> socketClients { get; } = new Dictionary<PolygonConnectionEndpoint, PolygonSocketClient>();

        // Polygin account subscription settings
        public PolygonSubscriptionSettings SubscriptionSettings { get; }

        // API Key (change this to reflect multiple key usage)
        protected readonly string API_Key;

        #region Events

        public event EventHandler<string> SystemMessage;
        private void OnSystemMessage(string msg)
        {
            SystemMessage?.Invoke(this, msg);
        }

        public event EventHandler<PolygonSocketClient> SocketConnectionChanged;
        private void OnSocketConnectionChanged(PolygonSocketClient client)
        {
            SocketConnectionChanged?.Invoke(this, client);
        }

        #endregion

        #region Constructor and Initialization

        public PolygonClient(string apiKey, PolygonSubscriptionSettings subscriptionSettings)
        {
            API_Key = apiKey;

            SubscriptionSettings = subscriptionSettings;

            _initPolygonClients();
        }

        private void _initPolygonClients()
        {
            // Instantiate REST client
            restClient = new PolygonRestClient(API_Key);

            // Instantiate SOCKET clients for each endpoint
            foreach (PolygonConnectionEndpoint endpoint in Enum.GetValues(typeof(PolygonConnectionEndpoint)))
            {
                // Check WebSocket permissions before creating a client
                if (!SubscriptionSettings.GetSettings(endpoint).HasPermission(PolygonPermissions.WebSockets))
                    continue;

                var client = socketClients.AddAndReturn(endpoint, new PolygonSocketClient(API_Key, endpoint));

                _initSocketClientHandlers(client);
            }
        }
        private void _initSocketClientHandlers(PolygonSocketClient client)
        {
            client.Opened += (s, e) =>
            {
                Debug.WriteLine($"{((PolygonSocketClient)s).Name} OPENED");
            };
            client.Closed += (s, e) =>
            {
                Debug.WriteLine($"{((PolygonSocketClient)s).Name} CLOSED");
            };
            client.MessageReceived += (s, e) =>
            {
                Debug.WriteLine($"{((PolygonSocketClient)s).Name} MESSAGE {e.Message}");
            };
            client.ErrorReceived += (s, e) =>
            {
                Debug.WriteLine($"{((PolygonSocketClient)s).Name} ERROR {e.ErrorMessage}");
            };
        }

        #endregion

        #region Socket Management

        public async Task ConnectAllSocketsAsync()
        {
            try
            {
                foreach (PolygonSocketClient client in socketClients.Values)
                {
                    await client.OpenAsync();
                }
            }
            catch (Exception ex)
            {
                OnSystemMessage($"Could not connect socket: {ex.Message}");
                Debug.WriteLine($"Could not connect socket: {ex.Message}");
            }
        }

        public async Task DisconnectAllSocketsAsync()
        {
            try
            {
                foreach (PolygonSocketClient client in socketClients.Values)
                {
                    await client.CloseAsync();
                }
            }
            catch (Exception ex)
            {
                OnSystemMessage($"Could not disconnect socket: {ex.Message}");
                Debug.WriteLine($"Could not disconnect socket: {ex.Message}");
            }
        }

        public PolygonSocketClient GetClient(PolygonConnectionEndpoint endpoint)
        {
            return socketClients[endpoint];
        }

        #endregion

        #region -------------------- Polygon REST API Market Data Endpoints -----------------------

        // ------ Notes ------
        //
        // 1) Return types vary slightly based on endpoint. Most return an array of 'Result' objects, as pagination and assembly of multiple responses is handled by the RESTClient.
        //    Some calls will always return a single 'Result' object, and a few return the top-level 'Response' object because they contain the actual data for those calls (v1 endpoints).   
        //
        // 2) These top-level requests handle parameter normalization and pass to the RESTClient for execution.
        //
        // 3) Timestamp values are in the form of a PolygonTimestamp class, which is created by the user and allows easy conversion of different timestamp values (string, datetime, Ms/Ns)
        //
        // 4) Optional parameters are nullables and will be left out of the request
        //
        // -------------------

        /// <summary>
        /// Get aggregate bars for a stock or option contract over a given date range in custom time window sizes. For example, if timespan = ‘minute’ and multiplier = ‘5’ then 5-minute bars will be returned.
        /// </summary>
        /// <param name="symbol">The ticker symbol of the stock or options contract.</param>
        /// <param name="multiplier">The size of the timespan multiplier.</param>
        /// <param name="timespan">The size of the time window.</param>
        /// <param name="from">The start of the aggregate time window (EST).</param>
        /// <param name="to">The end of the aggregate time window (EST).</param>
        /// <param name="adjusted">Whether or not the results are adjusted for splits. By default, results are adjusted.</param>
        /// <param name="sort">Sort the results by timestamp. asc will return results in ascending order (oldest at the top), desc will return results in descending order (newest at the top).</param>
        /// <param name="limit">Limits the number of base aggregates queried to create the aggregate results. Max 50000 and Default 5000.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<RestAggregatesBars_Result[]> Aggregates_Bars_Async(
            string symbol,
            int multiplier,
            PolygonTimespan timespan,
            DateTime from,
            DateTime to,
            bool? adjusted = null,
            PolygonSort? sort = null,
            int? limit = null)
        {
            try
            {
                // Normalize parameters
                symbol = NormalizeSymbol(symbol);

                // Call async request method
                return await restClient.Aggregates_Bars_Async(
                    symbol,
                    multiplier,
                    timespan,
                    from.ESTToUnixMilliseconds(),
                    to.ESTToUnixMilliseconds(),
                    adjusted,
                    sort,
                    limit);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<RestGroupedDailyBars_Result[]> Grouped_Daily_Bars_Async(
            DateTime date,
            bool? adjusted = null,
            bool? include_otc = null)
        {
            try
            {
                // Normalize parameters
                string _date = date.ToString("yyyy-MM-dd");

                // Call async reqwust method
                return await restClient.Grouped_Daily_Bars_Async(
                   _date,
                   adjusted,
                   include_otc);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<RestDailyOpenClose_Result> Daily_Open_Close_Async(
            string symbol,
            DateTime date,
            bool? adjusted = null)
        {
            try
            {
                // Normalize parameters
                symbol = NormalizeSymbol(symbol);
                string _date = date.ToString("yyyy-MM-dd");

                // Call async request method
                return await restClient.Daily_Open_Close_Async(
                    symbol,
                    _date,
                    adjusted);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<RestPreviousClose_Result[]> Previous_Close_Async(
            string symbol,
            bool? adjusted = null)
        {
            try
            {
                // Normalize parameters
                symbol = NormalizeSymbol(symbol);

                // Call async request method
                return await restClient.Previous_Close_Async(
                    symbol,
                    adjusted);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<RestTrades_Result[]> Trades_Async(
            string symbol,
            DateTime? timestamp = null,
            PolygonFilterParams? timestampFilter = null,
            PolygonOrder? order = null,
            int? limit = null,
            PolygonTradeQuoteSort? sort = null)
        {
            try
            {
                // Normalize parameters
                symbol = NormalizeSymbol(symbol);

                // If the timestamp is anything other than a date (has a non-default time component), convert to nanoseconds and send
                if (timestamp.HasValue && timestamp.Value.TimeOfDay != TimeSpan.Zero)
                {
                    long? _timestamp = timestamp?.ESTToUnixNanoseconds() ?? null;

                    // Call async request method
                    return await restClient.Trades_Async(
                        symbol,
                        _timestamp,
                        timestampFilter,
                        order,
                        limit,
                        sort);
                }
                else
                    return await restClient.Trades_Async(
                       symbol,
                       timestamp.HasValue ? timestamp.Value.ToString("yyyy-MM-dd") : null,
                       timestampFilter,
                       order,
                       limit,
                       sort);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<RestLastTrade_Result> Last_Trade_Async(
            string symbol)
        {
            try
            {
                // Normalize parameters
                symbol = NormalizeSymbol(symbol);

                // Call async request method
                return await restClient.Last_Trade_Async(
                    symbol);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<RestQuotes_Result[]> Quotes_Async(
            string symbol,
            DateTime? timestamp = null,
            PolygonFilterParams? timestampFilter = null,
            PolygonOrder? order = null,
            int? limit = null,
            PolygonTradeQuoteSort? sort = null)
        {
            try
            {
                // Normalize parameters
                symbol = NormalizeSymbol(symbol);

                // If the timestamp is anything other than a date (has a non-default time component), convert to nanoseconds and send
                if (timestamp.HasValue && timestamp.Value.TimeOfDay != TimeSpan.Zero)
                {
                    long? _timestamp = timestamp?.ESTToUnixNanoseconds() ?? null;

                    // Call async request method
                    return await restClient.Quotes_Async(
                        symbol,
                        _timestamp,
                        timestampFilter,
                        order,
                        limit,
                        sort);
                }
                else
                    return await restClient.Quotes_Async(
                       symbol,
                       timestamp.HasValue ? timestamp.Value.ToString("yyyy-MM-dd") : null,
                       timestampFilter,
                       order,
                       limit,
                       sort);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<RestLastQuote_Result> Last_Quote_Async(
            string symbol)
        {
            try
            {
                // Normalize parameters
                symbol = NormalizeSymbol(symbol);

                // Call async request method
                return await restClient.Last_Quote_Async(
                    symbol);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        // ---------- Snapshots ----------

        public async Task<RestTickerSnapshot_Result[]> All_Tickers_Async(
            List<string> tickers = null,
            bool? include_otc = null)
        {
            try
            {
                // Normalize parameters

                // Create a CSV string of tickers to pass
                StringBuilder sb = new StringBuilder();

                if (tickers != null)
                    for (int i = 0; i < tickers.Count; i++)
                    {
                        sb.Append(NormalizeSymbol(tickers[i]));
                        if (i + 1 < tickers.Count)
                            sb.Append(",");
                    }

                string tickerList = sb.ToString();

                // Call async request method
                return await restClient.All_Tickers_Async(
                    tickerList.Length > 0 ? tickerList : null,
                    include_otc);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<RestTickerSnapshot_Result[]> Gainers_Losers_Async(
            PolygonGainersLosers direction,
            bool? include_otc = null)
        {
            try
            {
                // Call async request method
                return await restClient.Gainers_Losers_Async(
                    direction,
                    include_otc);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<RestTickerSnapshot_Result> Ticker_Async(
            string stocksTicker)
        {
            // This method calls the same endpoint as All_Tickers but with a single ticker defined. Return data is exactly the same

            var result = await All_Tickers_Async(new List<string> { stocksTicker }, true);

            if (result.Length == 0)
                return null;

            return result[0];
        }

        public async Task<RestOptionContract_Result> Option_Contract_Async(
            string underlyingAsset,
            string optionContract)
        {
            try
            {
                // Normalize parameters
                underlyingAsset = NormalizeSymbol(underlyingAsset);
                optionContract = NormalizeSymbol(optionContract);

                // Call async request method
                return await restClient.Option_Contract_Async(
                    underlyingAsset,
                    optionContract);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<RestOptionsChain_Result[]> Options_Chain_Async(
            string underlyingAsset,
            double? strike_price = null,
            PolygonFilterParams? strikePriceFilter = null,
            DateTime? expirationDate = null,
            PolygonFilterParams? expirationDateFilter = null,
            OptionType? contract_type = null,
            PolygonOrder? order = null,
            int? limit = null,
            PolygonOptionsChainSort? sort = null)
        {
            try
            {
                // Normalize parameters
                underlyingAsset = NormalizeSymbol(underlyingAsset);
                string _expirationDate = expirationDate.HasValue ? expirationDate.Value.ToString("yyyy-MM-dd") : null;

                // Call async request method
                return await restClient.Options_Chain_Async(
                    underlyingAsset,
                    strike_price,
                    strikePriceFilter,
                    _expirationDate,
                    expirationDateFilter,
                    contract_type,
                    order,
                    limit,
                    sort);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //public async Task<object> Universal_Snapshot_Async()
        //{
        //    // This is a bizarre and complicated call that I am not going to bother with for a while
        //    throw new NotImplementedException();
        //}

        // ---------- Technical Indicators ----------

        public async Task<RestMovingAverage_Result[]> Simple_Moving_Average_Async(
            string symbol,
            DateTime? timestamp = null,
            PolygonFilterParams? timestampFilter = null,
            PolygonTimespan? timespan = null,
            int? window = null,
            PolygonAggregateSeriesType? series_type = null,
            PolygonOrder? order = null,
            bool? expand_underlying = null,
            bool? adjusted = null,
            int? limit = null)
        {
            try
            {
                // Normalize parameters
                symbol = NormalizeSymbol(symbol);

                long? _timetamp = null;
                if (timestamp.HasValue)
                    _timetamp = timestamp.Value.ESTToUnixMilliseconds();

                // Call async request method
                return await restClient.Simple_Moving_Average_Async(
                    symbol,
                    _timetamp,
                    timestampFilter,
                    timespan,
                    window,
                    series_type,
                    order,
                    expand_underlying,
                    adjusted,
                    limit);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<RestMovingAverage_Result[]> Exponential_Moving_Average_Async(
            string symbol,
            DateTime? timestamp = null,
            PolygonFilterParams? timestampFilter = null,
            PolygonTimespan? timespan = null,
            int? window = null,
            PolygonAggregateSeriesType? series_type = null,
            PolygonOrder? order = null,
            bool? expand_underlying = null,
            bool? adjusted = null,
            int? limit = null)
        {
            try
            {
                // Normalize parameters
                symbol = NormalizeSymbol(symbol);

                long? _timetamp = null;
                if (timestamp.HasValue)
                    _timetamp = timestamp.Value.ESTToUnixMilliseconds();

                // Call async request method
                return await restClient.Exponential_Moving_Average_Async(
                    symbol,
                    _timetamp,
                    timestampFilter,
                    timespan,
                    window,
                    series_type,
                    order,
                    expand_underlying,
                    adjusted,
                    limit);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<RestMACD_Result[]> Moving_Average_Convergence_Divergence_Async(
            string symbol,
            DateTime? timestamp = null,
            PolygonFilterParams? timestampFilter = null,
            PolygonTimespan? timespan = null,
            bool? adjusted = null,
            int? short_window = null,
            int? long_window = null,
            int? signal_window = null,
            PolygonAggregateSeriesType? series_type = null,
            bool? expand_underlying = null,
            PolygonOrder? order = null,
            int? limit = null)
        {
            try
            {
                // Normalize parameters
                symbol = NormalizeSymbol(symbol);

                long? _timetamp = null;
                if (timestamp.HasValue)
                    _timetamp = timestamp.Value.ESTToUnixMilliseconds();

                // Call async request method
                return await restClient.Moving_Average_Convergence_Divergence_Async(
                    symbol,
                    _timetamp,
                    timestampFilter,
                    timespan,
                    adjusted,
                    short_window,
                    long_window,
                    signal_window,
                    series_type,
                    expand_underlying,
                    order,
                    limit);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<RestRSI_Result[]> Relative_Strength_Index_Async(
            string symbol,
            DateTime? timestamp = null,
            PolygonFilterParams? timestampFilter = null,
            PolygonTimespan? timespan = null,
            int? window = null,
            PolygonAggregateSeriesType? series_type = null,
            bool? expand_underlying = null,
            PolygonOrder? order = null,
            int? limit = null)
        {
            try
            {
                // Normalize parameters
                symbol = NormalizeSymbol(symbol);

                long? _timetamp = null;
                if (timestamp.HasValue)
                    _timetamp = timestamp.Value.ESTToUnixMilliseconds();

                // Call async request method
                return await restClient.Relative_Strength_Index_Async(
                    symbol,
                    _timetamp,
                    timestampFilter,
                    timespan,
                    window,
                    series_type,
                    expand_underlying,
                    order,
                    limit);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region -------------------- Polygon REST API Reference Data Endpoints --------------------

        public async Task<RestOptionsContract_Result> Options_Contract_Async(
            string options_ticker,
            DateTime? as_of = null)
        {
            try
            {
                // Normalize parameters
                options_ticker = NormalizeSymbol(options_ticker);

                if (!options_ticker.IsOptionSymbol())
                    throw new ArgumentException(options_ticker, nameof(options_ticker));

                string _as_of = as_of.HasValue ? as_of.Value.ToString("yyyy-MM-dd") : null;

                // Call async request method
                return await restClient.Options_Contract_Async(
                    options_ticker,
                    _as_of);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<RestOptionsContract_Result[]> Options_Contracts_Async(
            string underlying_ticker = null,
            PolygonFilterParams? underlyingTickerFilter = null,
            OptionType? contract_type = null,
            DateTime? expiration_date = null,
            PolygonFilterParams? expirationDateFilter = null,
            DateTime? as_of = null,
            double? strike_price = null,
            PolygonFilterParams? strikePriceFilter = null,
            bool? expired = null,
            PolygonOrder? order = null,
            int? limit = null,
            PolygonOptionsContractsSort? sort = null
            )
        {
            try
            {
                // Normalize parameters
                underlying_ticker = underlying_ticker != null ? NormalizeSymbol(underlying_ticker) : null;

                string _expiration_date = expiration_date.HasValue ? expiration_date.Value.ToString("yyyy-MM-dd") : null;
                string _as_of = as_of.HasValue ? as_of.Value.ToString("yyyy-MM-dd") : null;

                // Call async request method
                return await restClient.Options_Contracts_Async(
                    underlying_ticker,
                    underlyingTickerFilter,
                    contract_type,
                    _expiration_date,
                    expirationDateFilter,
                    _as_of,
                    strike_price,
                    strikePriceFilter,
                    expired,
                    order,
                    limit,
                    sort);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<RestTickers_Result[]> Tickers_Async(
            string ticker = null,
            PolygonFilterParams? tickerFilter = null,
            PolygonTickerType? type = null,
            PolygonMarket? market = null,
            string exchange = null,
            string CUSIP = null,
            string CIK = null,
            DateTime? date = null,
            string search = null,
            bool? active = null,
            PolygonOrder? order = null,
            int? limit = null,
            PolygonTickerSort? sort = null
            )
        {
            try
            {
                // Normalize parameters
                ticker = ticker != null ? NormalizeSymbol(ticker) : null;

                string _date = date.HasValue ? date.Value.ToString("yyyy-MM-dd") : null;

                // Call async request method
                return await restClient.Tickers_Async(
                    ticker,
                    tickerFilter,
                    type,
                    market,
                    exchange,
                    CUSIP,
                    CIK,
                    _date,
                    search,
                    active,
                    order,
                    limit,
                    sort);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<RestTickerDetail_Result> Ticker_Details_Async(
            string ticker,
            DateTime? date = null
            )
        {
            try
            {
                if (ticker == null)
                    throw new ArgumentException(ticker, nameof(ticker));

                // Normalize parameters
                ticker = NormalizeSymbol(ticker);

                string _date = date.HasValue ? date.Value.ToString("yyyy-MM-dd") : null;

                // Call async request method
                return await restClient.Ticker_Details_Async(
                    ticker,
                    _date);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Get a timeline of events for the entity associated with the given ticker, CUSIP, or Composite FIGI.
        /// </summary>
        /// <param name="id">Identifier of an asset. This can currently be a Ticker, CUSIP, or Composite FIGI.</param>
        /// <param name="types">A comma-separated list of the types of event to include. Currently ticker_change is the only supported event_type. Leave blank to return all supported event_types.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<RestTickerEvents_Result> Ticker_Events_Async(
            string id,
            List<string> types = null)
        {
            try
            {
                if (id == null)
                    throw new ArgumentException(id, nameof(id));

                // Normalize parameters

                // Can be a ticker, CUSIP, or FIGI... don't have anything to handle the latter 2 yet
                id = id.ToUpper();

                string _types = null;

                if (types != null)
                    if (types.Count > 0)
                    {
                        StringBuilder sb = new StringBuilder();
                        for (int i = 0; i < types.Count; i++)
                        {
                            sb.Append(types[i]);
                            if (i + 1 < types.Count)
                                sb.Append(",");
                        }
                        _types = sb.ToString();
                    }

                // Call async request method
                return await restClient.Ticker_Events_Async(
                    id,
                    _types);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<RestTickerNews_Result[]> Ticker_News_Async(
            string ticker = null,
            PolygonFilterParams? tickerFilter = null,
            DateTime? published_utc = null,
            PolygonFilterParams? publishedUtcFilter = null,
            PolygonOrder? order = null,
            int? limit = null,
            PolygonNewsSort? sort = null)
        {
            try
            {
                // Normalize parameters

                // Can be a ticker, CUSIP, or FIGI... don't have anything to handle the latter 2 yet
                if (ticker != null)
                    ticker = NormalizeSymbol(ticker);

                string _published_utc = published_utc.HasValue ? published_utc.Value.ToString("yyyy-MM-dd") : null;

                // Call async request method
                return await restClient.Ticker_News_Async(
                    ticker,
                    tickerFilter,
                    _published_utc,
                    publishedUtcFilter,
                    order,
                    limit,
                    sort);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<RestTickerTypes_Result[]> Ticker_Types_Async(
            PolygonMarket? asset_class = null,
            PolygonLocale? locale = null)
        {
            try
            {
                // Call async request method
                return await restClient.Ticker_Types_Async(
                    asset_class,
                    locale);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<RestMarketHolidays_Result[]> Market_Holidays_Async()
        {
            try
            {
                // Call async request method
                return await restClient.Market_Holidays_Async();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<RestMarketStatus_Response> Market_Status_Async()
        {
            try
            {
                // Call async request method
                return await restClient.Market_Status_Async();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<RestStockSplits_Result[]> Stock_Splits_Async(
            string ticker = null,
            PolygonFilterParams? tickerFilter = null,
            DateTime? execution_date = null,
            PolygonFilterParams? executionDateFilter = null,
            bool? reverse_split = null,
            PolygonOrder? order = null,
            int? limit = null,
            PolygonStockSplitSort? sort = null
            )
        {
            try
            {
                // Normalize parameters
                ticker = ticker != null ? NormalizeSymbol(ticker) : null;

                string _execution_date = execution_date.HasValue ? execution_date.Value.ToString("yyyy-MM-dd") : null;

                // Call async request method
                return await restClient.Stock_Splits_Async(
                    ticker,
                    tickerFilter,
                    _execution_date,
                    executionDateFilter,
                    reverse_split,
                    order,
                    limit,
                    sort);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<RestDividends_Result[]> Dividends_Async(
            string ticker = null,
            PolygonFilterParams? tickerFilter = null,
            DateTime? ex_dividend_date = null,
            PolygonFilterParams? exDateFilter = null,
            DateTime? record_date = null,
            PolygonFilterParams? recordDateFilter = null,
            DateTime? declaration_date = null,
            PolygonFilterParams? declarationDateFilter = null,
            DateTime? pay_date = null,
            PolygonFilterParams? payDateFilter = null,
            PolygonDividendFrequency? frequency = null,
            double? cash_amount = null,
            PolygonFilterParams? cashAmountFilter = null,
            PolygonDividendType? dividend_type = null,
            PolygonSort? order = null,
            int? limit = null,
            PolygonDividendSort? sort = null
            )
        {
            try
            {
                // Normalize parameters
                ticker = ticker != null ? NormalizeSymbol(ticker) : null;

                string _ex_dividend_date = ex_dividend_date.HasValue ? ex_dividend_date.Value.ToString("yyyy-MM-dd") : null;
                string _record_date = record_date.HasValue ? record_date.Value.ToString("yyyy-MM-dd") : null;
                string _declaration_date = declaration_date.HasValue ? declaration_date.Value.ToString("yyyy-MM-dd") : null;
                string _pay_date = pay_date.HasValue ? pay_date.Value.ToString("yyyy-MM-dd") : null;

                // Call async request method
                return await restClient.Dividends_Async(
                    ticker,
                    tickerFilter,
                    _ex_dividend_date,
                    exDateFilter,
                    _record_date,
                    recordDateFilter,
                    _declaration_date,
                    declarationDateFilter,
                    _pay_date,
                    payDateFilter,
                    frequency,
                    cash_amount,
                    cashAmountFilter,
                    dividend_type,
                    order,
                    limit,
                    sort);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<RestStockFinancials_Result[]> Stock_Financials_Async(
            string ticker = null,
            string cik = null,
            string company_name = null,
            PolygonCompanyNameFilter? companyNameFilter = null,
            string sic = null,
            DateTime? filing_date = null,
            PolygonFilterParams? filingDateFilter = null,
            PolygonFinancialsTimeframe? timeframe = null,
            bool? include_sources = null,
            PolygonOrder? order = null,
            int? limit = null,
            PolygonFinancialsSort? sort = null
            )
        {
            try
            {
                // Normalize parameters
                ticker = ticker != null ? NormalizeSymbol(ticker) : null;

                string _filing_date = filing_date.HasValue ? filing_date.Value.ToString("yyyy-MM-dd") : null;

                // Call async request method
                return await restClient.Stock_Financials_Async(
                    ticker,
                    cik,
                    company_name,
                    companyNameFilter,
                    sic,
                    _filing_date,
                    filingDateFilter,
                    timeframe,
                    include_sources,
                    order,
                    limit,
                    sort);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<RestConditions_Result[]> Conditions_Async(
            PolygonMarket? asset_class = null,
            PolygonConditionCodeDataType? data_type = null,
            int? id = null,
            int? sip = null,
            PolygonOrder? order = null,
            int? limit = null,
            PolygonConditionsSort? sort = null
            )
        {
            try
            {
                // Call async request method
                return await restClient.Conditions_Async(
                    asset_class,
                    data_type,
                    id,
                    sip,
                    order,
                    limit,
                    sort);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<RestExchange_Result[]> Exchanges_Async(
            PolygonMarket? asset_class = null,
            PolygonLocale? locale = null)
        {
            try
            {
                // Call async request method
                return await restClient.Exchanges_Async(
                    asset_class,
                    locale);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region -------------------- Polygon Socket API -------------------------------------------

        public SocketHandler Aggregate_Second_Bars_Streaming(string symbol, PolygonConnectionEndpoint endpoint)
        {
            return GetClient(endpoint).Aggregate_Second_Bars_Streaming(symbol, true);
        }

        public SocketHandler Aggregate_Minute_Bars_Streaming(string symbol, PolygonConnectionEndpoint endpoint)
        {
            return GetClient(endpoint).Aggregate_Minute_Bars_Streaming(symbol, true);
        }

        public SocketHandler Trades_Streaming(string symbol, PolygonConnectionEndpoint endpoint)
        {
            return GetClient(endpoint).Trades_Streaming(symbol, true);
        }

        public SocketHandler Quotes_Streaming(string symbol, PolygonConnectionEndpoint endpoint)
        {
            return GetClient(endpoint).Quotes_Streaming(symbol, true);
        }

        #endregion
    }

}
