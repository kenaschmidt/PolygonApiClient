using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PolygonApiClient
{
    /// <summary>
    /// Go-between class for managing streaming socket data
    /// </summary>
    public class PolygonSocketHandler : IEquatable<PolygonSocketHandler>
    {
        #region Events

        public event EventHandler<Socket_Aggregate> SecondAggregateReceived;
        public event EventHandler<Socket_Aggregate> MinuteAggregateReceived;
        public event EventHandler<Socket_Quote> QuoteReceived;
        public event EventHandler<Socket_Trade> TradeReceived;
        public event EventHandler<Socket_Value> ValueReceived;

        #endregion

        public string Symbol { get; }

        public bool QuotesStreaming { get; set; } = false;
        public bool TradesStreaming { get; set; } = false;
        public bool SecondsStreaming { get; set; } = false;
        public bool MinutesStreaming { get; set; } = false;
        public bool ValueStreaming { get; set; } = false;

        public PolygonSocketHandler(string symbol)
        {
            Symbol = symbol;
        }

        public virtual void A(Socket_Aggregate a)
        {
            SecondAggregateReceived?.Invoke(this, a);
        }
        public virtual void AM(Socket_Aggregate am)
        {
            MinuteAggregateReceived?.Invoke(this, am);
        }
        public virtual void Q(Socket_Quote q)
        {
            QuoteReceived?.Invoke(this, q);
        }
        public virtual void T(Socket_Trade t)
        {
            TradeReceived?.Invoke(this, t);
        }
        public virtual void V(Socket_Value v)
        {
            ValueReceived?.Invoke(this, v);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as PolygonSocketHandler);
        }
        public bool Equals(PolygonSocketHandler other)
        {
            return !(other is null) &&
                   Symbol == other.Symbol;
        }
        public override int GetHashCode()
        {
            return -1758840423 + EqualityComparer<string>.Default.GetHashCode(Symbol);
        }
        public static bool operator ==(PolygonSocketHandler left, PolygonSocketHandler right)
        {
            return EqualityComparer<PolygonSocketHandler>.Default.Equals(left, right);
        }
        public static bool operator !=(PolygonSocketHandler left, PolygonSocketHandler right)
        {
            return !(left == right);
        }
    }
}
