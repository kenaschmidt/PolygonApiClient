using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PolygonApiClient.ExtendedClient
{
    public class OptionChain
    {
        public Stock UnderlyingStock { get; }

        public HashSet<Option> Options { get; } = new HashSet<Option>();

        public OptionChain(Stock underlyingStock)
        {
            UnderlyingStock = underlyingStock;
        }

        public void AddOption(Option option)
        {
            Options.Add(option);
        }

        public void AddOptions(ICollection<Option> options)
        {
            foreach (Option option in options)
                Options.Add(option);
        }
    }
}
