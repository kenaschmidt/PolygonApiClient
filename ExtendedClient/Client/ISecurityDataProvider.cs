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
        event EventHandler<bool> Working;

        Stock GetStock(string symbol);

        Index GetIndex(string symbol);

        Task<bool> ValidateStock(string symbol);

        Task<RestTickerDetail_Result> Ticker_Details_Async(Stock me);

        Task Load_Options_Expiry_Async(IHasOptions me, DateTime expiry);
        Task Load_Options_Chain_Async(IHasOptions me);
        Task Load_Options_Chain_Expired_Async(IHasOptions me, DateTime lookBackStart);


        Task<List<Bar>> Get_Bars_Async(Security me, PolygonTimespan barTimespan, int barMultiplier, DateTime day);
        Task<List<Bar>> Get_Bars_Async(Security me, PolygonTimespan barTimespan, int barMultiplier, DateTime from, DateTime to);


        Task Load_Quotes_And_Trades_Async(Security me, DateTime day);
        Task Load_Quotes_And_Trades_Async(Security me, DateTime from, DateTime to);


        Task<Quote> Quote_Async(Security me, DateTime? asOf = null);
        Task<Trade> Trade_Async(Security me, DateTime? asOf = null);

        Task<double> Value_Async(Index me, DateTime? asOf = null);

        Task<Bar> Previous_Close_Async(Security me);


        Task<PolygonSocketHandler> Stream_Quotes(Security me, bool subscribe);
        Task<PolygonSocketHandler> Stream_Trades(Security me, bool subscribe);
        Task<PolygonSocketHandler> Stream_Second_Bars(Security me, bool subscribe);
        Task<PolygonSocketHandler> Stream_Minute_Bars(Security me, bool subscribe);
        Task<PolygonSocketHandler> Stream_Values(Index me, bool subscribe);
        RestSnapshotHandler Stream_Quotes_Trades_Snapshots(Security security, int secondsInterval = 1);
        RestSnapshotHandler Stream_Greeks_Snapshots(Option option, int secondsInterval = 10);
    }
}
