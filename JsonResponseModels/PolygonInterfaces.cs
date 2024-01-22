using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PolygonApiClient
{
    //
    // These interfaces are meant to facilitate interoperability of different REST/Socket response formats in extended client implementations.
    //

    public interface IPolygonTrade
    {
        int[] Trade_Conditions { get; set; }

        int Exchange { get; set; }

        double Price { get; set; }

        int TRF_ID { get; set; }

        long Size { get; set; }
    }

    public interface IPolygonQuote
    {
        int Ask_Exchange { get; set; }

        double Ask_Price { get; set; }

        long Ask_Size { get; set; }

        int Bid_Exchange { get; set; }

        double Bid_Price { get; set; }

        long Bid_Size { get; set; }
    }

    public interface IPolygonBar
    {
        long Volume { get; set; }

        double VWAP { get; set; }

        double Open { get; set; }

        double Close { get; set; }

        double High { get; set; }

        double Low { get; set; }

        long Timestamp_Start_Ms { get; set; }
    }

    public interface IPolygonOptionData
    {
        double Break_Even_Price { get; set; }

        int Open_Interest { get; set; }

        double Implied_Volatility { get; set; }

        RestOptionContract_Day Day { get; set; }

        RestOptionContract_Details Details { get; set; }

        RestOptionContract_Greeks Greeks { get; set; }

        RestOptionContract_LastQuote Last_Quote { get; set; }

        RestOptionContract_UnderlyingAsset Underlying_Asset { get; set; }
    }
}
