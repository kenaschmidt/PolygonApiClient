using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PolygonApiClient.ExtendedClient
{
    /// <summary>
    /// Outlines methods that allow security objects to retrieve their own data
    /// </summary>
    public interface ISecurityDataProvider
    {

        Task Load_Options_Chain_Async(Stock me);

        Task Load_Options_Chain_Expired_Async(Stock me, DateTime lookBackStart);


        Task Load_Bars_Async(Security me, PolygonTimespan barTimespan, int barMultiplier, DateTime day);
        Task Load_Bars_Async(Security me, PolygonTimespan barTimespan, int barMultiplier, DateTime from, DateTime to);


        Task Load_Quotes_And_Trades_Async(Security me, DateTime day);
        Task Load_Quotes_And_Trades_Async(Security me, DateTime from, DateTime to);


        Task<Quote> Quote_Async(Security me, DateTime? asOf = null);

    }
}
