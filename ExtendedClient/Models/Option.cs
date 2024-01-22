using PolygonApiClient.ExtendedClient.Models;
using System;
using System.Threading.Tasks;

namespace PolygonApiClient.ExtendedClient
{
    public class Option : Security
    {
        public IPolygonOptionData optionData { get; }


        public Stock UnderlyingStock { get; }
        public OptionExpiry ExpiryCurve { get; set; }


        public DateTime Expiry { get; protected set; }
        public OptionType OptionType { get; protected set; }


        public Greeks Greeks { get; set; }


        public virtual double Strike => optionData.Details.Strike_Price;
        public virtual int Shares_Per_Contract => optionData.Details.Shares_Per_Contract;
        public virtual int Open_Interest => optionData.Open_Interest;


        public Option(IPolygonOptionData optionData, Stock underlyingStock) : base(optionData.Details.Symbol, underlyingStock.dataProvider)
        {
            this.optionData = optionData;
            if (optionData != null)
            {
                Expiry = DateTime.Parse(this.optionData.Details.Expiration_Date);
                OptionType = (OptionType)Enum.Parse(typeof(OptionType), this.optionData.Details.Contract_Type);
            }
            UnderlyingStock = underlyingStock;
        }
        public Option(string symbol, Stock underlyingStock) : base(symbol, underlyingStock.dataProvider)
        {
            UnderlyingStock = underlyingStock;
        }


        private RestSnapshotHandler snapshotHandler_greeks { get; set; }
        public void StreamGreeksSnapshots()
        {
            snapshotHandler_greeks = dataProvider.Stream_Quotes_Trades_Snapshots(this);
        }
        public void StopGreeksSnapshots()
        {
            snapshotHandler_greeks.Stop();
            snapshotHandler_greeks = null;
        }
    }
}
