﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace PolygonApiClient.ExtendedClient
{
    public class Stock : Security, IHasOptions
    {
        public OptionChain OptionChain { get; }

        public Stock(string symbol, ISecurityDataProvider dataProvider) : base(symbol, dataProvider)
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

        public override async Task<double> LastCalculationValueAsync(DateTime? asOf = null)
        {
            if (asOf == null)
                return (await LatestQuoteAsync()).MidpointPrice;
            else
                return (await LatestQuoteAsync(asOf.Value)).MidpointPrice;
        }
    }

}
