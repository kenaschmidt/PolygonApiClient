using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PolygonApiClient.ExtendedClient
{

    /// <summary>
    /// Collection of options with the same expiration (curve)
    /// </summary>
    public class OptionExpiry
    {
        public Stock UnderlyingStock { get; }
        public DateTime Expiry { get; }
        public bool IsExpired => Expiry < DateTime.Today;

        public List<double> Strikes => (from opt in Options select opt.Strike).ToList();
        public List<Option> Puts => Options.Puts();
        public List<Option> Calls => Options.Calls();
        public List<Option> All => Options;
        public long Open_Interest => Options.Sum(x => x.Open_Interest);

        protected List<Option> Options { get; } = new List<Option>();

        public OptionExpiry(Stock underlyingStock, DateTime expiry)
        {
            UnderlyingStock = underlyingStock ?? throw new ArgumentNullException(nameof(underlyingStock));
            Expiry = expiry;
        }
        public OptionExpiry(Stock underlyingStock, DateTime expiry, Option option)
        {
            UnderlyingStock = underlyingStock ?? throw new ArgumentNullException(nameof(underlyingStock));
            Expiry = expiry;

            if (option.Expiry != expiry)
                throw new ArgumentException($"{MethodBase.GetCurrentMethod().Name} Option expiry mismatch");
            Options.Add(option);
        }
        public OptionExpiry(Stock underlyingStock, DateTime expiry, IEnumerable<Option> options)
        {
            UnderlyingStock = underlyingStock ?? throw new ArgumentNullException(nameof(underlyingStock));
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
    }

    public class OptionChain
    {
        public Stock UnderlyingStock { get; }

        public Dictionary<DateTime, OptionExpiry> Options { get; } = new Dictionary<DateTime, OptionExpiry>();

        public List<DateTime> Expiries => Options.Keys.ToList();

        public List<double> StrikeRange => Options.Values.SelectMany(x => x.Strikes).ToList();

        public OptionChain(Stock underlyingStock)
        {
            UnderlyingStock = underlyingStock;
        }

        public void AddOption(Option option)
        {
            if (Options.ContainsKey(option.Expiry))
                Options[option.Expiry].AddOption(option);

            else
            {
                Options.Add(option.Expiry, new OptionExpiry(this.UnderlyingStock, option.Expiry, option));
            }
        }

        public void AddOptions(IEnumerable<Option> options)
        {
            foreach (Option option in options)
                AddOption(option);
        }

        public OptionExpiry Expiry(DateTime expiry)
        {
            var ret = Options[expiry.Date];
            return ret;
        }

        public OptionExpiry NextExpiry()
        {
            return Options[Options.Min(x => x.Key)];
        }

    }
}
