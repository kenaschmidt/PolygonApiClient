using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PolygonApiClient.ExtendedClient
{

    public class OptionChain
    {
        public Security UnderlyingSecurity { get; }

        internal Dictionary<DateTime, OptionExpiry> Options { get; } = new Dictionary<DateTime, OptionExpiry>();

        public List<DateTime> Expiries => Options.Keys.ToList();

        public List<double> StrikeRange => Options.Values.SelectMany(x => x.Strikes).ToList();

        internal OptionChain(Security underlyingSecurity)
        {
            UnderlyingSecurity = underlyingSecurity;
        }

        internal void AddOption(Option option)
        {
            if (Options.ContainsKey(option.Expiry))
                Options[option.Expiry].AddOption(option);

            else
            {
                Options.Add(option.Expiry, new OptionExpiry(this.UnderlyingSecurity, option.Expiry, option));
            }
        }

        internal void AddOptions(IEnumerable<Option> options)
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
