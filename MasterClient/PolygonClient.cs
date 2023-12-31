﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
        protected PolygonRestClient restClient { get; set; }
        protected PolygonSocketClient socketClientStocks { get; set; }
        protected PolygonSocketClient socketClientOptions { get; set; }
        public PolygonSubscriptionSettings SubscriptionSettings { get; }

        protected readonly string API_Key;

        #region Events

        public event EventHandler<string> SystemMessage;
        private void OnSystemMessage(string msg)
        {
            SystemMessage?.Invoke(this, msg);
        }

        public event EventHandler SocketConnectionChanged;
        private void OnSocketConnectionChanged()
        {
            SocketConnectionChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Public Properties

        private bool _Connected_StocksSocket { get; set; }
        public bool Connected_StocksSocket
        {
            get => _Connected_StocksSocket;
            set
            {
                if (_Connected_StocksSocket != value)
                {
                    _Connected_StocksSocket = value;
                    OnSocketConnectionChanged();
                }
            }
        }

        private bool _Connected_OptionSocket { get; set; }
        public bool Connected_OptionSocket
        {
            get => _Connected_OptionSocket;
            set
            {
                if (_Connected_OptionSocket != value)
                {
                    _Connected_OptionSocket = value;
                    OnSocketConnectionChanged();
                }
            }
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
            // Instantiate the clients which will handle different requests

            restClient = new PolygonRestClient(API_Key);

            socketClientStocks = new PolygonSocketClient(API_Key, PolygonConnectionEndpoint.stocks);

            socketClientOptions = new PolygonSocketClient(API_Key, PolygonConnectionEndpoint.options);
        }

        #endregion

        #region Connection Management

        public async void ConnectSocketsAsync()
        {
            try
            {
                await socketClientStocks.OpenAsync();
                Connected_StocksSocket = true;
                OnSystemMessage("Connected Socket: Stocks");
            }
            catch (Exception)
            {
                Connected_StocksSocket = false;
                OnSystemMessage("Could not connect socket: Stocks");
            }

            try
            {
                await socketClientOptions.OpenAsync();
                Connected_OptionSocket = true;
                OnSystemMessage("Connected Socket: Options");
            }
            catch (Exception)
            {
                Connected_OptionSocket = false;
                OnSystemMessage("Could not connect socket: Options");
            }
        }
        public async void DisconnectSocketsAsync()
        {
            try
            {
                await socketClientStocks.CloseAsync();
                Connected_StocksSocket = false;
                OnSystemMessage("Disconnected Socket: Stocks");
            }
            catch (Exception)
            {
                Connected_StocksSocket = false;
                OnSystemMessage("Could not disconnect socket: Stocks");
            }

            try
            {
                await socketClientOptions.CloseAsync();
                Connected_OptionSocket = false;
                OnSystemMessage("Disconnected Socket: Options");
            }
            catch (Exception)
            {
                Connected_OptionSocket = false;
                OnSystemMessage("Could not disconnect socket: Options");
            }
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
            bool? adjusted,
            PolygonSort? sort,
            int? limit)
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
            bool? adjusted,
            bool? include_otc)
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
            bool? adjusted)
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
            bool? adjusted)
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
            DateTime? timestamp,
            PolygonFilterParams? timestampFilter,
            PolygonOrder? order,
            int? limit,
            PolygonTradeQuoteSort? sort)
        {
            try
            {
                // Normalize parameters
                symbol = NormalizeSymbol(symbol);
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
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<RestLastTrade_Result[]> Last_Trade_Async(
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
            DateTime? timestamp,
            PolygonFilterParams? timestampFilter,
            PolygonOrder? order,
            int? limit,
            PolygonTradeQuoteSort? sort)
        {
            try
            {
                // Normalize parameters
                symbol = NormalizeSymbol(symbol);
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
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<RestLastQuote_Result[]> Last_Quote_Async(
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
            List<string> tickers,
            bool? include_otc)
        {
            try
            {
                // Normalize parameters

                // Create a CSV string of tickers to pass
                StringBuilder sb = new StringBuilder();
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
            bool? include_otc)
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

        public async Task<RestOptionContract_Result[]> Option_Contract_Async(
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
            double? strike_price,
            PolygonFilterParams? strikePriceFilter,
            DateTime? expirationDate,
            PolygonFilterParams? expirationDateFilter,
            OptionType? contract_type,
            PolygonOrder? order,
            int? limit,
            PolygonOptionsChainSort? sort)
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
            DateTime? timestamp,
            PolygonFilterParams? timestampFilter,
            PolygonTimespan? timespan,
            int? window,
            PolygonAggregateSeriesType? series_type,
            PolygonOrder? order,
            bool? expand_underlying,
            bool? adjusted,
            int? limit)
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
            DateTime? timestamp,
            PolygonFilterParams? timestampFilter,
            PolygonTimespan? timespan,
            int? window,
            PolygonAggregateSeriesType? series_type,
            PolygonOrder? order,
            bool? expand_underlying,
            bool? adjusted,
            int? limit)
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
            DateTime? timestamp,
            PolygonFilterParams? timestampFilter,
            PolygonTimespan? timespan,
            bool? adjusted,
            int? short_window,
            int? long_window,
            int? signal_window,
            PolygonAggregateSeriesType? series_type,
            bool? expand_underlying,
            PolygonOrder? order,
            int? limit)
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
            DateTime? timestamp,
            PolygonFilterParams? timestampFilter,
            PolygonTimespan? timespan,
            int? window,
            PolygonAggregateSeriesType? series_type,
            bool? expand_underlying,
            PolygonOrder? order,
            int? limit)
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

        public async Task<RestOptionsContract_Result[]> Options_Contract_Async(
            string options_ticker,
            DateTime? as_of)
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
            string underlying_ticker,
            PolygonFilterParams? underlyingTickerFilter,
            OptionType? contract_type,
            DateTime? expiration_date,
            PolygonFilterParams? expirationDateFilter,
            DateTime? as_of,
            double? strike_price,
            PolygonFilterParams? strikePriceFilter,
            bool? expired,
            PolygonOrder? order,
            int? limit,
            PolygonOptionsContractsSort? sort
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
            string ticker,
            PolygonFilterParams? tickerFilter,
            PolygonTickerType? type,
            PolygonMarket? market,
            string exchange,
            string CUSIP,
            string CIK,
            DateTime? date,
            string search,
            bool? active,
            PolygonOrder? order,
            int? limit,
            PolygonTickerSort? sort
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

        public async Task<RestTickerDetail_Result[]> Ticker_Details_Async(
            string ticker,
            DateTime? date
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
        public async Task<RestTickerEvents_Result[]> Ticker_Events_Async(
            string id,
            List<string> types)
        {
            try
            {
                if (id == null)
                    throw new ArgumentException(id, nameof(id));

                // Normalize parameters

                // Can be a ticker, CUSIP, or FIGI... don't have anything to handle the latter 2 yet
                id = id.ToUpper();

                string _types = null;

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
            string ticker,
            PolygonFilterParams? tickerFilter,
            DateTime? published_utc,
            PolygonFilterParams? publishedUtcFilter,
            PolygonOrder? order,
            int? limit,
            PolygonNewsSort? sort)
        {
            try
            {
                // Normalize parameters

                // Can be a ticker, CUSIP, or FIGI... don't have anything to handle the latter 2 yet
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
            PolygonMarket? asset_class,
            PolygonLocale? locale)
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

        public async Task<RestMarketHolidays_Response> Market_Holidays_Async()
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
            string ticker,
            PolygonFilterParams? tickerFilter,
            DateTime? execution_date,
            PolygonFilterParams? executionDateFilter,
            bool? reverse_split,
            PolygonOrder? order,
            int? limit,
            PolygonStockSplitSort? sort
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
            string ticker,
            PolygonFilterParams? tickerFilter,
            DateTime? ex_dividend_date,
            PolygonFilterParams? exDateFilter,
            DateTime? record_date,
            PolygonFilterParams? recordDateFilter,
            DateTime? declaration_date,
            PolygonFilterParams? declarationDateFilter,
            DateTime? pay_date,
            PolygonFilterParams? payDateFilter,
            PolygonDividendFrequency? frequency,
            double? cash_amount,
            PolygonFilterParams? cashAmountFilter,
            PolygonDividendType? dividend_type,
            PolygonSort? order,
            int? limit,
            PolygonDividendSort? sort
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
            string ticker,
            string cik,
            string company_name,
            PolygonCompanyNameFilter? companyNameFilter,
            string sic,
            DateTime? filing_date,
            PolygonFilterParams? filingDateFilter,
            PolygonFinancialsTimeframe? timeframe,
            bool? include_sources,
            PolygonOrder? order,
            int? limit,
            PolygonFinancialsSort? sort
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
            PolygonMarket? asset_class,
            PolygonConditionCodeDataType? data_type,
            int? id,
            int? sip,
            PolygonOrder? order,
            int? limit,
            PolygonConditionsSort? sort
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
            PolygonMarket? asset_class,
            PolygonLocale? locale)
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

    }

}
