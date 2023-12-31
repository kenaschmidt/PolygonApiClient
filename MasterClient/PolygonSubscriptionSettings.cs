using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PolygonApiClient.MasterClient
{
    /// <summary>
    /// A class that maintains values for user access based on subscription level. All values will default to Basic (Free) unless set
    /// </summary>
    public class PolygonSubscriptionSettings
    {
        public StocksSettings Stocks { get; private set; }
        public OptionsSettings Options { get; private set; }
        public IndicesSettings Indices { get; private set; }
        public CurrenciesSettings Currencies { get; private set; }

        public PolygonSubscriptionSettings(
            PolygonSubscription stocks = PolygonSubscription.basic,
            PolygonSubscription options = PolygonSubscription.basic,
            PolygonSubscription indices = PolygonSubscription.basic,
            PolygonSubscription currencies = PolygonSubscription.basic)
        {
            Stocks = new StocksSettings(stocks);
            Options = new OptionsSettings(options);
            Indices = new IndicesSettings(indices);
            Currencies = new CurrenciesSettings(currencies);
        }

    }

    public abstract class AssetClassSettings
    {
        public PolygonSubscription Subsription { get; private set; }

        public abstract DateTime HistoricalDataLookback { get; }

        public AssetClassSettings(PolygonSubscription subscription)
        {
            Subsription = subscription;
        }
    }
    public class StocksSettings : AssetClassSettings
    {
        public override DateTime HistoricalDataLookback
        {
            get
            {
                switch (Subsription)
                {
                    case PolygonSubscription.basic:
                        return DateTime.Today.AddYears(-2);
                    case PolygonSubscription.starter:
                        return DateTime.Today.AddYears(-5);
                    case PolygonSubscription.developer:
                        return DateTime.Today.AddYears(-10);
                    case PolygonSubscription.advanced:
                        return DateTime.Today.AddYears(-15);
                    default:
                        return DateTime.Today.AddYears(-2);
                }
            }
        }

        public StocksSettings(PolygonSubscription subscription) : base(subscription)
        {
        }
    }
    public class OptionsSettings : AssetClassSettings
    {
        public override DateTime HistoricalDataLookback
        {
            get
            {
                switch (Subsription)
                {
                    case PolygonSubscription.basic:
                        return DateTime.Today.AddYears(-2);
                    case PolygonSubscription.starter:
                        return DateTime.Today.AddYears(-2);
                    case PolygonSubscription.developer:
                        return DateTime.Today.AddYears(-4);
                    case PolygonSubscription.advanced:
                        return DateTime.Today.AddYears(-5);
                    default:
                        return DateTime.Today.AddYears(-2);
                }
            }
        }

        public OptionsSettings(PolygonSubscription subscription) : base(subscription)
        {
        }
    }
    public class CurrenciesSettings : AssetClassSettings
    {
        public override DateTime HistoricalDataLookback
        {
            get
            {
                switch (Subsription)
                {
                    case PolygonSubscription.basic:
                        return DateTime.Today.AddYears(-2);
                    case PolygonSubscription.starter:
                        return DateTime.Today.AddYears(-10);
                    default:
                        return DateTime.Today.AddYears(-2);
                }
            }
        }

        public CurrenciesSettings(PolygonSubscription subscription) : base(subscription)
        {
        }
    }
    public class IndicesSettings : AssetClassSettings
    {
        public override DateTime HistoricalDataLookback
        {
            get
            {
                switch (Subsription)
                {
                    case PolygonSubscription.basic:
                        return DateTime.Today.AddYears(0);
                    case PolygonSubscription.starter:
                        return DateTime.Today.AddYears(0);
                    case PolygonSubscription.advanced:
                        return DateTime.Today.AddYears(0);
                    default:
                        return DateTime.Today.AddYears(0);
                }
            }
        }

        public IndicesSettings(PolygonSubscription subscription) : base(subscription)
        {
        }
    }

}
