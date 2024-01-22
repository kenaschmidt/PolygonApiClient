using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PolygonApiClient.ExtendedClient
{
    public class NoPriceDataException : Exception
    {
        public Security Security { get; set; }

        public NoPriceDataException(Security security)
        {
            Security = security ?? throw new ArgumentNullException(nameof(security));
        }
    }
}
