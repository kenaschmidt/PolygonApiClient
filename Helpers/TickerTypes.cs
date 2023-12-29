using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PolygonApiClient
{
    public enum PolygonTickerType
    {
        None = 0,
        [Description("Common Stock")] CS = 1,
        [Description("Preferred Stock")] PFD = 2,
        [Description("Warrant")] WARRANT = 3,
        [Description("Rights")] RIGHT = 4,
        [Description("Corporate Bond")] BOND = 5,
        [Description("Exchange Traded Fund")] ETF = 6,
        [Description("Exchange Traded Note")] ETN = 7,
        [Description("Exchange Traded Vehicle")] ETV = 8,
        [Description("Structured Product")] SP = 9,
        [Description("American Depository Receipt Common")] ADRC = 10,
        [Description("American Depository Receipt Preferred")] ADRP = 11,
        [Description("American Depository Receipt Warrants")] ADRW = 12,
        [Description("American Depository Receipt Rights")] ADRR = 13,
        [Description("Fund")] FUND = 14,
        [Description("Basket")] BASKET = 15,
        [Description("Unit")] UNIT = 16,
        [Description("Liquidating Trust")] LT = 17,
        [Description("Ordinary Shares")] OS = 18,
        [Description("Global Depository Receipts")] GDR = 19,
        [Description("Other Security Type")] OTHER = 20,
        [Description("New York Registry Shares")] NYRS = 21,
        [Description("Agency Bond")] AGEN = 22,
        [Description("Equity Linked Bond")] EQLK = 23,
        [Description("Single-security ETF")] ETS = 24
    }
}
