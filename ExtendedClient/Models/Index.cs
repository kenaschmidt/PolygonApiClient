using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PolygonApiClient.ExtendedClient
{
    public partial class Index : Security, IHasOptions
    {
        public OptionChain OptionChain { get; }

        public Index(string symbol, ISecurityDataProvider dataProvider) : base(symbol, dataProvider)
        {
            OptionChain = new OptionChain(this);
        }

        public void AddOptions(List<IPolygonOptionData> options)
        {
            var newOptions = options.ConvertAll<Option>
                (x => { return new Option(x, this); });

            OptionChain.AddOptions(newOptions);
        }
        public void AddOptions(IPolygonOptionData[] options)
        {
            var newOptions = Array.ConvertAll<IPolygonOptionData, Option>(options, x => { return new Option(x, this); });

            OptionChain.AddOptions(newOptions);
        }
        public void AddOptions(RestOptionsContract_Result[] options)
        {
            var newOptions = Array.ConvertAll<RestOptionsContract_Result, ExpiredOption>(options, x => { return new ExpiredOption(x, this); });

            OptionChain.AddOptions(newOptions);
        }

        public async Task LoadOptions()
        {
            await dataProvider.Load_Options_Chain_Async(this);
        }
        public async Task LoadOptions(DateTime since, bool expiredOnly = true)
        {
            await dataProvider.Load_Options_Chain_Expired_Async(this, since);

            if (!expiredOnly)
                await LoadOptions();
        }
        public async Task LoadOptionExpiry(DateTime expiry)
        {
            await dataProvider.Load_Options_Expiry_Async(this, expiry);
        }
    }

    /// <summary>
    /// SOCKET/Realtime Implementation
    /// </summary>
    public partial class Index
    {
        public event EventHandler<double> ValueReceived;
        private void OnValueReceived(double value)
        {
            ValueReceived?.Invoke(this, value);
        }
        public bool ValueStreaming => SocketHandler?.ValueStreaming ?? false;

        public double? LastValue { get; private set; }

        protected void HandleSocketValue(object sender, Socket_Value e)
        {
            LastValue = e.Value;
            OnValueReceived(e.Value);
        }

        public async Task StreamValues()
        {
            if (ValueStreaming)
                return;

            Console.WriteLine($"START VALUE STREAM FOR {this.Symbol}");

            AttachSocketHandler(await dataProvider.Stream_Values(this, true));
        }
        public async Task StopValues()
        {
            if (!ValueStreaming)
                return;

            AttachSocketHandler(await dataProvider.Stream_Values(this, false));
        }

        public override async Task<double> LastCalculationValueAsync(DateTime? asOf = null)
        {
            if (asOf == null)
                return await LatestValueAsync();
            else
            {
                // Doesn't have historical yet
                return await LatestValueAsync();
            }
        }
        internal async Task<double> LatestValueAsync()
        {
            if (LastValue == null)
                LastValue = await dataProvider.Value_Async(this);

            return LastValue.Value;
        }
    }

}
