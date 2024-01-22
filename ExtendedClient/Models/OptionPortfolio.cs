using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PolygonApiClient.ExtendedClient.Models
{
    public class OptionPortfolio : Portfolio
    {
        public async Task<double> PortfolioDelta_Async(DateTime asOf)
        {
            double portfolioDeltaShares = await PortfolioDelta_Shares_Async(asOf);

            var contracts = Positions.Sum(p => p.Quantity);

            var shares_per_contract = (Positions.First() as OptionPosition).Option.Shares_Per_Contract; // This makes an assumption that all contracts have the same terms (100 shares)

            return (portfolioDeltaShares / shares_per_contract / contracts);
        }
        public async Task<double> PortfolioDelta_Shares_Async(DateTime asOf)
        {
            var tasks = new List<Task<double>>();

            foreach (OptionPosition position in Positions.AsParallel())
                tasks.Add(position.PositionDelta_Shares_Async(asOf));

            await Task.WhenAll(tasks.ToArray());

            var portfolioDeltaShares = tasks.Sum(t => t.Result);

            return portfolioDeltaShares;
        }
        public async Task<double> PortfolioDelta_Dollars(DateTime asOf)
        {
            var tasks = new List<Task<double>>();

            foreach (OptionPosition position in Positions.AsParallel())
                tasks.Add(position.PositionDelta_Dollars_Async(asOf));

            await Task.WhenAll(tasks.ToArray());

            var portfolioDeltaDollars = tasks.Sum(t => t.Result);

            return portfolioDeltaDollars;
        }

        public async Task<List<(double price, double delta)>> PortfolioDeltaCurve_Async(DateTime asOf, double lowPrice, double highPrice, double step = 0.01)
        {
            var _ret = await PortfolioDeltaCurve_Shares_Async(asOf, lowPrice, highPrice, step);

            // Absolute value sum of positions to divide by
            double sumPositions = Positions.Sum(x => Math.Abs(x.Quantity));

            List<(double price, double delta)> ret = new List<(double price, double delta)>();

            foreach (var _r in _ret)
                ret.Add((_r.price, _r.deltaShares / 100.0 / sumPositions));

            return ret;
        }

        public async Task<List<(double price, double deltaShares)>> PortfolioDeltaCurve_Shares_Async(DateTime asOf, double lowPrice, double highPrice, double step = 0.01)
        {
            var _ret = new Dictionary<double, double>();

            for (double i = lowPrice; i <= highPrice; i += step)
                _ret.Add(i, 0.0);

            var tasks = new List<Task<List<(double price, double deltaShares)>>>();

            foreach (OptionPosition position in Positions)
                tasks.Add(position.PositionDeltaCurve_Shares_Async(asOf, lowPrice, highPrice, step));

            await Task.WhenAll(tasks.ToArray());

            foreach (var task in tasks)
                foreach (var val in task.Result)
                    _ret[val.price] += val.deltaShares;

            List<(double price, double deltaShares)> ret = new List<(double price, double deltaShares)>();

            foreach (var _r in _ret)
                ret.Add((_r.Key, _r.Value));

            return ret;
        }

        public async Task<List<(double price, double deltaDollars)>> PortfolioDeltaCurve_Dollars_Async(DateTime asOf, double lowPrice, double highPrice, double step = 0.01)
        {
            var _ret = new Dictionary<double, double>();

            for (double i = lowPrice; i <= highPrice; i += step)
                _ret.Add(i, 0.0);

            var tasks = new List<Task<List<(double price, double deltaDollars)>>>();

            foreach (OptionPosition position in Positions)
                tasks.Add(position.PositionDeltaCurve_Dollars_Async(asOf, lowPrice, highPrice, step));

            await Task.WhenAll(tasks.ToArray());

            foreach (var task in tasks)
                foreach (var val in task.Result)
                    _ret[val.price] += val.deltaDollars;

            List<(double price, double deltaDollars)> ret = new List<(double price, double deltaDollars)>();

            foreach (var _r in _ret)
                ret.Add((_r.Key, _r.Value));

            return ret;
        }



        public async Task<double> PortfolioGamma_Async(DateTime asOf)
        {
            double portfolioGammaShares = await PortfolioGamma_Shares_Async(asOf);

            var contracts = Positions.Sum(p => p.Quantity);

            var shares_per_contract = (Positions.First() as OptionPosition).Option.Shares_Per_Contract; // This makes an assumption that all contracts have the same terms (100 shares)

            return (portfolioGammaShares / shares_per_contract / contracts);
        }
        public async Task<double> PortfolioGamma_Shares_Async(DateTime asOf)
        {
            var tasks = new List<Task<double>>();

            foreach (OptionPosition position in Positions.AsParallel())
                tasks.Add(position.PositionGamma_Shares_Async(asOf));

            await Task.WhenAll(tasks.ToArray());

            var portfolioGammaShares = tasks.Sum(t => t.Result);

            return portfolioGammaShares;
        }
        public async Task<double> PortfolioGamma_Dollars_Async(DateTime asOf)
        {
            var tasks = new List<Task<double>>();

            foreach (OptionPosition position in Positions.AsParallel())
                tasks.Add(position.PositionGamma_Dollars_Async(asOf));

            await Task.WhenAll(tasks.ToArray());

            var portfolioGammaDollar = tasks.Sum(t => t.Result);

            return portfolioGammaDollar;
        }
    }

}
