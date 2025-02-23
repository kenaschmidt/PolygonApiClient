using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PolygonApiClient
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

        public AssetClassSettings GetSettings(PolygonConnectionEndpoint endpoint)
        {
            switch (endpoint)
            {
                case PolygonConnectionEndpoint.stocks:
                    return Stocks;
                case PolygonConnectionEndpoint.options:
                    return Options;
                case PolygonConnectionEndpoint.indices:
                    return Indices;
                case PolygonConnectionEndpoint.forex:
                case PolygonConnectionEndpoint.crypto:
                    return Currencies;
                default:
                    return null;
            }
        }
    }

    public abstract class AssetClassSettings
    {
        public PolygonSubscription Subscription { get; private set; }

        public abstract DateTime HistoricalDataLookback { get; }
        public PolygonPermissions Permissions { get; private set; }

        public AssetClassSettings(PolygonSubscription subscription)
        {
            Subscription = subscription;

            setPermissionFlags();
        }

        private void setPermissionFlags()
        {
            //
            // Stocks
            //
            if (this.GetType() == typeof(StocksSettings))
            {
                switch (Subscription)
                {
                    case PolygonSubscription.basic:
                        Permissions =
                            PolygonPermissions.None;
                        break;
                    case PolygonSubscription.starter:
                        Permissions =
                            PolygonPermissions.Snapshots |
                            PolygonPermissions.WebSockets;
                        break;
                    case PolygonSubscription.developer:
                        Permissions =
                            PolygonPermissions.Snapshots |
                            PolygonPermissions.WebSockets |
                            PolygonPermissions.Trades;
                        break;
                    case PolygonSubscription.advanced:
                        Permissions =
                            PolygonPermissions.Snapshots |
                            PolygonPermissions.WebSockets |
                            PolygonPermissions.Trades |
                            PolygonPermissions.Quotes;
                        break;
                    default:
                        Permissions =
                            PolygonPermissions.None;
                        break;
                }
            }

            //
            // Options
            //
            if (this.GetType() == typeof(OptionsSettings))
            {
                switch (Subscription)
                {
                    case PolygonSubscription.basic:
                        Permissions =
                            PolygonPermissions.None;
                        break;
                    case PolygonSubscription.starter:
                        Permissions =
                            PolygonPermissions.Snapshots |
                            PolygonPermissions.WebSockets;
                        break;
                    case PolygonSubscription.developer:
                        Permissions =
                            PolygonPermissions.Snapshots |
                            PolygonPermissions.WebSockets |
                            PolygonPermissions.Trades;
                        break;
                    case PolygonSubscription.advanced:
                        Permissions =
                            PolygonPermissions.Snapshots |
                            PolygonPermissions.WebSockets |
                            PolygonPermissions.Trades |
                            PolygonPermissions.Quotes;
                        break;
                    default:
                        Permissions =
                            PolygonPermissions.None;
                        break;
                }
            }

            //
            // Indices
            //
            if (this.GetType() == typeof(IndicesSettings))
            {
                switch (Subscription)
                {
                    case PolygonSubscription.basic:
                        Permissions =
                            PolygonPermissions.None;
                        break;
                    case PolygonSubscription.starter:
                    case PolygonSubscription.advanced:
                        Permissions =
                            PolygonPermissions.Snapshots |
                            PolygonPermissions.WebSockets;
                        break;
                    default:
                        Permissions =
                            PolygonPermissions.None;
                        break;
                }
            }

            //
            // Currencies
            //
            if (this.GetType() == typeof(CurrenciesSettings))
            {
                switch (Subscription)
                {
                    case PolygonSubscription.basic:
                        Permissions =
                            PolygonPermissions.None;
                        break;
                    case PolygonSubscription.starter:
                        Permissions =
                            PolygonPermissions.Snapshots |
                            PolygonPermissions.WebSockets |
                            PolygonPermissions.Trades |
                            PolygonPermissions.Quotes;
                        break;
                    default:
                        Permissions =
                            PolygonPermissions.None;
                        break;
                }
            }
        }
        public bool HasPermission(PolygonPermissions permission)
        {
            return Permissions.HasFlag(permission);
        }
    }

    public class StocksSettings : AssetClassSettings
    {
        public override DateTime HistoricalDataLookback
        {
            get
            {
                switch (Subscription)
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
                switch (Subscription)
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
                switch (Subscription)
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
                switch (Subscription)
                {
                    case PolygonSubscription.basic:
                        return DateTime.Today.AddYears(1);
                    case PolygonSubscription.starter:
                        return DateTime.Today.AddYears(1);
                    case PolygonSubscription.advanced:
                        return DateTime.Today.AddYears(1);
                    default:
                        return DateTime.Today.AddYears(1);
                }
            }
        }

        public IndicesSettings(PolygonSubscription subscription) : base(subscription)
        {
        }
    }
}
