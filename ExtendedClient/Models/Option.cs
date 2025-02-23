using PolygonApiClient.ExtendedClient.Models;
using System;
using System.Threading.Tasks;

namespace PolygonApiClient.ExtendedClient
{
    public class Option : Security
    {

        public event EventHandler<Greeks> GreeksChanged;
        private void OnGreeksChanged()
        {
            GreeksChanged?.Invoke(this, this.Greeks);
        }

        public IPolygonOptionData optionData { get; }


        public Security UnderlyingSecurity { get; }
        public OptionExpiry ExpiryCurve { get; set; }


        public DateTime Expiry { get; protected set; }
        public OptionType OptionType { get; protected set; }


        private Greeks _Greeks { get; set; }
        public Greeks Greeks
        {
            get => _Greeks;
            set
            {
                _Greeks = value;
                OnGreeksChanged();
            }
        }

        public virtual double Strike => optionData.Details.Strike_Price;
        public virtual int Shares_Per_Contract => optionData.Details.Shares_Per_Contract;
        public virtual int Open_Interest => optionData.Open_Interest;


        public Option(IPolygonOptionData optionData, Security underlyingSecurity) : base(optionData.Details.Symbol, underlyingSecurity.dataProvider)
        {
            this.optionData = optionData;
            if (optionData != null)
            {
                Expiry = DateTime.Parse(this.optionData.Details.Expiration_Date);
                OptionType = (OptionType)Enum.Parse(typeof(OptionType), this.optionData.Details.Contract_Type);
            }
            UnderlyingSecurity = underlyingSecurity;
        }
        public Option(string symbol, Security underlyingSecurity) : base(symbol, underlyingSecurity.dataProvider)
        {
            UnderlyingSecurity = underlyingSecurity;
        }


        private RestSnapshotHandler snapshotHandler_greeks { get; set; }
        public void StreamGreeksSnapshots(int secondsInterval = 5)
        {
            if (snapshotHandler_greeks == null)
                snapshotHandler_greeks = dataProvider.Stream_Greeks_Snapshots(this, secondsInterval);
        }
        public void StopGreeksSnapshots()
        {
            if (snapshotHandler_greeks != null)
            {
                snapshotHandler_greeks.Stop();
                snapshotHandler_greeks = null;
            }
        }

        public override async Task<Quote> LatestQuoteAsync(DateTime asOf)
        {
            // For options we may get erroneous 0 values outside of trading hours so we need to adjust...might be a better way to do this

            if (asOf.TimeOfDay < TimeHelpers.MarketRTHOpenEST)
                asOf = TradingCalendar.Calendar.PriorTradingDay(asOf).AtMarketRTHCloseEST();

            return await dataProvider.Quote_Async(this, asOf);
        }

        public override async Task<double> LastCalculationValueAsync(DateTime? asOf = null)
        {
            if (asOf == null)
                return (await LatestQuoteAsync()).MidpointPrice;
            else
                return (await LatestQuoteAsync(asOf.Value)).MidpointPrice;
        }
    }
}
