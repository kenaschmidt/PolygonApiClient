using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PolygonApiClient.ExtendedClient.Models
{
    public class OptionPosition : Position
    {
        public Option Option => this.Security as Option;

        public OptionPosition(Option option) : base(option)
        {
        }

        public OptionPosition(Option option, double quantity) : base(option, quantity)
        {
        }

        public OptionPosition(Option option, double quantity, double averagePrice) : base(option, quantity, averagePrice)
        {
        }

        public async Task<double> PositionDelta_Async(DateTime asOf)
        {
            var ret = await Option.Delta_Async(asOf);

            return (ret * LongShort);
        }
        public async Task<double> PositionDelta_Shares_Async(DateTime asOf)
        {
            var d = await PositionDelta_Async(asOf);

            var ret = d * Quantity * Option.Shares_Per_Contract;

            return ret;
        }

        public async Task<double> PositionDelta_Dollars_Async(DateTime asOf)
        {
            var p = Option.UnderlyingStock.LatestQuoteAsync(asOf);
            var s = PositionDelta_Shares_Async(asOf);

            return (await p).MidpointPrice * (await s);
        }

        public async Task<List<(double price, double deltaDollars)>> PositionDeltaCurve_Dollars_Async(DateTime asOf, double lowPrice, double highPrice, double step = 0.01)
        {
            var p = (await Option.UnderlyingStock.LatestQuoteAsync(asOf)).MidpointPrice;

            var dCurve = await Option.DeltaCurve_Async(asOf, lowPrice, highPrice, step);

            var ret = dCurve.ConvertAll<(double price, double deltaDollars)>(x => (x.price, x.delta * Option.Shares_Per_Contract * Quantity * p));

            return ret;
        }
        public async Task<List<(double price, double deltaShares)>> PositionDeltaCurve_Shares_Async(DateTime asOf, double lowPrice, double highPrice, double step = 0.01)
        {
            var p = (await Option.UnderlyingStock.LatestQuoteAsync(asOf)).MidpointPrice;

            var dCurve = await Option.DeltaCurve_Async(asOf, lowPrice, highPrice, step);

            var ret = dCurve.ConvertAll<(double price, double deltaShares)>(x => (x.price, x.delta * Option.Shares_Per_Contract * Quantity));

            return ret;
        }

        public async Task<double> PositionGamma_Async(DateTime asof)
        {
            var ret = await Option.Gamma_Async(asof);

            return ret * LongShort;
        }
        public async Task<double> PositionGamma_Shares_Async(DateTime asOf)
        {
            var g = await PositionGamma_Async(asOf);

            var ret = g * Quantity * Option.Shares_Per_Contract;

            return ret;
        }
        public async Task<double> PositionGamma_Dollars_Async(DateTime asOf)
        {
            var p = Option.UnderlyingStock.LatestQuoteAsync(asOf);
            var s = PositionGamma_Shares_Async(asOf);

            return (await p).MidpointPrice * (await s);
        }
    }

}
