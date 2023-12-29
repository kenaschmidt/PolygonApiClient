using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PolygonApiClient
{
    public enum Tape
    {
        [Description("NYSE")]
        A = 1,
        [Description("ARCA")]
        B = 2,
        [Description("NASDAQ")]
        C = 3
    }

}
