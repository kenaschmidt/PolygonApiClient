using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using System.Threading;
using System.Collections.Concurrent;
using DataFarm_Polygon.Models;
using PolygonApiClient.WebSocketsClient;
using System.Security.Permissions;

namespace PolygonApiClient.RESTClient
{
    internal class PolygonRestClient
    {
        public event EventHandler<MessageReceivedEventArgs> MessageReceived;
        private void OnMessageReceived(string message)
        {
            MessageReceived?.Invoke(this, new MessageReceivedEventArgs(message));
        }

        private HttpClient httpClient { get; set; }

        private string APIKey { get; }

        public bool Connected { get; private set; }

        // 5 year lookback for current plan
        public static DateTime PolygonEarliestDataRequestLimit => (DateTime.Today.AddYears(-5));

        public PolygonRestClient(string apiKey)
        {
            APIKey = apiKey;

            _initHttpClient();
            _initRequestQueue();
        }

        private void _initHttpClient()
        {
            httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(@"https://api.polygon.io");
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {APIKey}");
            httpClient.Timeout = Timeout.InfiniteTimeSpan;
        }

        #region Request Queuing

        //
        // Request Queue Settings
        //

        private const int MaxRequestsPerSec = 100;

        private ConcurrentQueue<Task> RequestQueue { get; set; }               

        private System.Timers.Timer RequestTimer { get; set; }

        private void _initRequestQueue()
        {
            RequestQueue = new ConcurrentQueue<Task>();
            RequestTimer = new System.Timers.Timer()
            {
                Interval = (1000 / MaxRequestsPerSec)
            };
            RequestTimer.Elapsed += (s, e) =>
            {
                if (RequestQueue.TryDequeue(out Task request))
                {
                    request.RunSynchronously();
                }
            };
            RequestTimer.Start();
        }

        /// <summary>
        /// Before making a REST request, methods await a RequestToken which will return when the request can be processed.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private bool RequestToken(out Task token)
        {
            token = new Task(() => { });
            RequestQueue.Enqueue(token);
            return true;
        }

        #endregion
               

        #region Polygon REST Requests

        //// Request Trade Snapshot
        //public async Task<List<RestTrades_Result>> TradeSnapshotAsync(Security security, DateTime day)
        //{
        //    string symbol = security.PolygonIOSymbol();

        //    // Form request string
        //    var reqStr = $@"/v3/trades/{symbol.ToUpper()}?timestamp={day.Date:yyyy-MM-dd}&order=desc&limit=50000&sort=timestamp";

        //    var ret = new List<RestTrades_Result>();

        //    // While we are receiving follow-on request URLs
        //    while (reqStr != "done")
        //    {
        //        // Wait for a local request token
        //        if (RequestToken(out Task token))
        //            await token;

        //        // Wait on the HTTP response
        //        var response = await httpClient.GetAsync(reqStr);

        //        // If response is good
        //        if (response.IsSuccessStatusCode)
        //        {
        //            // Read response from the server
        //            var replyString = await response.Content.ReadAsStringAsync();

        //            // Convert the string response to JSON model
        //            var replyObject = JsonConvert.DeserializeObject<RestTrades_Response>(replyString);

        //            if (replyObject.Results.Count() > 0)
        //            {
        //                ret.AddRange(replyObject.Results);
        //            }

        //            // If there is a follow-on request, get the URL
        //            if (replyObject.next_url != null)
        //                reqStr = replyObject.next_url;
        //            else
        //                reqStr = "done";
        //        }
        //        else
        //        {
        //            Console.WriteLine($"Error in request: {reqStr}");
        //            reqStr = "done";
        //        }
        //    }

        //    return ret;
        //}

        //// Request Quote Snapshot
        //public async Task<List<RestQuotes_Result>> QuoteSnapshotAsync(Security security, DateTime day)
        //{
        //    string symbol = security.PolygonIOSymbol();

        //    // Form request string
        //    var reqStr = $@"/v3/quotes/{symbol.ToUpper()}?timestamp={day:yyyy-MM-dd}&order=desc&limit=50000&sort=timestamp";

        //    var ret = new List<RestQuotes_Result>();

        //    // While we are receiving follow-on request URLs
        //    while (reqStr != "done")
        //    {
        //        // Wait for a local request token
        //        if (RequestToken(out Task token))
        //            await token;

        //        // Wait on the HTTP response
        //        var response = await httpClient.GetAsync(reqStr);

        //        if (response.IsSuccessStatusCode)
        //        {
        //            // Read response from the server
        //            var replyString = await response.Content.ReadAsStringAsync();

        //            // Convert the string response to JSON model
        //            var replyObject = JsonConvert.DeserializeObject<RestQuotes_Response>(replyString);

        //            if (replyObject.results.Count() > 0)
        //            {
        //                ret.AddRange(replyObject.results);
        //            }

        //            // If there is a follow-on request, get the URL
        //            if (replyObject.next_url != null)
        //                reqStr = replyObject.next_url;
        //            else
        //                reqStr = "done";
        //        }
        //        else
        //        {
        //            Console.WriteLine($"Error in request: {reqStr}");
        //            reqStr = "done";
        //        }
        //    }

        //    return ret;
        //}

        //// Request Aggregate Bar Snapshot
        //public async Task<List<RestAggregatesBars_Result>> AggregateBarSnapshotAsync(Security security, int multiplier, BarSize timeSpan, DateTime fromDate, DateTime toDate, bool adjusted = true, bool descending = true, int limit = 50000)
        //{
        //    string symbol = security.PolygonIOSymbol();

        //    var ret = new List<RestAggregatesBars_Result>();

        //    // Form request string
        //    var reqStr = $@"/v2/aggs/ticker/{symbol.ToUpper()}/range/{multiplier}/{timeSpan.ToString()}/{fromDate:yyyy-MM-dd}/{toDate:yyyy-MM-dd}?limit={limit}&adjusted={(adjusted ? "true" : "false")}&sort={(descending ? "desc" : "asc")}";

        //    Console.WriteLine(reqStr);

        //    // Get a token and wait in line to submit request
        //    if (RequestToken(out Task token))
        //        await token;

        //    // Send request
        //    var response = await httpClient.GetAsync(reqStr);

        //    // Check success
        //    if (response.IsSuccessStatusCode)
        //    {
        //        // Read response from the server
        //        var replyString = await response.Content.ReadAsStringAsync();

        //        // Convert the string response to JSON model
        //        var replyObject = JsonConvert.DeserializeObject<RestAggregatesBars_Response>(replyString);

        //        if (replyObject.ResultsCount > 0)
        //        {
        //            ret.AddRange(replyObject.Results);
        //        }
        //    }
        //    else
        //    {
        //        Console.WriteLine($"Error in request: {reqStr}");
        //        reqStr = "done";
        //    }

        //    return ret;
        //}

        //// Request Option Chain Snapshot
        //public async Task<List<RestOptionsChain_Result>> OptionsChainAsync(Stock stock)
        //{
        //    // Form request string
        //    var reqStr = $@"/v3/snapshot/options/{stock.Symbol}?limit=250&sort=expiration_date";

        //    var ret = new List<RestOptionsChain_Result>();

        //    // While additional requests remain
        //    while (reqStr != "done")
        //    {
        //        // Wait for request token
        //        if (RequestToken(out Task token))
        //            await token;

        //        var response = await httpClient.GetAsync(reqStr);

        //        if (response.IsSuccessStatusCode)
        //        {
        //            var replyString = await response.Content.ReadAsStringAsync();
        //            var replyObject = JsonConvert.DeserializeObject<RestOptionsChain_Response>(replyString);

        //            ret.AddRange(replyObject.results);

        //            if (replyObject.next_url != null)
        //                reqStr = replyObject.next_url;
        //            else
        //                reqStr = "done";
        //        }
        //        else
        //        {
        //            Console.WriteLine($"Error in request: {reqStr}");
        //            reqStr = "done";
        //        }
        //    }
        //    Console.WriteLine($"Option chain request for {stock.Symbol} complete");

        //    return ret;
        //}

        //public async Task<List<RestOptionsChainExpired_Result>> ExpiredOptionsChainAsync(Stock stock, DateTime date)
        //{
        //    //
        //    // This particular call will only return up to 1000 records and will not paginate, so it is constructed to only request one day (calling method responsible for iterating)
        //    //

        //    // Form request string
        //    var reqStr = $@"/v3/reference/options/contracts?underlying_ticker={stock.Symbol}&expiration_date={date:yyyy-MM-dd}&limit=1000&expired=true&sort=expiration_date";

        //    var ret = new List<RestOptionsChainExpired_Result>();


        //    // While additional requests remain
        //    while (reqStr != "done")
        //    {
        //        // Wait for request token
        //        if (RequestToken(out Task token))
        //            await token;

        //        var response = await httpClient.GetAsync(reqStr);

        //        if (response.IsSuccessStatusCode)
        //        {
        //            var replyString = await response.Content.ReadAsStringAsync();
        //            var replyObject = JsonConvert.DeserializeObject<RestOptionsChainExpired_Response>(replyString);

        //            ret.AddRange(replyObject.results);

        //            reqStr = "done";
        //        }
        //        else
        //        {
        //            Console.WriteLine($"Error in request: {reqStr}");
        //            reqStr = "done";
        //        }
        //    }

        //    Console.WriteLine($"Expired option chain request for {stock.Symbol} complete");

        //    return ret;
        //}

        //// Request All Supported Ticker Symbols (Common Stock and ETFs)
        //public async Task<List<string>> AllTickersAsync()
        //{
        //    // Form request string
        //    var reqStr = $@"https://api.polygon.io/v3/reference/tickers?limit=1000&type=CS&market=stocks&active=true";

        //    // Return object
        //    var ret = new List<string>();

        //    // While there are additional request URLs
        //    while (reqStr != "done")
        //    {
        //        // Wait to send our request
        //        if (RequestToken(out Task token))
        //            await token;

        //        // Wait on server response
        //        var response = await httpClient.GetAsync(reqStr);

        //        // If successful response
        //        if (response.IsSuccessStatusCode)
        //        {
        //            // Read response string from server
        //            var replyString = await response.Content.ReadAsStringAsync();

        //            // Convert response local JSON objects
        //            var replyObject = JsonConvert.DeserializeObject<RestTickers_Response>(replyString);

        //            // Add ticker symbols to return object
        //            ret.AddRange(replyObject.results.Select(x => x.ticker).ToArray());

        //            // If there is another request URL
        //            if (replyObject.next_url != null)
        //                reqStr = replyObject.next_url;
        //            else
        //                reqStr = "done";
        //        }
        //        else
        //        {
        //            Console.WriteLine($"Error in request: {reqStr}");
        //            reqStr = "done";
        //        }
        //    }

        //    //
        //    // ETFs
        //    //

        //    // Form request string
        //    reqStr = $@"https://api.polygon.io/v3/reference/tickers?limit=1000&type=ETF&market=stocks&active=true";

        //    // While there are additional request URLs
        //    while (reqStr != "done")
        //    {
        //        // Wait to send our request
        //        if (RequestToken(out Task token))
        //            await token;

        //        // Wait on server response
        //        var response = await httpClient.GetAsync(reqStr);

        //        // If successful response
        //        if (response.IsSuccessStatusCode)
        //        {
        //            // Read response string from server
        //            var replyString = await response.Content.ReadAsStringAsync();

        //            // Convert response local JSON objects
        //            var replyObject = JsonConvert.DeserializeObject<RestTickers_Response>(replyString);

        //            // Add ticker symbols to return object
        //            ret.AddRange(replyObject.results.Select(x => x.ticker).ToArray());

        //            // If there is another request URL
        //            if (replyObject.next_url != null)
        //                reqStr = replyObject.next_url;
        //            else
        //                reqStr = "done";
        //        }
        //        else
        //        {
        //            Console.WriteLine($"Error in request: {reqStr}");
        //            reqStr = "done";
        //        }
        //    }

        //    return ret;
        //}

        #endregion
    }
}
