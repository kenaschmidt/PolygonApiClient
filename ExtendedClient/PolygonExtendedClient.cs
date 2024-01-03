using PolygonApiClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PolygonApiClient.ExtendedClient
{
    /// <summary>
    /// Provides extended and simplified methods to interact with Polygon API data
    /// </summary>
    public class PolygonExtendedClient : PolygonClient
    {
        public PolygonExtendedClient(string apiKey, PolygonSubscriptionSettings subscriptionSettings) : base(apiKey, subscriptionSettings)
        {
        }
    }
}
