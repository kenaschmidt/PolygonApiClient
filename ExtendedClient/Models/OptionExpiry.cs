using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace PolygonApiClient.ExtendedClient
{
    /// <summary>
    /// Collection of options with the same expiration (curve)
    /// </summary>
    public class OptionExpiry
    {
        public Security UnderlyingSecurity { get; }
        public DateTime Expiry { get; }
        public bool IsExpired => Expiry < DateTime.Today;

        public List<double> Strikes => (from opt in Options select opt.Strike).Distinct().ToList();
        public List<Option> Puts => Options.Puts();
        public List<Option> Calls => Options.Calls();
        public List<Option> All => Options;
        public long Open_Interest => Options.Sum(x => x.Open_Interest);
        public long Open_Interest_Calls => Options.Calls().Sum(x => x.Open_Interest);
        public long Open_Interest_Puts => Options.Puts().Sum(x => x.Open_Interest);

        protected List<Option> Options { get; } = new List<Option>();

        public OptionExpiry(Security underlyingSecurity, DateTime expiry)
        {
            UnderlyingSecurity = underlyingSecurity ?? throw new ArgumentNullException(nameof(underlyingSecurity));
            Expiry = expiry;
        }
        public OptionExpiry(Security underlyingSecurity, DateTime expiry, Option option)
        {
            UnderlyingSecurity = underlyingSecurity ?? throw new ArgumentNullException(nameof(underlyingSecurity));
            Expiry = expiry;

            if (option.Expiry != expiry)
                throw new ArgumentException($"{MethodBase.GetCurrentMethod().Name} Option expiry mismatch");
            Options.Add(option);
        }
        public OptionExpiry(Security underlyingSecurity, DateTime expiry, IEnumerable<Option> options)
        {
            UnderlyingSecurity = underlyingSecurity ?? throw new ArgumentNullException(nameof(underlyingSecurity));
            Expiry = expiry;

            AddOptions(options);
        }
        public void AddOptions(IEnumerable<Option> options)
        {
            foreach (var option in options)
                AddOption(option);
        }
        public void AddOption(Option option)
        {
            if (option.Expiry != this.Expiry)
                throw new ArgumentException($"{MethodBase.GetCurrentMethod().Name} Option expiry mismatch");

            if (Options.Contains(option))
                return;

            option.ExpiryCurve = this;

            Options.Add(option);
        }

        public List<Option> Strike(double strike)
        {
            var ret = Options.ByStrike(strike);
            return ret;
        }

        public async Task<List<Option>> ATMStrikes(int count = 1)
        {
            var ret = new List<Option>();

            var underlyingPrice = (await UnderlyingSecurity.LatestQuoteAsync()).MidpointPrice;

            var above = Strikes.Where(s => s >= underlyingPrice).OrderBy(s => s).Take(count);
            var below = Strikes.Where(s => s < underlyingPrice).OrderByDescending(s => s).Take(count);
            var allStrikes = above.Union(below);

            foreach (var val in allStrikes)
            {
                ret.Add(Calls.ByStrike(val).SingleOrDefault());
                ret.Add(Puts.ByStrike(val).SingleOrDefault());
            }

            ret.OrderBy(s => s.Strike);

            return ret;
        }


    }
}
