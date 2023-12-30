using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PolygonApiClient.Helpers;
using PolygonApiClient.Properties;
using PolygonApiClient.RESTClient;
using PolygonApiClient.WebSocketsClient;

namespace PolygonApiClient
{
    //
    // This class controls both the Socket and REST clients and provides all top-level functionality to external clients
    //
    public class PolygonClient
    {
        PolygonRestClient restClient { get; set; }
        PolygonSocketClient socketClientStocks { get; set; }
        PolygonSocketClient socketClientOptions { get; set; }

        private readonly string ApiKey;

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

        public PolygonClient(string apiKey)
        {
            ApiKey = apiKey;

            _initPolygonClients();
        }

        private void _initPolygonClients()
        {
            // Instantiate the clients which will handle different requests

            restClient = new PolygonRestClient(ApiKey);

            socketClientStocks = new PolygonSocketClient(ApiKey, PolygonConnectionEndpoint.stocks);

            socketClientOptions = new PolygonSocketClient(ApiKey, PolygonConnectionEndpoint.options);
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

        public async Task<RestAggregatesBars_Response> Aggregates_Bars_Async(
            string symbol,
            int multiplier,
            PolygonTimespan timespan,
            DateTime from,
            DateTime to,
            bool adjusted = true,
            PolygonSort sort = PolygonSort.asc,
            int limit = 5000)
        {
            throw new NotImplementedException();
        }

        public async Task<RestGroupedDailyBars_Response> Grouped_Daily_Bars_Async(
            DateTime date,
            bool adjusted = true,
            bool include_otc = false)
        {
            throw new NotImplementedException();
        }

        public async Task<DailyOpenClose_Response> Daily_Open_Close_Async(
            string symbol,
            DateTime date,
            bool adjusted = true)
        {
            throw new NotImplementedException();
        }

        public async Task<PreviousClose_Response> Previous_Close_Async(
            string symbol,
            bool adjusted = true)
        {
            throw new NotImplementedException();
        }

        public async Task<RestTrades_Response> Trades_Async(
            string symbol,
            DateTime timestamp,
            PolygonFilterParams timestampFilter = PolygonFilterParams.NotSet,
            PolygonOrder order = PolygonOrder.asc,
            int limit = 10,
            string sort = "timestamp")
        {
            throw new NotImplementedException();
        }

        public async Task<RestLastTrade_Response> Last_Trade_Async(
            string symbol)
        {
            throw new NotImplementedException();
        }

        public async Task<RestQuotes_Response> Quotes_Async(
            string symbol,
            DateTime timestamp,
            PolygonFilterParams timestampFilter = PolygonFilterParams.NotSet,
            PolygonOrder order = PolygonOrder.asc,
            int limit = 10,
            string sort = "timestamp")
        {
            throw new NotImplementedException();
        }

        public async Task<RestLastQuote_Response> Last_Quote_Async(
            string symbol)
        {
            throw new NotImplementedException();
        }


        // ---------- Snapshots ----------

        public async Task<RestTickerSnapshot_Response> All_Tickers_Async(
            List<string> tickers,
            bool include_otc = false)
        {
            throw new NotImplementedException();
        }

        public async Task<RestTickerSnapshot_Response> Gainers_Losers_Async(
            PolygonGainersLosers direction = PolygonGainersLosers.gainers,
            bool include_otc = false)
        {
            throw new NotImplementedException();
        }

        public async Task<RestTickerSnapshot_Response> Ticker_Async(
            string stocksTicker)
        {
            // This method calls the same endpoint as All_Tickers but with a single ticker defined. Return data is exactly the same

            return await All_Tickers_Async(new List<string> { stocksTicker });
        }

        public async Task<RestOptionContract_Response> Option_Contract_Async(
            string underlyingAsset,
            string optionContract)
        {
            throw new NotImplementedException();
        }
        public async Task<RestOptionsChain_Response> Options_Chain_Async(
            string underlyingAsset,
            double? strike_price = null,
            PolygonFilterParams strikePriceFilter = PolygonFilterParams.NotSet,
            DateTime? expirationDate = null,
            PolygonFilterParams expirationDateFilter = PolygonFilterParams.NotSet,
            OptionType contract_type = OptionType.NotSet,
            PolygonOrder order = PolygonOrder.asc,
            int limit = 10,
            PolygonOptionsChainSort sort = PolygonOptionsChainSort.NotSet)
        {
            throw new NotImplementedException();
        }

        public async Task<object> Universal_Snapshot_Async()
        {
            // This is a bizarre and complicated call that I am not going to bother with for a while
            throw new NotImplementedException();
        }

        // ---------- Technical Indicators ----------

        public async Task<RestMovingAverage_Response> Simple_Moving_Average_Async(
            string stockTicker,
            DateTime timestamp,
            PolygonFilterParams timestampFilter,
            PolygonTimespan timespan,
            int window,
            PolygonAggregateSeriesType series_type,
            PolygonOrder order = PolygonOrder.desc,
            bool expand_underlying = false,
            bool adjusted = true,
            int limit = 10)
        {
            throw new NotImplementedException();
        }

        public async Task<RestMovingAverage_Response> Exponential_Moving_Average_Async(
            string stockTicker,
            DateTime timestamp,
            PolygonFilterParams timestampFilter,
            PolygonTimespan timespan,
            int window,
            PolygonAggregateSeriesType series_type,
            PolygonOrder order = PolygonOrder.desc,
            bool expand_underlying = false,
            bool adjusted = true,
            int limit = 10)
        {
            throw new NotImplementedException();
        }

        public async Task<RestMACD_Response> Moving_Average_Convergence_Divergence_Async(
            string stockTicker,
            DateTime timestamp,
            PolygonFilterParams timestampFilter,
            PolygonTimespan timespan,
            int window,
            PolygonAggregateSeriesType series_type,
            PolygonOrder order = PolygonOrder.desc,
            bool expand_underlying = false,
            bool adjusted = true,
            int limit = 10)
        {
            throw new NotImplementedException();
        }

        public async Task<RestMACD_Response> Relative_Strength_Index_Async(
            string stockTicker,
            DateTime timestamp,
            PolygonFilterParams timestampFilter,
            PolygonTimespan timespan,
            int window,
            PolygonAggregateSeriesType series_type,
            PolygonOrder order = PolygonOrder.desc,
            bool expand_underlying = false,
            bool adjusted = true,
            int limit = 10)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region -------------------- Polygon REST API Reference Data Endpoints --------------------

        public async Task<RestTickers_Response> Tickers_Async(
            string ticker = "",
            PolygonFilterParams tickerFilter = PolygonFilterParams.NotSet,
            PolygonTickerType type = PolygonTickerType.None,
            PolygonMarket market = PolygonMarket.None,
            string exchange = "",
            string CUSIP = "",
            string CIK = "",
            DateTime? date = null,
            string search = "",
            bool active = true,
            PolygonOrder order = PolygonOrder.asc,
            PolygonTickerSort sort = PolygonTickerSort.ticker
            )
        {
            throw new NotImplementedException();
        }

        public async Task<RestTickerDetail_Response> Ticker_Details_Async(
            string ticker,
            DateTime? date = null
            )
        {
            throw new NotImplementedException();
        }

        public async Task<RestTickerEvents_Response> Ticker_Events_Async(
            string id,
            List<string> types)
        {
            throw new NotImplementedException();
        }

        public async Task<RestTickerNews_Response> Ticker_News_Async(
            string ticker,
            PolygonFilterParams tickerFilter,
            DateTime published_utc,
            PolygonFilterParams publishedUtcFilter,
            PolygonOrder order = PolygonOrder.asc,
            int limit = 10,
            string sort = "published_utc")
        {
            throw new NotImplementedException();
        }

        public async Task<RestTickerTypes_Response> Ticker_Types_Async(
            PolygonMarket asset_class = PolygonMarket.None,
            PolygonLocale locale = PolygonLocale.None)
        {
            throw new NotImplementedException();
        }

        public async Task<RestMarketHolidays_Response[]> Market_Holidays_Async()
        {
            throw new NotImplementedException();
        }

        public async Task<RestMarketStatus_Response> Market_Status_Asyc()
        {
            throw new NotImplementedException();
        }

        public async Task<RestStockSplits_Response> Stock_Splits_Async(
            string ticker = "",
            PolygonFilterParams tickerFilter = PolygonFilterParams.NotSet,
            DateTime? executionDate = null,
            PolygonFilterParams executionDateFilter = PolygonFilterParams.NotSet,
            bool? reverse_split = null,
            PolygonOrder order = PolygonOrder.asc,
            int limit = 10,
            PolygonStockSplitSort sort = PolygonStockSplitSort.execution_date
            )
        {
            throw new NotImplementedException();
        }
        public async Task<RestDividends_Response> Dividends_Async(
            string ticker = "",
            PolygonFilterParams tickerFilter = PolygonFilterParams.NotSet,
            DateTime? ex_dividend_date = null,
            PolygonFilterParams exDateFilter = PolygonFilterParams.NotSet,
            DateTime? record_date = null,
            PolygonFilterParams recordDateFilter = PolygonFilterParams.NotSet,
            DateTime? declaration_date = null,
            PolygonFilterParams declarationDateFilter = PolygonFilterParams.NotSet,
            DateTime? pay_date = null,
            PolygonFilterParams payDateFilter = PolygonFilterParams.NotSet,
            PolygonDividendFrequency frequency = PolygonDividendFrequency.NotSet,
            double? cash_amount = null,
            PolygonFilterParams cashAmountFilter = PolygonFilterParams.NotSet,
            PolygonDividendType dividend_type = PolygonDividendType.NotSet,
            PolygonSort order = PolygonSort.asc,
            int limit = 10,
            PolygonDividendSort sort = PolygonDividendSort.ticker
            )
        {
            throw new NotImplementedException();
        }

        public async Task<RestStockFinancials_Response> Stock_Financials_Async(
            string ticker,
            DateTime period_of_report_date,
            PolygonFilterParams periodFilter = PolygonFilterParams.NotSet,
            PolygonFinancialsTimeframe timeframe = PolygonFinancialsTimeframe.NotSet,
            bool include_sources = false,
            PolygonOrder order = PolygonOrder.asc,
            int limit = 10,
            PolygonFinancialsSort sort = PolygonFinancialsSort.filing_date
            )
        {
            throw new NotImplementedException();
        }

        public async Task<RestConditions_Response> Conditions_Async(
            PolygonMarket asset_class,
            PolygonConditionCodeDataType data_type = PolygonConditionCodeDataType.None,
            int id = -1,
            int SIP = -1,
            PolygonOrder order = PolygonOrder.asc,
            int limit = 10,
            PolygonConditionsSort sort = PolygonConditionsSort.NotSet
            )
        {
            throw new NotImplementedException();
        }

        public async Task<RestExchange_Response> Exchanges_Async(
            PolygonMarket asset_class,
            PolygonLocale locale)
        {
            throw new NotImplementedException();
        }

        public async Task<RestOptionsContract_Response> Options_Contract_Async(
            string options_ticker,
            DateTime? as_of = null)
        {
            throw new NotImplementedException();
        }

        public async Task<RestOptionsContract_Response> Options_Contracts_Async(
            string underlying_ticker = "",
            PolygonFilterParams underlyingTickerFilter = PolygonFilterParams.NotSet,
            OptionType contract_type = OptionType.NotSet,
            DateTime? expiration_date = null,
            PolygonFilterParams expirationDateFilter = PolygonFilterParams.NotSet,
            DateTime? as_of = null,
            double? strike_price = null,
            PolygonFilterParams strikePriceFilter = PolygonFilterParams.NotSet,
            bool expired = false,
            PolygonOrder order = PolygonOrder.asc,
            int limit = 10,
            PolygonOptionsContractsSort sort = PolygonOptionsContractsSort.NotSet
            )
        {
            throw new NotImplementedException();
        }

        #endregion   

    }

}
