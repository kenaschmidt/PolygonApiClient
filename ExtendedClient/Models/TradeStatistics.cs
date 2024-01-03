using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PolygonApiClient.ExtendedClient
{
    public class TradeStatistics : IEquatable<TradeStatistics>
    {
        #region Events

        public event EventHandler<Trade> SocketTradeReceived;
        private void OnSocketTradeReceived(Trade t)
        {
            SocketTradeReceived?.Invoke(this, t);
        }

        #endregion

        public Security Security { get; }

        public TradeStatistics(Security security)
        {
            Security = security;

            addHandlers();
        }

        private void addHandlers()
        {
            if (Security == null)
                return;

            Security.TradeReceived += Security_SocketTradeReceived;
        }

        private void Security_SocketTradeReceived(object sender, Trade e)
        {
            Trades.Add(e);

            // Update statistics
            TotalVolume += e.TradeSize;
            TotalBuyVolume += e.TradeSide == TradeSide.buy ? e.TradeSize : 0;
            TotalSellVolume += e.TradeSide == TradeSide.sell ? e.TradeSize : 0;

            OnSocketTradeReceived(e);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as TradeStatistics);
        }

        public bool Equals(TradeStatistics other)
        {
            return !(other is null) &&
                   EqualityComparer<Security>.Default.Equals(Security, other.Security);
        }

        public override int GetHashCode()
        {
            return 11019027 + EqualityComparer<Security>.Default.GetHashCode(Security);
        }

        protected List<Trade> Trades { get; } = new List<Trade>();

        #region Calculated Statistics

        public long TotalVolume { get; protected set; } = 0;
        public long TotalBuyVolume { get; protected set; } = 0;
        public long TotalSellVolume { get; protected set; } = 0;

        public static bool operator ==(TradeStatistics left, TradeStatistics right)
        {
            return EqualityComparer<TradeStatistics>.Default.Equals(left, right);
        }

        public static bool operator !=(TradeStatistics left, TradeStatistics right)
        {
            return !(left == right);
        }

        #endregion
    }

    /// <summary>
    /// Manages multiple TradeStatistics objects to present aggregate figures
    /// </summary>
    public class TradeStatisticsAggregator
    {
        public TradeStatisticsAggregator()
        {
        }

        protected HashSet<TradeStatistics> TradeStatistics { get; } = new HashSet<TradeStatistics>();

        public void AddStatistics(TradeStatistics statistics)
        {
            TradeStatistics.Add(statistics);
        }

        public long TotalVolume => TradeStatistics.Sum(x => x.TotalVolume);
        public long TotalBuyVolume => TradeStatistics.Sum(x => x.TotalBuyVolume);
        public long TotalSellVolume => TradeStatistics.Sum(x => x.TotalSellVolume);
    }
}
