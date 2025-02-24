﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Collections.Concurrent;
using PolygonApiClient.WebSocketsClient;
using System.Security.Permissions;
using static PolygonApiClient.PolygonHelpers;
using System.Reflection;
using System.Diagnostics;
using PolygonApiClient.Helpers;
using System.Timers;
using System.CodeDom;
using System.Text.RegularExpressions;
using System.Text.Json.Serialization;
using System.Net;

namespace PolygonApiClient.RESTClient
{
    public class PolygonRestClient
    {
        #region Events

        public event EventHandler<string> MessageReceived;
        private void OnMessageReceived(string message)
        {
            MessageReceived?.Invoke(this, message);
        }

        #endregion

        private HttpClient httpClient { get; set; }

        private readonly string API_Key;

        #region Constructor and Initialization

        internal PolygonRestClient(string apiKey)
        {
            this.API_Key = apiKey;

            _initHttpClient();
            _initRequestQueue();
        }

        private void _initHttpClient()
        {
            httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(@"https://api.polygon.io");
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {API_Key}");
            httpClient.Timeout = Timeout.InfiniteTimeSpan;

            System.Net.ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
        }
        private void _initRequestQueue()
        {
            requestQueue = new ConcurrentQueue<Task>();
            requestTimer = new System.Timers.Timer()
            {
                Interval = (1000 / MaxRequestsPerSec)
            };
            requestTimer.Elapsed += (s, e) =>
            {
                if (requestQueue.TryDequeue(out Task request))
                {
                    request.RunSynchronously();
                }
            };
            requestTimer.Start();
        }

        #endregion

        #region Request Metering/Queuing

        // 
        // REST requests are metered by awaiting an empty Task<> (Token) which is placed in a queue and processed by a background timer.
        //

        private ConcurrentQueue<Task> requestQueue { get; set; }
        private System.Timers.Timer requestTimer { get; set; }
        private int MaxRequestsPerSec { get; set; } = 100;

        private bool requestToken(out Task token)
        {
            token = new Task(() => { });
            requestQueue.Enqueue(token);
            return true;
        }

        #endregion

        #region Request Processing

        public class CustomInt64Converter : JsonConverter<long>
        {
            public override long Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                return reader.TryGetInt64(out var result) ? result : Convert.ToInt64(reader.GetDecimal());
            }

            public override void Write(Utf8JsonWriter writer, long value, JsonSerializerOptions options)
            {
                writer.WriteNumberValue(value);
            }
        }
        JsonSerializerOptions jsonOptions = new JsonSerializerOptions
        {
            Converters = { new CustomInt64Converter() }
        };

        /// <summary>
        /// Sends a request string and processes the return data into object[s] of the type TResult. Processes all subsequent (paginated) data before returning.
        /// </summary>
        /// <typeparam name="TResponse">The Response object</typeparam>
        /// <typeparam name="TResult">The Result object</typeparam>
        /// <param name="reqString">Fully-formed request string</param>
        /// <returns></returns>
        private async Task<TResult> processRestRequestSingleResultAsync<TResponse, TResult>(string reqString) where TResponse : Rest_Response<TResult>
        {
            try
            {

                if (requestToken(out Task token))
                    await token;

                // Send request
                HttpResponseMessage response = await httpClient.GetAsync(reqString);

                // Check success
                if (response.IsSuccessStatusCode)
                {
                    // Read response from the server
                    string replyString = await response.Content.ReadAsStringAsync();


                    // Convert the string response to appropriate JSON model (TResponse)
                    TResponse responseObject = JsonSerializer.Deserialize<TResponse>(replyString, jsonOptions);

                    // If there is a follow-on request, throw an error
                    if (responseObject.Next_URL != null)
                        throw new Exception($"{MethodBase.GetCurrentMethod()} Received Next_URL when expecting a single result");

                    // Add 'Results' to a return list
                    return responseObject.Results;
                }
                else
                {
                    throw new ArgumentException($"{response.ReasonPhrase}: {reqString}");
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Sends a request string and processes the return data into object[s] of the type TResult. Processes all subsequent (paginated) data before returning.
        /// </summary>
        /// <typeparam name="TResponse">The Response object</typeparam>
        /// <typeparam name="TResult">The Result object</typeparam>
        /// <param name="reqString">Fully-formed request string</param>
        /// <returns></returns>
        private async Task<TResult[]> processRestRequestArrayResultAsync<TResponse, TResult>(string reqString, bool singlePageOnly = false) where TResponse : Rest_Response<TResult[]>
        {
            try
            {
                // List of tasks to be executed for each returned result (so we can keep processing incoming replies)
                List<Task<TResponse>> tasks = new List<Task<TResponse>>();

                // Return data
                List<TResult> ret = new List<TResult>();

                // While the response contains follow-on replies
                while (reqString != "done")
                {
                    // Wait for our turn to transmit
                    if (requestToken(out Task token))
                        await token;

                    // Send request
                    HttpResponseMessage response = await httpClient.GetAsync(reqString);

                    // Check success
                    if (response.IsSuccessStatusCode)
                    {
                        // Read response from the server                        
                        string replyString = await response.Content.ReadAsStringAsync();

                        // Create a task to perform the deserialization in the background
                        tasks.Add(Task<TResponse>.Run(() =>
                        {
                            TResponse r = JsonSerializer.Deserialize<TResponse>(replyString, jsonOptions);
                            return r;
                        }));

                        // Parse the next URL manually so we can continue without waiting for deserialization
                        reqString = singlePageOnly ? "done" : parseNextURL(replyString);
                    }
                    else
                    {
                        throw new ArgumentException($"Error in request: {reqString}");
                    }
                }

                // Wait for all the deserialization tasks to complete
                await Task.WhenAll(tasks.ToArray());

                // Add all the deserialized results (that contained values) to the return list
                foreach (var task in tasks)
                {
                    if (task.Result.Results != null)
                        ret.AddRange(task.Result.Results);
                }

                // Return
                return ret.ToArray();
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Sends a request string and processes the return data into object[s] of the type TResult. Processes all subsequent (paginated) data before returning.
        /// </summary>
        /// <typeparam name="TResponse">The Response object</typeparam>
        /// <typeparam name="TResult">The Result object</typeparam>
        /// <param name="reqString">Fully-formed request string</param>
        /// <returns></returns>
        private async Task<TResult[]> processRestRequest_Special2_ResultAsync<TResult>(string reqString)
        {
            try
            {
                if (requestToken(out Task token))
                    await token;

                // Send request
                HttpResponseMessage response = await httpClient.GetAsync(reqString);

                // Check success
                if (response.IsSuccessStatusCode)
                {
                    // Read response from the server
                    string replyString = await response.Content.ReadAsStringAsync();

                    // Convert the string response to appropriate JSON model (TResponse)
                    TResult[] resultObject = JsonSerializer.Deserialize<TResult[]>(replyString, jsonOptions);

                    return resultObject;
                }
                else
                {
                    throw new ArgumentException($"Error in request: {reqString}");
                }

            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Handles several special cases where Results are not returned as an array, but multiple pages may be returned and the results must be combined before being returned as an array
        /// </summary>
        /// <typeparam name="TResponse">The Response object</typeparam>
        /// <typeparam name="TResult">The Result object</typeparam>
        /// <param name="reqString">Fully-formed request string</param>
        /// <returns></returns>
        private async Task<TResult[]> processRestRequest_Special1_ResultAsync<TResponse, TResult>(string reqString) where TResponse : Rest_Response<TResult>, IResultSpecial
        {
            try
            {
                List<TResult> ret = new List<TResult>();

                while (reqString != "done")
                {
                    if (requestToken(out Task token))
                        await token;

                    // Send request
                    HttpResponseMessage response = await httpClient.GetAsync(reqString);

                    // Check success
                    if (response.IsSuccessStatusCode)
                    {
                        // Read response from the server
                        string replyString = await response.Content.ReadAsStringAsync();

                        // Convert the string response to appropriate JSON model (TResponse)
                        TResponse responseObject = JsonSerializer.Deserialize<TResponse>(replyString, jsonOptions);

                        // Add 'Results' to a return list
                        ret.Add(responseObject.Results);

                        // If there is a follow-on request, get the URL
                        if (responseObject.Next_URL != null)
                            reqString = responseObject.Next_URL;
                        else
                            reqString = "done";
                    }
                    else
                    {
                        throw new ArgumentException($"Error in request: {reqString}");
                    }
                }

                return ret.ToArray();
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Sends a request string and processes the return data into object of the type TResponse. This is used for a few v1 requests
        /// </summary>
        /// <typeparam name="T">The TResponse object expected for return data</typeparam>
        /// <param name="reqString">Fully-formed request string</param>
        /// <returns></returns>
        private async Task<TResponse> processRestRequestAsync<TResponse>(string reqString)
        {

            if (requestToken(out Task token))
                await token;

            // Send request
            HttpResponseMessage response = await httpClient.GetAsync(reqString);

            // Check success
            if (response.IsSuccessStatusCode)
            {
                // Read response from the server
                string replyString = await response.Content.ReadAsStringAsync();

                // Convert the string response to appropriate JSON model (TResponse)
                TResponse responseObject = JsonSerializer.Deserialize<TResponse>(replyString, jsonOptions);

                return responseObject;
            }
            else
            {
                throw new ArgumentException($"Error in request: {reqString}");
            }
        }

        /// <summary>
        /// This method regex-parses the raw response string to pull out the Next_URL value for paginated responses. 
        /// This speeds up processing time since we don't have to wait for the entire value to be deserialied before sending the next request.
        /// </summary>
        /// <param name="responseString"></param>
        /// <returns></returns>
        private string parseNextURL(string responseString)
        {
            Regex rx = new Regex("next_url(.*)https://api.polygon.io+(/[\\w- :./?%&=]*)?");

            var match = rx.Match(responseString);

            if (match.Success == true)
            {
                rx = new Regex("https://api.polygon.io+(/[\\w- :./?%&=]*)?");
                return rx.Match(match.Value).Value;
            }
            else return "done";
        }

        #endregion

        #region -------------------- Polygon REST API Market Data Endpoints -----------------------

        internal async Task<RestAggregatesBars_Result[]> Aggregates_Bars_Async(
            string symbol,
            int multiplier,
            PolygonTimespan timespan,
            long fromMs,
            long toMs,
            bool? adjusted,
            PolygonSort? sort,
            int? limit)
        {

            // Build optional parameters string                
            string optionalParameters = OptionalParametersStringBuilder(
                (nameof(adjusted), adjusted),
                (nameof(sort), sort),
                (nameof(limit), limit));

            // Base request string with optional parameters appended
            string reqStr = $@"/v2/aggs/ticker/{symbol}/range/{multiplier}/{timespan}/{fromMs}/{toMs}{optionalParameters}";

            // Submit request and return processed results
            return await processRestRequestArrayResultAsync<RestAggregatesBars_Response, RestAggregatesBars_Result>(reqStr);
        }

        internal async Task<RestGroupedDailyBars_Result[]> Grouped_Daily_Bars_Async(
            string date,
            bool? adjusted,
            bool? include_otc)
        {

            // Build optional parameters string                
            string optionalParameters = OptionalParametersStringBuilder(
                (nameof(adjusted), adjusted),
                (nameof(include_otc), include_otc));

            // Base request string with optional parameters appended
            string reqStr = $@"/v2/aggs/grouped/locale/us/market/stocks/{date}{optionalParameters}";

            // Submit request and return processed results
            return await processRestRequestArrayResultAsync<RestGroupedDailyBars_Response, RestGroupedDailyBars_Result>(reqStr);
        }

        internal async Task<RestDailyOpenClose_Result> Daily_Open_Close_Async(
            string symbol,
            string date,
            bool? adjusted)
        {
            // Build optional parameters string                
            string optionalParameters = OptionalParametersStringBuilder(
                (nameof(adjusted), adjusted));

            // Base request string with optional parameters appended
            string reqStr = $@"/v1/open-close/{symbol}/{date}{optionalParameters}";

            // Submit request and return processed results
            return await processRestRequestAsync<RestDailyOpenClose_Result>(reqStr);
        }

        internal async Task<RestPreviousClose_Result[]> Previous_Close_Async(
            string symbol,
            bool? adjusted)
        {
            // Build optional parameters string                
            string optionalParameters = OptionalParametersStringBuilder(
                (nameof(adjusted), adjusted));

            // Base request string with optional parameters appended
            string reqStr = $@"/v2/aggs/ticker/{symbol}/prev{optionalParameters}";

            // Submit request and return processed results
            return await processRestRequestArrayResultAsync<RestPreviousClose_Response, RestPreviousClose_Result>(reqStr);
        }

        internal async Task<RestTrades_Result[]> Trades_Async(
            string symbol,
            string timestamp,
            PolygonFilterParams? timestampFilter,
            PolygonOrder? order,
            int? limit,
            PolygonTradeQuoteSort? sort)
        {
            // Build optional parameters string                
            string optionalParameters = OptionalParametersStringBuilder(
                (nameof(timestamp), timestamp),
                (nameof(timestampFilter), timestampFilter),
                (nameof(order), order),
                (nameof(limit), limit),
                (nameof(sort), sort));

            // Base request string with optional parameters appended
            string reqStr = $@"/v3/trades/{symbol}{optionalParameters}";

            // Submit request and return processed results
            return await processRestRequestArrayResultAsync<RestTrades_Response, RestTrades_Result>(reqStr);
        }

        internal async Task<RestTrades_Result[]> Trades_Async(
           string symbol,
           long? timestamp,
           PolygonFilterParams? timestampFilter,
           PolygonOrder? order,
           int? limit,
           PolygonTradeQuoteSort? sort)
        {
            // Build optional parameters string                
            string optionalParameters = OptionalParametersStringBuilder(
                (nameof(timestamp), timestamp),
                (nameof(timestampFilter), timestampFilter),
                (nameof(order), order),
                (nameof(limit), limit),
                (nameof(sort), sort));

            // Base request string with optional parameters appended
            string reqStr = $@"/v3/trades/{symbol}{optionalParameters}";

            // Submit request and return processed results
            return await processRestRequestArrayResultAsync<RestTrades_Response, RestTrades_Result>(reqStr);
        }

        /// <summary>
        /// TEST: Special method to only retrieve a window of quotes
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="order"></param>
        /// <param name="limit"></param>
        /// <param name="sort"></param>
        /// <returns></returns>
        internal async Task<RestTrades_Result[]> Trades_Async(
            string symbol,
            long from,
            long to,
            PolygonOrder? order,
            int? limit,
            PolygonTradeQuoteSort? sort)
        {
            // Build optional parameters string                
            string optionalParameters = OptionalParametersStringBuilder(
                (nameof(order), order),
                (nameof(limit), limit),
                (nameof(sort), sort));

            string timestampFilter = $"timestamp.gte={from}&timestamp.lt={to}";

            // Base request string with optional parameters appended
            string reqStr = $@"/v3/trades/{symbol}{optionalParameters}{timestampFilter}";

            // Submit request and return processed results
            return await processRestRequestArrayResultAsync<RestTrades_Response, RestTrades_Result>(reqStr);
        }


        internal async Task<RestLastTrade_Result> Last_Trade_Async(
            string symbol)
        {
            // Base request string
            string reqStr = $@"/v2/last/trade/{symbol}";

            // Submit request and return processed results
            return await processRestRequestSingleResultAsync<RestLastTrade_Response, RestLastTrade_Result>(reqStr);
        }

        internal async Task<RestTrades_Result> Last_Trade_Async(
            string symbol,
            long timestamp)
        {
            // Build optional parameters string                
            string optionalParameters = OptionalParametersStringBuilder(
                (nameof(timestamp), timestamp),
                ("timestampFilter", PolygonFilterParams.lte),
                ("limit", 1));

            // Base request string with optional parameters appended
            string reqStr = $@"/v3/trades/{symbol}{optionalParameters}";

            // Submit request and return processed results
            var ret = await processRestRequestArrayResultAsync<RestTrades_Response, RestTrades_Result>(reqStr, true);

            return ret.SingleOrDefault();
        }

        internal async Task<RestQuotes_Result[]> Quotes_Async(
            string symbol,
            string timestamp,
            PolygonFilterParams? timestampFilter,
            PolygonOrder? order,
            int? limit,
            PolygonTradeQuoteSort? sort)
        {
            // Build optional parameters string                
            string optionalParameters = OptionalParametersStringBuilder(
                (nameof(timestamp), timestamp),
                (nameof(timestampFilter), timestampFilter),
                (nameof(order), order),
                (nameof(limit), limit),
                (nameof(sort), sort));

            // Base request string with optional parameters appended
            string reqStr = $@"/v3/quotes/{symbol}{optionalParameters}";

            // Submit request and return processed results
            var result = await processRestRequestArrayResultAsync<RestQuotes_Response, RestQuotes_Result>(reqStr);

            return result;
        }
        internal async Task<RestQuotes_Result[]> Quotes_Async(
            string symbol,
            long? timestamp,
            PolygonFilterParams? timestampFilter,
            PolygonOrder? order,
            int? limit,
            PolygonTradeQuoteSort? sort)
        {
            // Build optional parameters string                
            string optionalParameters = OptionalParametersStringBuilder(
                (nameof(timestamp), timestamp),
                (nameof(timestampFilter), timestampFilter),
                (nameof(order), order),
                (nameof(limit), limit),
                (nameof(sort), sort));

            // Base request string with optional parameters appended
            string reqStr = $@"/v3/quotes/{symbol}{optionalParameters}";

            // Submit request and return processed results
            var result = await processRestRequestArrayResultAsync<RestQuotes_Response, RestQuotes_Result>(reqStr);

            return result;
        }

        /// <summary>
        /// TEST: Special method to only retrieve a window of quotes
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="order"></param>
        /// <param name="limit"></param>
        /// <param name="sort"></param>
        /// <returns></returns>
        internal async Task<RestQuotes_Result[]> Quotes_Async(
            string symbol,
            long from,
            long to,
            PolygonOrder? order,
            int? limit,
            PolygonTradeQuoteSort? sort)
        {
            // Build optional parameters string                
            string optionalParameters = OptionalParametersStringBuilder(
                (nameof(order), order),
                (nameof(limit), limit),
                (nameof(sort), sort));

            string timestampFilter = $"?timestamp.gte={from}&timestamp.lt={to}&";

            // Base request string with optional parameters appended
            string reqStr = $@"/v3/quotes/{symbol}{timestampFilter}{optionalParameters}";

            // Submit request and return processed results
            return await processRestRequestArrayResultAsync<RestQuotes_Response, RestQuotes_Result>(reqStr);
        }

        internal async Task<RestLastQuote_Result> Last_Quote_Async(
            string symbol)
        {
            // Base request string
            string reqStr = $@"/v2/last/nbbo/{symbol}";

            // DEBUG
            // Console.WriteLine($"{MethodBase.GetCurrentMethod()} {reqStr}");

            // Submit request and return processed results
            return await processRestRequestSingleResultAsync<RestLastQuote_Response, RestLastQuote_Result>(reqStr);
        }


        internal async Task<RestQuotes_Result> Last_Quote_Async(
            string symbol,
            long timestamp)
        {
            // Build optional parameters string                
            string optionalParameters = OptionalParametersStringBuilder(
                (nameof(timestamp), timestamp),
                ("timestampFilter", PolygonFilterParams.lte),
                ("limit", 1));

            // Base request string with optional parameters appended
            string reqStr = $@"/v3/quotes/{symbol}{optionalParameters}";
            Debug.WriteLine($"POLYGON REQUEST:  {reqStr}");
            // Submit request and return processed results
            var ret = await processRestRequestArrayResultAsync<RestQuotes_Response, RestQuotes_Result>(reqStr, true);

            return ret.SingleOrDefault();
        }

        // ---------- Snapshots ----------

        internal async Task<RestTickerSnapshot_Result[]> All_Tickers_Async(
            string tickers,
            bool? include_otc)
        {
            // Build optional parameters string                
            string optionalParameters = OptionalParametersStringBuilder(
                (nameof(tickers), tickers),
                (nameof(include_otc), include_otc));

            // Base request string with optional parameters appended
            string reqStr = $@"/v2/snapshot/locale/us/markets/stocks/tickers{optionalParameters}";

            // Submit request and return processed results
            return await processRestRequestArrayResultAsync<RestTickerSnapshot_Response, RestTickerSnapshot_Result>(reqStr);
        }

        internal async Task<RestTickerSnapshot_Result[]> Gainers_Losers_Async(
            PolygonGainersLosers direction,
            bool? include_otc)
        {
            // Build optional parameters string                
            string optionalParameters = OptionalParametersStringBuilder(
                (nameof(include_otc), include_otc));

            // Base request string with optional parameters appended
            string reqStr = $@"/v2/snapshot/locale/us/markets/stocks/{direction}{optionalParameters}";

            // Submit request and return processed results
            return await processRestRequestArrayResultAsync<RestTickerSnapshot_Response, RestTickerSnapshot_Result>(reqStr);
        }

        //public async Task<RestTickerSnapshot_Response> Ticker_Async(
        //    string stocksTicker)
        //{
        //    // This method calls the same endpoint as All_Tickers but with a single ticker defined. Return data is exactly the same. The appropriate call is made from the top-level
        //    // client and this method is only present for consistency.

        //    throw new NotImplementedException();
        //}

        internal async Task<RestOptionContract_Result> Option_Contract_Async(
            string underlyingAsset,
            string optionContract)
        {
            // Base request string with optional parameters appended
            string reqStr = $@"/v3/snapshot/options/{underlyingAsset}/{optionContract}";

            // Submit request and return processed results
            return await processRestRequestSingleResultAsync<RestOptionContract_Response, RestOptionContract_Result>(reqStr);
        }
        internal async Task<RestOptionsChain_Result[]> Options_Chain_Async(
            string underlyingAsset,
            double? strike_price,
            PolygonFilterParams? strikePriceFilter,
            string expiration_date,
            PolygonFilterParams? expirationDateFilter,
            OptionType? contract_type,
            PolygonOrder? order,
            int? limit,
            PolygonOptionsChainSort? sort)
        {
            // Build optional parameters string                
            string optionalParameters = OptionalParametersStringBuilder(
                (nameof(strike_price), strike_price),
                (nameof(strikePriceFilter), strikePriceFilter),
                (nameof(expiration_date), expiration_date),
                (nameof(expirationDateFilter), expirationDateFilter),
                (nameof(contract_type), contract_type),
                (nameof(order), order),
                (nameof(limit), limit),
                (nameof(sort), sort));

            // Base request string with optional parameters appended
            string reqStr = $@"/v3/snapshot/options/{underlyingAsset}{optionalParameters}";

            // Submit request and return processed results
            return await processRestRequestArrayResultAsync<RestOptionsChain_Response, RestOptionsChain_Result>(reqStr);
        }

        //public async Task<object> Universal_Snapshot_Async()
        //{
        //    // This is a bizarre and complicated call that I am not going to bother with for a while
        //    throw new NotImplementedException();
        //}

        // ---------- Technical Indicators ----------

        internal async Task<RestMovingAverage_Result[]> Simple_Moving_Average_Async(
            string symbol,
            long? timestamp,
            PolygonFilterParams? timestampFilter,
            PolygonTimespan? timespan,
            int? window,
            PolygonAggregateSeriesType? series_type,
            PolygonOrder? order,
            bool? expand_underlying,
            bool? adjusted,
            int? limit)
        {
            // Build optional parameters string                
            string optionalParameters = OptionalParametersStringBuilder(
                (nameof(timestamp), timestamp),
                (nameof(timestampFilter), timestampFilter),
                (nameof(timespan), timespan),
                (nameof(window), window),
                (nameof(series_type), series_type),
                (nameof(order), order),
                (nameof(expand_underlying), expand_underlying),
                (nameof(adjusted), adjusted),
                (nameof(limit), limit));

            // Base request string with optional parameters appended
            string reqStr = $@"/v1/indicators/sma/{symbol}{optionalParameters}";

            // Submit request and return processed results
            return await processRestRequest_Special1_ResultAsync<RestMovingAverage_Response, RestMovingAverage_Result>(reqStr);
        }

        internal async Task<RestMovingAverage_Result[]> Exponential_Moving_Average_Async(
           string symbol,
            long? timestamp,
            PolygonFilterParams? timestampFilter,
            PolygonTimespan? timespan,
            int? window,
            PolygonAggregateSeriesType? series_type,
            PolygonOrder? order,
            bool? expand_underlying,
            bool? adjusted,
            int? limit)
        {
            // Build optional parameters string                
            string optionalParameters = OptionalParametersStringBuilder(
                (nameof(timestamp), timestamp),
                (nameof(timestampFilter), timestampFilter),
                (nameof(timespan), timespan),
                (nameof(window), window),
                (nameof(series_type), series_type),
                (nameof(order), order),
                (nameof(expand_underlying), expand_underlying),
                (nameof(adjusted), adjusted),
                (nameof(limit), limit));

            // Base request string with optional parameters appended
            string reqStr = $@"/v1/indicators/ema/{symbol}{optionalParameters}";

            // Submit request and return processed results
            return await processRestRequest_Special1_ResultAsync<RestMovingAverage_Response, RestMovingAverage_Result>(reqStr);
        }

        internal async Task<RestMACD_Result[]> Moving_Average_Convergence_Divergence_Async(
            string symbol,
            long? timestamp,
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
            // Build optional parameters string                
            string optionalParameters = OptionalParametersStringBuilder(
                (nameof(timestamp), timestamp),
                (nameof(timestampFilter), timestampFilter),
                (nameof(timespan), timespan),
                (nameof(adjusted), adjusted),
                (nameof(short_window), short_window),
                (nameof(long_window), long_window),
                (nameof(signal_window), signal_window),
                (nameof(series_type), series_type),
                (nameof(expand_underlying), expand_underlying),
                (nameof(order), order),
                (nameof(limit), limit));

            // Base request string with optional parameters appended
            string reqStr = $@"/v1/indicators/macd/{symbol}{optionalParameters}";

            // Submit request and return processed results
            return await processRestRequest_Special1_ResultAsync<RestMACD_Response, RestMACD_Result>(reqStr);
        }

        internal async Task<RestRSI_Result[]> Relative_Strength_Index_Async(
            string symbol,
            long? timestamp,
            PolygonFilterParams? timestampFilter,
            PolygonTimespan? timespan,
            int? window,
            PolygonAggregateSeriesType? series_type,
            bool? expand_underlying,
            PolygonOrder? order,
            int? limit)
        {
            // Build optional parameters string                
            string optionalParameters = OptionalParametersStringBuilder(
                (nameof(timestamp), timestamp),
                (nameof(timestampFilter), timestampFilter),
                (nameof(timespan), timespan),
                (nameof(window), window),
                (nameof(series_type), series_type),
                (nameof(expand_underlying), expand_underlying),
                (nameof(order), order),
                (nameof(limit), limit));

            // Base request string with optional parameters appended
            string reqStr = $@"/v1/indicators/rsi/{symbol}{optionalParameters}";

            // Submit request and return processed results
            return await processRestRequest_Special1_ResultAsync<RestRSI_Response, RestRSI_Result>(reqStr);
        }

        #endregion

        #region -------------------- Polygon REST API Reference Data Endpoints --------------------

        internal async Task<RestOptionsContract_Result> Options_Contract_Async(
            string options_ticker,
            string as_of)
        {
            // Build optional parameters string                
            string optionalParameters = OptionalParametersStringBuilder(
                (nameof(as_of), as_of));

            // Base request string with optional parameters appended
            string reqStr = $@"/v3/reference/options/contracts/{options_ticker}{optionalParameters}";

            // Submit request and return processed results
            return await processRestRequestSingleResultAsync<RestOptionsContract_Response, RestOptionsContract_Result>(reqStr);
        }

        internal async Task<RestOptionsContract_Result[]> Options_Contracts_Async(
            string underlying_ticker,
            PolygonFilterParams? underlyingTickerFilter,
            OptionType? contract_type,
            string expiration_date,
            PolygonFilterParams? expirationDateFilter,
            string as_of,
            double? strike_price,
            PolygonFilterParams? strikePriceFilter,
            bool? expired,
            PolygonOrder? order,
            int? limit,
            PolygonOptionsContractsSort? sort
            )
        {
            // Build optional parameters string                
            string optionalParameters = OptionalParametersStringBuilder(
                (nameof(underlying_ticker), underlying_ticker),
                (nameof(underlyingTickerFilter), underlyingTickerFilter),
                (nameof(contract_type), contract_type),
                (nameof(expiration_date), expiration_date),
                (nameof(expirationDateFilter), expirationDateFilter),
                (nameof(as_of), as_of),
                (nameof(strike_price), strike_price),
                (nameof(strikePriceFilter), strikePriceFilter),
                (nameof(expired), expired),
                (nameof(order), order),
                (nameof(limit), limit),
                (nameof(sort), sort));

            // Base request string with optional parameters appended
            string reqStr = $@"/v3/reference/options/contracts{optionalParameters}";

            // Submit request and return processed results
            return await processRestRequestArrayResultAsync<RestOptionsContracts_Response, RestOptionsContract_Result>(reqStr);
        }

        internal async Task<RestTickers_Result[]> Tickers_Async(
            string ticker,
            PolygonFilterParams? tickerFilter,
            PolygonTickerType? type,
            PolygonMarket? market,
            string exchange,
            string CUSIP,
            string CIK,
            string date,
            string search,
            bool? active,
            PolygonOrder? order,
            int? limit,
            PolygonTickerSort? sort
            )
        {
            // Build optional parameters string                
            string optionalParameters = OptionalParametersStringBuilder(
                (nameof(ticker), ticker),
                (nameof(tickerFilter), tickerFilter),
                (nameof(type), type),
                (nameof(market), market),
                (nameof(exchange), exchange),
                (nameof(CUSIP), CUSIP),
                (nameof(CIK), CIK),
                (nameof(date), date),
                (nameof(search), search),
                (nameof(active), active),
                (nameof(order), order),
                (nameof(limit), limit),
                (nameof(sort), sort));

            // Base request string with optional parameters appended
            string reqStr = $@"/v3/reference/tickers{optionalParameters}";

            // Submit request and return processed results
            return await processRestRequestArrayResultAsync<RestTickers_Response, RestTickers_Result>(reqStr);
        }

        internal async Task<RestIndicesSnapshot_Result[]> Indices_Snapshot_Async(
            string ticker,
            PolygonFilterParams? tickerFilter,
            PolygonOrder? order,
            int? limit,
            PolygonTickerSort? sort)
        {
            // Build optional parameters string                
            string optionalParameters = OptionalParametersStringBuilder(
                (nameof(ticker), ticker),
                (nameof(tickerFilter), tickerFilter),
                (nameof(order), order),
                (nameof(limit), limit),
                (nameof(sort), sort));

            // Base request string with optional parameters appended
            string reqStr = $@"/v3/snapshot/indices{optionalParameters}";

            // Submit request and return processed results
            return await processRestRequestArrayResultAsync<RestIndiciesSnapshot_Response, RestIndicesSnapshot_Result>(reqStr);
        }

        internal async Task<RestTickerDetail_Result> Ticker_Details_Async(
            string ticker,
            string date
            )
        {
            // Build optional parameters string                
            string optionalParameters = OptionalParametersStringBuilder(
                (nameof(date), date));

            // Base request string with optional parameters appended
            string reqStr = $@"/v3/reference/tickers/{ticker}{optionalParameters}";

            // Submit request and return processed results
            return await processRestRequestSingleResultAsync<RestTickerDetail_Response, RestTickerDetail_Result>(reqStr);
        }

        /// <summary>
        /// Get a timeline of events for the entity associated with the given ticker, CUSIP, or Composite FIGI.
        /// </summary>
        /// <param name="id">Identifier of an asset. This can currently be a Ticker, CUSIP, or Composite FIGI.</param>
        /// <param name="types">A comma-separated list of the types of event to include. Currently ticker_change is the only supported event_type. Leave blank to return all supported event_types.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        internal async Task<RestTickerEvents_Result> Ticker_Events_Async(
            string id,
            string types)
        {
            // Build optional parameters string                
            string optionalParameters = OptionalParametersStringBuilder(
                (nameof(types), types));

            // Base request string with optional parameters appended
            string reqStr = $@"/vX/reference/tickers/{id}/events{optionalParameters}";

            // Submit request and return processed results
            return await processRestRequestSingleResultAsync<RestTickerEvents_Response, RestTickerEvents_Result>(reqStr);
        }

        internal async Task<RestTickerNews_Result[]> Ticker_News_Async(
            string ticker,
            PolygonFilterParams? tickerFilter,
            string published_utc,
            PolygonFilterParams? publishedUtcFilter,
            PolygonOrder? order,
            int? limit,
            PolygonNewsSort? sort)
        {
            // Build optional parameters string                
            string optionalParameters = OptionalParametersStringBuilder(
                (nameof(ticker), ticker),
                (nameof(tickerFilter), tickerFilter),
                (nameof(published_utc), published_utc),
                (nameof(publishedUtcFilter), publishedUtcFilter),
                (nameof(order), order),
                (nameof(limit), limit),
                (nameof(sort), sort));

            // Base request string with optional parameters appended
            string reqStr = $@"/v2/reference/news{optionalParameters}";

            // Submit request and return processed results
            return await processRestRequestArrayResultAsync<RestTickerNews_Response, RestTickerNews_Result>(reqStr);
        }

        internal async Task<RestTickerTypes_Result[]> Ticker_Types_Async(
            PolygonMarket? asset_class,
            PolygonLocale? locale)
        {
            // Build optional parameters string                
            string optionalParameters = OptionalParametersStringBuilder(
                (nameof(asset_class), asset_class),
                (nameof(locale), locale));

            // Base request string with optional parameters appended
            string reqStr = $@"/v3/reference/tickers/types{optionalParameters}";

            // Submit request and return processed results
            return await processRestRequestArrayResultAsync<RestTickerTypes_Response, RestTickerTypes_Result>(reqStr);
        }

        internal async Task<RestMarketHolidays_Result[]> Market_Holidays_Async()
        {
            // Base request string with optional parameters appended
            string reqStr = $@"/v1/marketstatus/upcoming";

            // DEBUG
            // Console.WriteLine($"{MethodBase.GetCurrentMethod()} {reqStr}");

            // Submit request and return processed results
            return await processRestRequest_Special2_ResultAsync<RestMarketHolidays_Result>(reqStr);
        }

        internal async Task<RestMarketStatus_Response> Market_Status_Async()
        {
            // Base request string with optional parameters appended
            string reqStr = $@"/v1/marketstatus/now";

            // Submit request and return processed results
            return await processRestRequestAsync<RestMarketStatus_Response>(reqStr);
        }

        internal async Task<RestStockSplits_Result[]> Stock_Splits_Async(
            string ticker,
            PolygonFilterParams? tickerFilter,
            string execution_date,
            PolygonFilterParams? executionDateFilter,
            bool? reverse_split,
            PolygonOrder? order,
            int? limit,
            PolygonStockSplitSort? sort
            )
        {
            // Build optional parameters string                
            string optionalParameters = OptionalParametersStringBuilder(
                (nameof(ticker), ticker),
                (nameof(tickerFilter), tickerFilter),
                (nameof(execution_date), execution_date),
                (nameof(executionDateFilter), executionDateFilter),
                (nameof(reverse_split), reverse_split),
                (nameof(order), order),
                (nameof(limit), limit),
                (nameof(sort), sort));

            // Base request string with optional parameters appended
            string reqStr = $@"/v3/reference/splits{optionalParameters}";

            // Submit request and return processed results
            return await processRestRequestArrayResultAsync<RestStockSplits_Response, RestStockSplits_Result>(reqStr);
        }

        internal async Task<RestDividends_Result[]> Dividends_Async(
            string ticker,
            PolygonFilterParams? tickerFilter,
            string ex_dividend_date,
            PolygonFilterParams? exDateFilter,
            string record_date,
            PolygonFilterParams? recordDateFilter,
            string declaration_date,
            PolygonFilterParams? declarationDateFilter,
            string pay_date,
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
            // Build optional parameters string                
            string optionalParameters = OptionalParametersStringBuilder(
                (nameof(ticker), ticker),
                (nameof(tickerFilter), tickerFilter),
                (nameof(ex_dividend_date), ex_dividend_date),
                (nameof(exDateFilter), exDateFilter),
                (nameof(record_date), record_date),
                (nameof(recordDateFilter), recordDateFilter),
                (nameof(declaration_date), declaration_date),
                (nameof(declarationDateFilter), declarationDateFilter),
                (nameof(pay_date), pay_date),
                (nameof(payDateFilter), payDateFilter),
                (nameof(frequency), frequency),
                (nameof(cash_amount), cash_amount),
                (nameof(cashAmountFilter), cashAmountFilter),
                (nameof(dividend_type), dividend_type),
                (nameof(order), order),
                (nameof(limit), limit),
                (nameof(sort), sort));

            // Base request string with optional parameters appended
            string reqStr = $@"/v3/reference/dividends{optionalParameters}";

            // Submit request and return processed results
            return await processRestRequestArrayResultAsync<RestDividends_Response, RestDividends_Result>(reqStr);
        }

        internal async Task<RestStockFinancials_Result[]> Stock_Financials_Async(
            string ticker,
            string cik,
            string company_name,
            PolygonCompanyNameFilter? companyNameFilter,
            string sic,
            string filing_date,
            PolygonFilterParams? filingDateFilter,
            PolygonFinancialsTimeframe? timeframe,
            bool? include_sources,
            PolygonOrder? order,
            int? limit,
            PolygonFinancialsSort? sort
            )
        {
            // Build optional parameters string                
            string optionalParameters = OptionalParametersStringBuilder(
                (nameof(ticker), ticker),
                (nameof(cik), cik),
                (nameof(company_name), company_name),
                (nameof(companyNameFilter), companyNameFilter),
                (nameof(sic), sic),
                (nameof(filing_date), filing_date),
                (nameof(filingDateFilter), filingDateFilter),
                (nameof(timeframe), timeframe),
                (nameof(include_sources), include_sources),
                (nameof(order), order),
                (nameof(limit), limit),
                (nameof(sort), sort));

            // Base request string with optional parameters appended
            string reqStr = $@"/vX/reference/financials{optionalParameters}";

            // Submit request and return processed results
            return await processRestRequestArrayResultAsync<RestStockFinancials_Response, RestStockFinancials_Result>(reqStr);
        }

        internal async Task<RestConditions_Result[]> Conditions_Async(
            PolygonMarket? asset_class,
            PolygonConditionCodeDataType? data_type,
            int? id,
            int? sip,
            PolygonOrder? order,
            int? limit,
            PolygonConditionsSort? sort
            )
        {
            // Build optional parameters string                
            string optionalParameters = OptionalParametersStringBuilder(
                (nameof(asset_class), asset_class),
                (nameof(data_type), data_type),
                (nameof(id), id),
                (nameof(sip), sip),
                (nameof(order), order),
                (nameof(limit), limit),
                (nameof(sort), sort));

            // Base request string with optional parameters appended
            string reqStr = $@"/v3/reference/conditions{optionalParameters}";

            // DEBUG
            // Console.WriteLine($"{MethodBase.GetCurrentMethod()} {reqStr}");

            // Submit request and return processed results
            return await processRestRequestArrayResultAsync<RestConditions_Response, RestConditions_Result>(reqStr);
        }

        internal async Task<RestExchange_Result[]> Exchanges_Async(
            PolygonMarket? asset_class,
            PolygonLocale? locale)
        {
            // Build optional parameters string                
            string optionalParameters = OptionalParametersStringBuilder(
                (nameof(asset_class), asset_class),
                (nameof(locale), locale));

            // Base request string with optional parameters appended
            string reqStr = $@"/v3/reference/exchanges{optionalParameters}";

            // Submit request and return processed results
            return await processRestRequestArrayResultAsync<RestExchange_Response, RestExchange_Result>(reqStr);
        }

        #endregion   

    }
}
