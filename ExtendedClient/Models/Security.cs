using PolygonApiClient.ExtendedClient;
using PolygonApiClient.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Permissions;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace PolygonApiClient.ExtendedClient
{
    public abstract class Security : IEquatable<Security>
    {
        #region Events

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

        #endregion

        protected SocketHandler SocketHandler { get; set; }

        public string Symbol { get; }

        protected Security(string symbol)
        {
            Symbol = symbol;
        }

        public List<Trade> Trades { get; protected set; } = new List<Trade>();
        public List<Quote> Quotes { get; protected set; } = new List<Quote>();

        public Trade LastTrade => Trades.LastOrDefault();
        public Quote LastQuote => Quotes.LastOrDefault();

        public void AttachSocketHandler(SocketHandler socketHandler)
        {
            if (SocketHandler != null)
                return;

            SocketHandler = socketHandler;

            SocketHandler.QuoteReceived += HandleSocketQuote;
            SocketHandler.TradeReceived += HandleSocketTrade;
            SocketHandler.SecondAggregateReceived += HandleSocketAggregateSecond;
            SocketHandler.MinuteAggregateReceived += HandleSocketAggregateMinute;
        }

        protected void HandleSocketQuote(object sender, Socket_Quote e)
        {
            Quotes.Add(new Quote(e));

            OnQuoteReceived(LastQuote);
        }
        protected void HandleSocketTrade(object sender, Socket_Trade e)
        {
            if (LastQuote != null)
                LastTrade.SetTradeSide(LastQuote);

            Trades.Add(new Trade(e));

            OnTradeReceived(LastTrade);
        }
        protected void HandleSocketAggregateSecond(object sender, Socket_Aggregate e)
        {

        }
        protected void HandleSocketAggregateMinute(object sender, Socket_Aggregate e)
        {

        }

        public void AddQuotes(RestQuotes_Result[] quotes)
        {
            Quotes.AddRange(quotes.ToQuotes());
            Debug.WriteLine($"Added {quotes.Length} quotes to {this.Symbol}");
        }
        public void AddQuotes(List<Quote> quotes)
        {
            Quotes.AddRange(quotes);
            Debug.WriteLine($"Added {quotes.Count} quotes to {this.Symbol}");
        }
        public void AddTrades(RestTrades_Result[] trades)
        {
            Trades.AddRange(trades.ToTrades());
            Debug.WriteLine($"Added {trades.Length} trades to {this.Symbol}");
        }
        public void AddTrades(List<Trade> trades)
        {
            Trades.AddRange(trades);
            Debug.WriteLine($"Added {trades.Count} trades to {this.Symbol}");
        }

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
    }

    public class Stock : Security
    {
        public OptionChain OptionChain { get; }

        public Stock(string symbol) : base(symbol)
        {
            OptionChain = new OptionChain(this);
        }

        public void AddOptions(List<IPolygonOptionContract> options)
        {
            var newOptions = options.ConvertAll<Option>
                ((IPolygonOptionContract x) => { return new Option(x); });

            OptionChain.AddOptions(newOptions);

            Debug.WriteLine($"Added {OptionChain.Options.Count} option contract to {Symbol}");
        }
        public void AddOptions(IPolygonOptionContract[] options)
        {
            var newOptions = Array.ConvertAll<IPolygonOptionContract, Option>
                (options, (IPolygonOptionContract x) => { return new Option(x); });

            OptionChain.AddOptions(newOptions);

            Debug.WriteLine($"Added {OptionChain.Options.Count} option contract to {Symbol}");
        }
        public void AddOptions(RestOptionsChain_Result[] optionsResults)
        {
            var details = (from result in optionsResults
                           select result.Details).ToList<IPolygonOptionContract>();

            AddOptions(details);
        }
    }

    public class Option : Security
    {
        protected IPolygonOptionContract contractBase { get; }

        public DateTime Expiry { get; }
        public double Strike => contractBase.Strike_Price;
        public OptionType OptionType { get; }

        public Option(IPolygonOptionContract optionDetails) : base(optionDetails.Symbol)
        {
            contractBase = optionDetails;
            Expiry = DateTime.Parse(contractBase.Expiration_Date);
            OptionType = (OptionType)Enum.Parse(typeof(OptionType), contractBase.Contract_Type);
        }
    }

    public abstract class Tick
    {
        public PolygonTimestamp Timestamp { get; protected set; }

        protected Tick()
        {
        }
    }

    public class Quote : Tick
    {
        protected IPolygonQuote TickBase { get; }

        public double BidPrice => TickBase.Bid_Price;
        public long BidSize => TickBase.Bid_Size;
        public double AskPrice => TickBase.Ask_Price;
        public double AskSize => TickBase.Ask_Size;
        public double Spread => (AskPrice - BidPrice);

        public Quote(Socket_Quote tickBase)
        {
            TickBase = tickBase;
            Timestamp = PolygonTimestamp.FromMilliseconds(tickBase.Timestamp_Ms);
        }

        public Quote(RestQuotes_Result tickBase)
        {
            TickBase = tickBase;
            Timestamp = PolygonTimestamp.FromNanoseconds(tickBase.Participant_Timestamp_Ns);
        }
    }

    public class Trade : Tick
    {
        protected IPolygonTrade TickBase { get; }

        public double TradePrice => TickBase.Price;
        public long TradeSize => TickBase.Size;
        public TradeSide TradeSide { get; set; }

        public Trade(Socket_Trade tickBase)
        {
            TickBase = tickBase;
            Timestamp = PolygonTimestamp.FromMilliseconds(tickBase.Timestamp_Ms);
        }

        public Trade(RestTrades_Result tickBase)
        {
            TickBase = tickBase;
            Timestamp = PolygonTimestamp.FromNanoseconds(tickBase.Participant_Timestamp_Ns);
        }
    }

    public class Bar
    {

    }
}
