using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PolygonApiClient
{
    public enum Exchange
    {
        [Description("NYSE American")] XASE = 1,
        [Description("Nasdaq OMX BX")] XBOS = 2,
        [Description("NYSE National")] XCIS = 3,
        [Description("FINRA")] FINY = 4,
        [Description("Unlisted Trading Privileges")] UNLISTED = 5,
        [Description("International Securities Exchange")] XISE = 6,
        [Description("Cboe EDGA")] EDGA = 7,
        [Description("Cboe EDGX")] EDGX = 8,
        [Description("NYSE Chicago")] XCHI = 9,
        [Description("New York Stock Exchange")] XNYS = 10,
        [Description("NYSE Arca")] ARCX = 11,
        [Description("Nasdaq")] XNAS = 12,
        [Description("Consolidated Tape Association")] CONSOLIDATED = 13,
        [Description("Long-Term Stock Exchange")] LTSE = 14,
        [Description("Investors Exchange")] IEXG = 15,
        [Description("Cboe Stock Exchange")] CBSX = 16,
        [Description("Nasdaq Philadelphia Exchange LLC")] XPHL = 17,
        [Description("Cboe BYX")] BATY = 18,
        [Description("Cboe BZX")] BATS = 19,
        [Description("MIAX Pearl")] EPRL = 20,
        [Description("Members Exchange")] MEMX = 21,
        [Description("OTC Equity Security")] OOTC = 62,
        [Description("NYSE American Options")] AMXO = 300,
        [Description("Boston Options Exchange")] XBOX = 301,
        [Description("Chicago Board Options Exchange")] XCBO = 302,
        [Description("MIAX Emerald")] EMLD = 303,
        [Description("Cboe EDGX Options")] EDGO = 304,
        [Description("Nasdaq Global Markets Exchange Group")] GEMX = 307,
        [Description("International Securities Exchange")] XISX = 308,
        [Description("Nasdaq MRX Options Exchange")] MCRY = 309,
        [Description("MIAX International Securities Exchange")] XMIO = 312,
        [Description("NYSE Arca Options")] ARCO = 313,
        [Description("Options Price Reporting Authority")] OPRA = 314,
        [Description("MIAX Pearl Options")] MPRL = 315,
        [Description("Nasdaq Options Market")] XNDQ = 316,
        [Description("Nasdaq BX - Options")] XBXO = 319,
        [Description("Cboe C2 Options Exchange")] C2OX = 322,
        [Description("Nasdaq Philadelphia Exchange Options")] XPHO = 323,
        [Description("Cboe BZX Options Exchange")] BATO = 325
    }

    public enum TradeReportingFacility
    {
        [Description("FINRA/Nasdaq TRF Carteret NJ")] FINRA_NASDAQ_NJ = 201,
        [Description("FINRA/Nasdaq TRF Chicgo IL")] FINRA_NASDAQ_IL = 202,
        [Description("FINRA/NYSE TRF")] FINRA_NYSE = 203
    }
}
