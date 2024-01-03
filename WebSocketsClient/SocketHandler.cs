using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PolygonApiClient
{
    /// <summary>
    /// Go-between class for managing streaking socket data
    /// </summary>
    public class SocketHandler : IEquatable<SocketHandler>
    {
        #region Events

        public event EventHandler<Socket_Aggregate> SecondAggregateReceived;
        public event EventHandler<Socket_Aggregate> MinuteAggregateReceived;
        public event EventHandler<Socket_Quote> QuoteReceived;
        public event EventHandler<Socket_Trade> TradeReceived;

        #endregion

        public string Symbol { get; }

        public SocketHandler(string symbol)
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

        public override bool Equals(object obj)
        {
            return Equals(obj as SocketHandler);
        }
        public bool Equals(SocketHandler other)
        {
            return !(other is null) &&
                   Symbol == other.Symbol;
        }
        public override int GetHashCode()
        {
            return -1758840423 + EqualityComparer<string>.Default.GetHashCode(Symbol);
        }
        public static bool operator ==(SocketHandler left, SocketHandler right)
        {
            return EqualityComparer<SocketHandler>.Default.Equals(left, right);
        }
        public static bool operator !=(SocketHandler left, SocketHandler right)
        {
            return !(left == right);
        }
    }
}
