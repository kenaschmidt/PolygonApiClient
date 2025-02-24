﻿using PolygonApiClient.ExtendedClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Permissions;
using System.Security.Policy;
using System.Threading.Tasks;

namespace PolygonApiClient.ExtendedClient
{
    public interface IHasOptions
    {
        string Symbol { get; }

        OptionChain OptionChain { get; }

        void AddOptions(List<IPolygonOptionData> options);

        void AddOptions(IPolygonOptionData[] options);

        void AddOptions(RestOptionsContract_Result[] options);

        Task LoadOptions();
        
        Task LoadOptions(DateTime since, bool expiredOnly = true);

        Task LoadOptionExpiry(DateTime expiry);

    }


    public abstract partial class Security : IEquatable<Security>
    {
        public ISecurityDataProvider dataProvider { get; }

        public string Symbol { get; }

        protected Security(string symbol, ISecurityDataProvider dataClient)
        {
            Symbol = symbol;
            dataProvider = dataClient;
        }

        public List<Trade> Trades { get; protected set; } = new List<Trade>();
        public List<Quote> Quotes { get; protected set; } = new List<Quote>();

        public Trade LastTrade { get; set; }
        public Quote LastQuote { get; set; }


        private void AddQuotes(List<Quote> quotes)
        {
            Quotes.AddRange(quotes);
            if (quotes.Count > 0)
                OnQuoteReceived(quotes.Last());
        }
        private void AddTrades(List<Trade> trades)
        {
            Trades.AddRange(trades);
        }

        public void AddQuotesAndTrades(List<Quote> quotes, List<Trade> trades)
        {
            //
            // Exclude Trades with appropriate codes here            
            //

            trades.RemoveAll(x => x.IsNotUpdateVolumeTrade());

            if (LastQuote != null && quotes.Count == 0)
                quotes.Add(LastQuote);

            trades.SetTradeSides(quotes);

            this.AddQuotes(quotes);
            this.AddTrades(trades);

            OnTradeSnapshotReceived(trades);
        }

        public async Task<List<Bar>> GetBars(int timespanMultiplier, PolygonTimespan timespan, DateTime from, DateTime to)
        {
            return await dataProvider.Get_Bars_Async(this, timespan, timespanMultiplier, from, to);
        }
        public async Task<List<Bar>> GetBars(int timespanMultiplier, PolygonTimespan timespan, DateTime day)
        {
            return await dataProvider.Get_Bars_Async(this, timespan, timespanMultiplier, day);
        }

        #region Data Request Methods / REST Async

        public async Task<Quote> LatestQuoteAsync()
        {
            if (QuotesStreaming && LastQuote != null)
                return LastQuote;

            return await dataProvider.Quote_Async(this);
        }
        public virtual async Task<Quote> LatestQuoteAsync(DateTime asOf)
        {
            return await dataProvider.Quote_Async(this, asOf);
        }
        public async Task<Trade> LatestTradeAsync()
        {
            if (TradesStreaming && LastTrade != null)
                return LastTrade;

            return await dataProvider.Trade_Async(this);
        }
        public async Task<Trade> LatestTradeAsync(DateTime asOf)
        {
            return await dataProvider.Trade_Async(this, asOf);
        }
        public async Task<Bar> LatestDailyOHLCAsync()
        {
            return await dataProvider.Previous_Close_Async(this);
        }

        public async Task LoadQuotesTradesAsync(DateTime day)
        {
            await dataProvider.Load_Quotes_And_Trades_Async(this, day);
        }
        public async Task LoadQuotesTradesAsync(DateTime from, DateTime to)
        {
            await dataProvider.Load_Quotes_And_Trades_Async(this, from, to);
        }

        //
        // Last Calculation Values are used to return a good value for pricing purposes on any type of security.
        //
        public abstract Task<double> LastCalculationValueAsync(DateTime? asOf = null);

        #endregion

        #region IEquatable

        public override bool Equals(object obj)
        {
            return Equals(obj as Security);
        }
        public bool Equals(Security other)
        {
            return !(other is null) &&
                   Symbol == other.Symbol;
        }
        public override int GetHashCode()
        {
            return -1758840423 + EqualityComparer<string>.Default.GetHashCode(Symbol);
        }
        public static bool operator ==(Security left, Security right)
        {
            return EqualityComparer<Security>.Default.Equals(left, right);
        }
        public static bool operator !=(Security left, Security right)
        {
            return !(left == right);
        }

        #endregion

    }

    /// <summary>
    /// SOCKET/Realtime Implementation
    /// </summary>
    public abstract partial class Security
    {

        public event EventHandler<Trade> TradeReceived;
        private void OnTradeReceived(Trade trade)
        {
            TradeReceived?.Invoke(this, trade);
        }

        public event EventHandler<Quote> QuoteReceived;
        private void OnQuoteReceived(Quote quote)
        {
            QuoteReceived?.Invoke(this, quote);
        }

        public event EventHandler<Bar> SecondBarReceived;
        private void OnSecondBarReceived(Bar bar)
        {
            SecondBarReceived?.Invoke(this, bar);
        }

        public event EventHandler<Bar> MinuteBarReceived;
        private void OnMinuteBarReceived(Bar bar)
        {
            MinuteBarReceived?.Invoke(this, bar);
        }

        public event EventHandler<List<Trade>> TradeSnapshotReceived;
        private void OnTradeSnapshotReceived(List<Trade> tradeSnapshots)
        {
            TradeSnapshotReceived?.Invoke(this, tradeSnapshots);
        }


        protected PolygonSocketHandler SocketHandler { get; set; }
        public void AttachSocketHandler(PolygonSocketHandler socketHandler)
        {
            if (SocketHandler != null)
                return;

            SocketHandler = socketHandler;

            SocketHandler.QuoteReceived += HandleSocketQuote;
            SocketHandler.TradeReceived += HandleSocketTrade;
            SocketHandler.SecondAggregateReceived += HandleSocketAggregateSecond;
            SocketHandler.MinuteAggregateReceived += HandleSocketAggregateMinute;
        }

        public bool QuotesStreaming => SocketHandler?.QuotesStreaming ?? false;
        public bool TradesStreaming => SocketHandler?.TradesStreaming ?? false;
        public bool SecondsStreaming => SocketHandler?.SecondsStreaming ?? false;
        public bool MinutesStreaming => SocketHandler?.MinutesStreaming ?? false;
        public bool SnapshotQuotesTradesStreaming => snapshotHandler_quotesTrades != null;

        protected void HandleSocketQuote(object sender, Socket_Quote e)
        {
            LastQuote = Quotes.AddAndReturn(new Quote(e));
            OnQuoteReceived(LastQuote);
        }
        protected void HandleSocketTrade(object sender, Socket_Trade e)
        {
            Trade newTrade = new Trade(e);

            if (LastQuote != null)
                newTrade.SetTradeSide(LastQuote);

            LastTrade = Trades.AddAndReturn(newTrade);

            OnTradeReceived(LastTrade);
        }
        protected void HandleSocketAggregateSecond(object sender, Socket_Aggregate e)
        {
            Bar bar = new Bar(e, PolygonTimespan.second, 1);
            OnSecondBarReceived(bar);
        }
        protected void HandleSocketAggregateMinute(object sender, Socket_Aggregate e)
        {
            Bar bar = new Bar(e, PolygonTimespan.minute, 1);
            OnMinuteBarReceived(bar);
        }

        public async Task StreamQuotes()
        {
            if (QuotesStreaming)
                return;

            Console.WriteLine($"START QUOTES FOR {this.Symbol}");

            AttachSocketHandler(await dataProvider.Stream_Quotes(this, true));
        }
        public async Task StopQuotes()
        {
            if (!QuotesStreaming)
                return;

            AttachSocketHandler(await dataProvider.Stream_Quotes(this, false));
        }
        public async Task StreamTrades()
        {
            if (TradesStreaming)
                return;

            AttachSocketHandler(await dataProvider.Stream_Trades(this, true));
        }
        public async Task StopTrades()
        {
            if (!TradesStreaming)
                return;

            AttachSocketHandler(await dataProvider.Stream_Trades(this, false));
        }
        public async Task StreamSecondBars()
        {
            if (SecondsStreaming)
                return;

            AttachSocketHandler(await dataProvider.Stream_Second_Bars(this, true));
        }
        public async Task StopSecondBars()
        {
            if (!SecondsStreaming)
                return;

            AttachSocketHandler(await dataProvider.Stream_Second_Bars(this, false));
        }
        public async Task StreamMinuteBars()
        {
            if (MinutesStreaming)
                return;

            AttachSocketHandler(await dataProvider.Stream_Minute_Bars(this, true));
        }
        public async Task StopMinuteBars()
        {
            if (!MinutesStreaming)
                return;

            AttachSocketHandler(await dataProvider.Stream_Minute_Bars(this, false));
        }


        private RestSnapshotHandler snapshotHandler_quotesTrades { get; set; }
        public void StreamQuotesTradesSnapshots(int secondsInterval = 1)
        {
            // This socket handler only exists to allow starting/stopping the stream - the actual data is passed directly to the AddQuotesAndTrades() method

            snapshotHandler_quotesTrades = dataProvider.Stream_Quotes_Trades_Snapshots(this, secondsInterval);
        }
        public void StopQuotesTradesSnapshots()
        {
            snapshotHandler_quotesTrades.Stop();
            snapshotHandler_quotesTrades = null;
        }

    }

}
