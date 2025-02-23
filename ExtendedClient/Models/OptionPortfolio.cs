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
