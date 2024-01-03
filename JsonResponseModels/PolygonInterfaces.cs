using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PolygonApiClient
{
    public interface IPolygonTrade
    {
        int[] Trade_Conditions { get; set; }

        int Exchange { get; set; }

        double Price { get; set; }

        int TRF_ID { get; set; }

        Int64 Size { get; set; }
    }
    public interface IPolygonQuote
    {
        int Ask_Exchange { get; set; }

        double Ask_Price { get; set; }

        Int64 Ask_Size { get; set; }

        int Bid_Exchange { get; set; }

        double Bid_Price { get; set; }

        Int64 Bid_Size { get; set; }
    }
    public interface IPolygonOptionContract
    {
        string Symbol { get; set; }
        string Contract_Type { get; set; }
        string Exercise_Style { get; set; }
        string Expiration_Date { get; set; }
        int Shares_Per_Contract { get; set; }
        double Strike_Price { get; set; }        
    }
}
