using System;

namespace PolygonApiClient.ExtendedClient
{
    public class ExpiredOption : Option
    {
        protected new RestOptionsContract_Result optionData { get; }

        public override int Open_Interest => 0;
        public override double Strike => optionData.Strike_Price;
        public override int Shares_Per_Contract => optionData.Shares_Per_Contract;

        public ExpiredOption(RestOptionsContract_Result optionData, Security underlyingSecurity) : base(optionData.Symbol, underlyingSecurity)
        {
            this.optionData = optionData;
            Expiry = DateTime.Parse(this.optionData.Expiration_Date);
            OptionType = (OptionType)Enum.Parse(typeof(OptionType), this.optionData.Contract_Type);
        }
    }

}
