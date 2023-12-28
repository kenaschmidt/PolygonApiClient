using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataFarm_Polygon.Models;
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


        public PolygonClient(string apiKey)
        {
            ApiKey = apiKey;

            _initPolygonClients();
        }


        private void _initPolygonClients()
        {
            // Instatiate the clients which will handle different requests

            restClient = new PolygonRestClient(ApiKey);

            socketClientStocks = new PolygonSocketClient(ApiKey, ConnectionEndpoint.stocks);

            socketClientOptions = new PolygonSocketClient(ApiKey, ConnectionEndpoint.options);
        }

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


        #region Polygon REST API Market Data Endpoints - STOCKS

        public RestAggregatesBars_Response Aggregates_Bars(
            string stocksTicker,
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
        public RestGroupedDailyBars_Response Grouped_Daily_Bars(
            DateTime date,
            bool adjusted = true,
            bool include_otc = false)
        {
            throw new NotImplementedException();
        }

        public DailyOpenClose_Response Daily_Open_Close(
            string stocksTicker,
            DateTime date,
            bool adjusted = true)
        {
            throw new NotImplementedException();
        }

        public PreviousClose_Response Previous_Close(
            string stocksTicker,
            bool adjusted = true)
        {
            throw new NotImplementedException();
        }

        public RestTrades_Response Trades(
            string stocksTicker,
            DateTime timestamp,
            PolygonFilterParams timestampFilter = PolygonFilterParams.none,
            PolygonOrder order = PolygonOrder.asc,
            int limit = 10,
            string sort = "timestamp")
        {
            throw new NotImplementedException();
        }

        public RestLastTrade_Response Last_Trade(
            string stocksTicker)
        {
            throw new NotImplementedException();
        }

        public RestQuotes_Response Quotes_NBBO(
            string stocksTicker,
            DateTime timestamp,
            PolygonFilterParams timestampFilter = PolygonFilterParams.none,
            PolygonOrder order = PolygonOrder.asc,
            int limit = 10,
            string sort = "timestamp")
        {
            throw new NotImplementedException();
        }

        public RestLastQuote_Response Last_Quote(
            string stocksTicker)
        {
            throw new NotImplementedException();
        }

        //
        // SNAPSHOTS
        //




        #endregion

        #region REST Requests (Static) OLD

        ///// <summary>
        ///// Loads the current, active options chain for a stock (non-expired)
        ///// </summary>
        ///// <param name="stock"></param>
        ///// <returns></returns>
        //public async Task LoadOptionChainAsync(Stock stock)
        //{
        //    OnSystemMessage($"Requesting options chain data for {stock.Symbol}");

        //    var result = await restClient.OptionsChainAsync(stock);

        //    OnSystemMessage($"Received {result.Count} results...");

        //    foreach (var item in result)
        //    {
        //        var newContract = new Option(
        //            stock,
        //            DateTime.ParseExact(item.details.expiration_date, "yyyy-MM-dd", CultureInfo.CurrentCulture),
        //            item.details.contract_type == "call" ? OptionType.Call : OptionType.Put,
        //            item.details.strike_price,
        //            item.details.shares_per_contract);

        //        stock.OptionChain.AddOption(newContract);
        //    }
        //    OnSystemMessage($"Populated {stock.OptionChain.Options.Count} contracts.");
        //}

        ///// <summary>
        ///// Loads all expired options for a stock (starting from startDate)
        ///// </summary>
        ///// <param name="stock"></param>
        ///// <param name="startDate">First date (inclusive) to look for expiries</param>
        ///// <returns></returns>
        //public async Task LoadExpiredOptionChainAsync(Stock stock, DateTime startDate)
        //{

        //    DateTime requestDate = DateTime.Today.AddDays(-1);

        //    while (requestDate >= startDate)
        //    {

        //        OnSystemMessage($"Requesting expired options chain data for {stock.Symbol} with expiry {requestDate}");

        //        var result = await restClient.ExpiredOptionsChainAsync(stock, requestDate);

        //        OnSystemMessage($"Received {result.Count} results...");

        //        foreach (var item in result)
        //        {
        //            var newContract = new Option(
        //                stock,
        //                DateTime.ParseExact(item.expiration_date, "yyyy-MM-dd", CultureInfo.CurrentCulture),
        //                item.contract_type == "call" ? OptionType.Call : OptionType.Put,
        //                item.strike_price,
        //                item.shares_per_contract);

        //            stock.OptionChain.AddOption(newContract);
        //        }

        //        requestDate = requestDate.AddDays(-1);
        //    }

        //    OnSystemMessage($"Populated {stock.OptionChain.Options.Count} contracts.");
        //}

        ///// <summary>
        ///// Loads aggregate prie bar data for a security
        ///// </summary>
        ///// <param name="security"></param>
        ///// <param name="multiplier"></param>
        ///// <param name="barSize"></param>
        ///// <param name="fromDate"></param>
        ///// <param name="toDate"></param>
        ///// <returns></returns>
        //public async Task LoadPriceBarsAsync(Security security, int multiplier, BarSize barSize, DateTime fromDate, DateTime toDate)
        //{
        //    OnSystemMessage($"Requesting price data for {security.Symbol}: {multiplier} {barSize.ToString()} from {fromDate:MM/dd/yyyy HH:mm:ss} to {toDate:MM/dd/yyyy HH:mm:ss}");

        //    var result = await restClient.AggregateBarSnapshotAsync(security, multiplier, barSize, fromDate, toDate);

        //    OnSystemMessage($"Received {result.Count} bars...");

        //    List<PriceBar> newBars = new List<PriceBar>();

        //    foreach (var item in result)
        //    {
        //        var newBar = new PriceBar(
        //            security,
        //            item.TimestampStart_ms.UnixMillisecondsToEst(),
        //            barSize,
        //            multiplier,
        //            item.Open,
        //            item.High,
        //            item.Low,
        //            item.Close,
        //            (int)item.Volume);

        //        newBars.Add(newBar);
        //    }

        //    security.AddPriceBars(newBars);

        //    OnSystemMessage($"Added {newBars.Count} bars to security");
        //}

        ///// <summary>
        ///// Loads quotes and trades for a security for a specific day
        ///// </summary>
        ///// <param name="security"></param>
        ///// <param name="day"></param>
        //public async Task LoadQuotesAndTradesAsync(Security security, DateTime day)
        //{
        //    // Get lists of quotes and trades seperately

        //    var qTicks = await LoadQuoteTicksAsync(security, day);

        //    var tTicks = await LoadTradeTicksAsync(security, day);

        //    List<Tick> allTicks = new List<Tick>();

        //    OnSystemMessage($"Combining {(qTicks).Count} and {(tTicks).Count}");

        //    allTicks.AddRange(qTicks);
        //    allTicks.AddRange(tTicks);

        //    // Add the new ticks to the security - clear the existing data so only 1 day is loaded at a time

        //    OnSystemMessage($"Adding combined total {allTicks.Count} ticks");

        //    security.AddTicks(allTicks, true);

        //    OnSystemMessage($"Added {allTicks.Count} ticks to {security.Symbol}");
        //}

        ///// <summary>
        ///// Loads all trades for a given security for a single day
        ///// </summary>
        ///// <param name="security"></param>
        ///// <param name="day"></param>
        ///// <returns></returns>
        //public async Task<List<TradeTick>> LoadTradeTicksAsync(Security security, DateTime day)
        //{
        //    OnSystemMessage($"Requesting trade data for {security.Symbol} on {day:yy-MM-dd}");

        //    var result = await restClient.TradeSnapshotAsync(security, day);

        //    OnSystemMessage($"Received {result.Count} ticks...");

        //    List<TradeTick> newTicks = new List<TradeTick>();

        //    foreach (var item in result)
        //    {
        //        var newTick = new TradeTick(security)
        //        {
        //            // !!!!
        //            // NOTE: The timestamp for this call is in NANOSECONDS. Convert to MILLISECONDS manually for consistency
        //            // !!!!

        //            Timestamp_ms = item.SIP_Timestamp_Ns.NanoToMilli(),
        //            TradePrice = item.Price,
        //            TradeSize = item.Size
        //        };

        //        newTick.ExchangeCode = (Exchange)item.Exchange;

        //        if (item.Conditions != null)
        //            foreach (var code in item.Conditions)
        //                newTick.ConditionCodes.Add((TradeConditions)code);

        //        newTicks.Add(newTick);
        //    }

        //    return newTicks;
        //}

        ///// <summary>
        ///// Loads all quote ticks for a given security for a single day
        ///// </summary>
        ///// <returns></returns>
        //public async Task<List<QuoteTick>> LoadQuoteTicksAsync(Security security, DateTime day)
        //{
        //    OnSystemMessage($"Requesting quote data for {security.Symbol} on {day:yy-MM-dd}");

        //    var result = await restClient.QuoteSnapshotAsync(security, day);

        //    OnSystemMessage($"Received {result.Count} ticks...");

        //    List<QuoteTick> newTicks = new List<QuoteTick>();

        //    foreach (var item in result)
        //    {
        //        var newTick = new QuoteTick(security)
        //        {
        //            // !!!!
        //            // NOTE: The timestamp for this call is in NANOSECONDS. Convert to MILLISECONDS manually for consistency
        //            // !!!!

        //            Timestamp_ms = item.Participant_Timestamp_Ns.NanoToMilli(),
        //            BidPrice = item.Bid_Price,
        //            BidSize = item.Bid_Size,
        //            BidExchangeCode = (Exchange)item.Bid_Exchange,
        //            AskPrice = item.Ask_Price,
        //            AskSize = item.Ask_Size,
        //            AskExchangeCode = (Exchange)item.Ask_Exchange
        //        };

        //        if (item.Conditions != null)
        //            foreach (var code in item.Conditions)
        //                newTick.ConditionCodes.Add((QuoteConditions)code);

        //        if (item.Indicators != null)
        //            foreach (var code in item.Indicators)
        //                newTick.IndicatorCodes.Add((QuoteIndicators)code);

        //        newTicks.Add(newTick);
        //    }

        //    return newTicks;
        //}

        #endregion

        #region SOCKET Requests (Live)



        #endregion

    }

}
