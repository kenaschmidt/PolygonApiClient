using System;
using System.Collections.Generic;
using System.Linq;

namespace PolygonApiClient.ExtendedClient.Models
{
    //
    // A Position represents quantity of a specific security held
    //
    public class Position
    {
        public Security Security { get; }

        private double _InitialQuantity { get; set; }
        private double _AdditionalQuantity { get; set; }
        public double Quantity
        {
            get => _AdditionalQuantity + _InitialQuantity;
        }

        private double _InitialAveragePrice { get; set; }
        private double _AdjustedAveragePrice { get; set; }
        public double AveragePrice
        {
            get => ((_AdjustedAveragePrice * Quantity)
                + (_InitialAveragePrice * _InitialQuantity))
                / Quantity;
        }

        public int LongShort => Math.Sign(Quantity);

        public List<Trade> Trades { get; private set; } = new List<Trade>();

        public Position(Security security)
        {
            Security = security ?? throw new ArgumentNullException(nameof(security));
        }
        public Position(Security security, double quantity) : this(security)
        {
            _InitialQuantity = quantity;
        }
        public Position(Security security, double quantity, double averagePrice) : this(security, quantity)
        {
            _InitialAveragePrice = averagePrice;
        }

        public void AddTrade(Trade trade)
        {
            this.Trades.Add(trade);
            calculateFromTrades();
        }
        public void AddTrades(List<Trade> trades)
        {
            this.Trades.AddRange(trades);
            calculateFromTrades();
        }

        protected void calculateFromTrades(DateTime? asOf = null)
        {
            if (this.Trades.Count == 0)
                return;

            if (asOf.HasValue)
            {
                var trades = this.Trades.Where(t => t.Timestamp.EST <= asOf.Value).ToList();
                calculateAveragePriceFromTrades(trades);
                calculateQuantityFromTrades(trades);
            }
            else
            {
                calculateAveragePriceFromTrades(this.Trades);
                calculateQuantityFromTrades(this.Trades);
            }

        }
        private void calculateAveragePriceFromTrades(List<Trade> trades)
        {
            // Calculates average position price from trade list.
            // If the position zeros out, calculation starts over

            double avgPrice = 0.0;
            double quantity = 0.0;

            foreach (var trade in trades)
            {
                if (quantity == 0)
                {
                    avgPrice = 0;
                }
                else if (quantity > 0)
                {
                    if (trade.TradeSide == TradeSide.buy)
                    {
                        avgPrice = ((avgPrice * quantity) + (trade.TradePrice * trade.TradeSize));
                        quantity += trade.TradeSize;
                        avgPrice /= quantity;
                    }
                    else if (trade.TradeSide == TradeSide.sell)
                    {
                        quantity -= trade.TradeSize;
                    }
                    else
                    {
                        // No side
                    }
                }
                else
                {
                    if (trade.TradeSide == TradeSide.buy)
                    {
                        quantity += trade.TradeSize;
                    }
                    else if (trade.TradeSide == TradeSide.sell)
                    {
                        avgPrice = ((avgPrice * Math.Abs(quantity)) + (trade.TradePrice * trade.TradeSize));
                        quantity -= trade.TradeSize;
                        avgPrice /= Math.Abs(quantity);
                    }
                    else
                    {
                        // No size
                    }
                }
            }

            _AdjustedAveragePrice = avgPrice;
        }
        private void calculateQuantityFromTrades(List<Trade> trades)
        {
            _AdditionalQuantity = (trades.Sum(t => t.TradeSize * (int)t.TradeSide));
        }
    }

}
