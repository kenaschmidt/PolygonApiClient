using PolygonApiClient.ExtendedClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Permissions;
using System.Security.Policy;
using System.Threading.Tasks;

namespace PolygonApiClient.ExtendedClient
{
    public abstract partial class Security : IEquatable<Security>
    {
        public ISecurityDataProvider dataProvider { get; }

        public string Symbol { get; }

        protected Security(string symbol, ISecurityDataProvider dataClient)
        {
            Symbol = symbol;
            dataProvider = dataClient;
        }

        protected List<Trade> Trades { get; } = new List<Trade>();
        protected List<Quote> Quotes { get; } = new List<Quote>();
        protected BarList Bars { get; } = new BarList();

        protected Trade LastTrade { get; set; }
        protected Quote LastQuote { get; set; }


        private void AddQuotes(List<Quote> quotes)
        {
            Quotes.AddRange(quotes);
        }
        private void AddTrades(List<Trade> trades)
        {
            Trades.AddRange(trades);
        }

        public void AddQuotesAndTrades(List<Quote> quotes, List<Trade> trades)
        {
            // This is here to try and fix situations where we are adding a list with trades that show up before any quotes
            if (LastQuote != null)
                quotes.Add(LastQuote);

            trades.SetTradeSides(quotes);

            this.AddQuotes(quotes);
            this.AddTrades(trades);

            OnTradeSnapshotReceived(trades);
        }
        public void AddBars(List<Bar> bars)
        {
            Bars.AddBars(bars);
        }


        public List<Bar> GetBars(int timespanMultiplier, PolygonTimespan timespan, DateTime from, DateTime to)
        {
            return Bars.GetBars(timespanMultiplier, timespan, from, to);
        }
        public List<Bar> GetBars(int timespanMultiplier, PolygonTimespan timespan, DateTime day)
        {
            return Bars.GetBars(timespanMultiplier, timespan, day.Date, day.Date.AddDays(1).AddTicks(-1));
        }

        #region Pricing Data

        //
        // Realtime (latest) pricing
        //
        public async Task<Quote> LastQuoteAsync()
        {
            if (QuotesStreaming)
                return LastQuote;

            return await dataProvider.Quote_Async(this);
        }

        //
        // Historical (As-of) pricing
        //

        public async Task<Quote> LastQuoteAsync(DateTime asOf)
        {
            return await dataProvider.Quote_Async(this, asOf);
        }

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

        protected void HandleSocketQuote(object sender, Socket_Quote e)
        {
            Quotes.Add(new Quote(e));

            OnQuoteReceived(LastQuote);
        }
        protected void HandleSocketTrade(object sender, Socket_Trade e)
        {
            Trade newTrade = new Trade(e);

            if (LastQuote != null)
                newTrade.SetTradeSide(LastQuote);

            Trades.Add(newTrade);

            OnTradeReceived(LastTrade);
        }
        protected void HandleSocketAggregateSecond(object sender, Socket_Aggregate e)
        {
            Bar bar = new Bar(e, PolygonTimespan.second, 1);
            Bars.AddBar(bar);
            OnSecondBarReceived(bar);
        }
        protected void HandleSocketAggregateMinute(object sender, Socket_Aggregate e)
        {
            Bar bar = new Bar(e, PolygonTimespan.minute, 1);
            Bars.AddBar(bar);
            OnMinuteBarReceived(bar);
        }

    }
}
