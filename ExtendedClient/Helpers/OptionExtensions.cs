using PolygonApiClient.ExtendedClient.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using OptionMath;
using static OptionMath.OptionPricing;
using System.IO;

namespace PolygonApiClient.ExtendedClient
{
    public static class OptionExtensions
    {

        private static OptionMath.OptionType ToOptionMathOptionType(this PolygonApiClient.OptionType me)
        {
            return (OptionMath.OptionType)((int)me);
        }

        public static double RiskFreeRate = 0.0426;

        /// <summary>
        /// Returns the option Time To Expiration as a fraction of a year
        /// </summary>
        /// <param name="me"></param>
        /// <returns></returns>
        public static double TimeToExpiration(this Option me, DateTime asOf)
        {
            return TradingCalendar.Calendar.OptionYearsToExpiration(asOf, me.Expiry);
        }

        public async static Task<double> IV_Async(this Option me, DateTime asOf)
        {
            var underlyingLastPrice = await me.UnderlyingSecurity.LastCalculationValueAsync(asOf);
            var optionLastQuote = await me.LatestQuoteAsync(asOf);

            return ImpliedVolatility(
                 me.OptionType.ToOptionMathOptionType(),
                underlyingLastPrice,
                me.Strike,
                me.TimeToExpiration(asOf),
                RiskFreeRate,
                (optionLastQuote).MidpointPrice);
        }
        public async static Task<double> IV_Async(this Option me, DateTime asOf, double underlyingPrice)
        {
            var optionLastQuote = me.LatestQuoteAsync(asOf);

            return ImpliedVolatility(
                me.OptionType.ToOptionMathOptionType(),
                underlyingPrice,
                me.Strike,
                me.TimeToExpiration(asOf),
                RiskFreeRate,
                (await optionLastQuote).MidpointPrice);
        }
        public static double IV(this Option me, DateTime asOf, double underlyingPrice, double optionPrice)
        {
            return ImpliedVolatility(
                me.OptionType.ToOptionMathOptionType(),
                underlyingPrice,
                me.Strike,
                me.TimeToExpiration(asOf),
                RiskFreeRate,
                optionPrice);
        }

        public async static Task<Greeks> Greeks_Async(this Option me, DateTime asOf)
        {
            var underlyingLastQuote = (await me.UnderlyingSecurity.LastCalculationValueAsync(asOf));

            var optionLastQuote = (await me.LatestQuoteAsync(asOf)).MidpointPrice;

            var iv = me.IV(asOf, underlyingLastQuote, optionLastQuote);

            return new Greeks(
                asOf,
                iv,
                Delta(me, asOf, underlyingLastQuote, optionLastQuote),
                Gamma(me, asOf, underlyingLastQuote, optionLastQuote),
                Theta(me, asOf, underlyingLastQuote, optionLastQuote),
                Vega(me, asOf, underlyingLastQuote, optionLastQuote),
                Vanna(me, asOf, underlyingLastQuote, optionLastQuote),
                Veta(me, asOf, underlyingLastQuote, optionLastQuote),
                Charm(me, asOf, underlyingLastQuote, optionLastQuote),
                Vomma(me, asOf, underlyingLastQuote, optionLastQuote),
                Zomma(me, asOf, underlyingLastQuote, optionLastQuote),
                Speed(me, asOf, underlyingLastQuote, optionLastQuote),
                Color(me, asOf, underlyingLastQuote, optionLastQuote),
                Lambda(me, asOf, underlyingLastQuote, optionLastQuote));
        }


        public static Greeks Greeks(this Option me, DateTime asOf, double optionPrice, double spotPrice)
        {
            var underlyingLastQuote = spotPrice;

            var optionLastQuote = optionPrice;

            var iv = me.IV(asOf, underlyingLastQuote, optionLastQuote);

            return new Greeks(
                asOf,
                iv,
                Delta(me, asOf, underlyingLastQuote, optionLastQuote),
                Gamma(me, asOf, underlyingLastQuote, optionLastQuote),
                Theta(me, asOf, underlyingLastQuote, optionLastQuote),
                Vega(me, asOf, underlyingLastQuote, optionLastQuote),
                Vanna(me, asOf, underlyingLastQuote, optionLastQuote),
                Veta(me, asOf, underlyingLastQuote, optionLastQuote),
                Charm(me, asOf, underlyingLastQuote, optionLastQuote),
                Vomma(me, asOf, underlyingLastQuote, optionLastQuote),
                Zomma(me, asOf, underlyingLastQuote, optionLastQuote),
                Speed(me, asOf, underlyingLastQuote, optionLastQuote),
                Color(me, asOf, underlyingLastQuote, optionLastQuote),
                Lambda(me, asOf, underlyingLastQuote, optionLastQuote));
        }

        //
        // Asynchronous methods retrieve pricing data from REST client
        //

        public async static Task<double> Delta_Async(this Option me, DateTime asOf)
        {
            var underlyingLastQuote = (await me.UnderlyingSecurity.LastCalculationValueAsync(asOf));

            var iv = await me.IV_Async(asOf, underlyingLastQuote);

            var ret = OptionPricing.Delta(
                underlyingLastQuote,
                me.OptionType.ToOptionMathOptionType(),
                me.Strike,
                me.TimeToExpiration(asOf),
                iv,
                RiskFreeRate,
                0.0);

            return ret;
        }
        public async static Task<double> Gamma_Async(this Option me, DateTime asOf)
        {
            var underlyingLastQuote = (await me.UnderlyingSecurity.LastCalculationValueAsync(asOf));

            var iv = await me.IV_Async(asOf, underlyingLastQuote);

            var ret = OptionPricing.Gamma(
                underlyingLastQuote,
                me.Strike,
                me.TimeToExpiration(asOf),
                iv,
                RiskFreeRate,
                0.0);

            return ret;
        }
        public async static Task<double> Theta_Async(this Option me, DateTime asOf)
        {
            var underlyingLastQuote = (await me.UnderlyingSecurity.LastCalculationValueAsync(asOf));

            var iv = await me.IV_Async(asOf, underlyingLastQuote);

            var ret = OptionPricing.Theta(
                underlyingLastQuote,
                me.OptionType.ToOptionMathOptionType(),
                me.Strike,
                me.TimeToExpiration(asOf),
                iv,
                RiskFreeRate,
                0.0);

            return ret;
        }
        public async static Task<double> Vega_Async(this Option me, DateTime asOf)
        {
            var underlyingLastQuote = (await me.UnderlyingSecurity.LastCalculationValueAsync(asOf));

            var iv = await me.IV_Async(asOf, underlyingLastQuote);

            var ret = OptionPricing.Vega(
                underlyingLastQuote,
                me.Strike,
                me.TimeToExpiration(asOf),
                iv,
                RiskFreeRate,
                0.0);

            return ret;
        }
        public async static Task<double> Vanna_Async(this Option me, DateTime asOf)
        {
            var underlyingLastQuote = (await me.UnderlyingSecurity.LastCalculationValueAsync(asOf));

            var iv = await me.IV_Async(asOf, underlyingLastQuote);

            var ret = OptionPricing.Vanna(
                underlyingLastQuote,
                me.OptionType.ToOptionMathOptionType(),
                me.Strike,
                me.TimeToExpiration(asOf),
                iv,
                RiskFreeRate,
                0.0);

            return ret;
        }
        public static async Task<double> Charm_Async(this Option me, DateTime asOf)
        {
            var underlyingLastQuote = (await me.UnderlyingSecurity.LastCalculationValueAsync(asOf));

            var iv = await me.IV_Async(asOf, underlyingLastQuote);

            var ret = OptionPricing.Charm(
                underlyingLastQuote,
                me.OptionType.ToOptionMathOptionType(),
                me.Strike,
                me.TimeToExpiration(asOf),
                iv,
                RiskFreeRate,
                0.0);

            return ret;
        }
        public async static Task<double> Lambda_Async(this Option me, DateTime asOf)
        {
            var underlyingLastQuote = (await me.UnderlyingSecurity.LastCalculationValueAsync(asOf));

            var optionLastQuote = (await me.LatestQuoteAsync(asOf)).MidpointPrice;

            var delta = await Delta_Async(me, asOf);

            var ret = OptionPricing.Lambda(
                underlyingLastQuote,
                optionLastQuote,
                delta);

            return ret;
        }
        public async static Task<double> Vomma_Async(this Option me, DateTime asOf)
        {
            var underlyingLastQuote = (await me.UnderlyingSecurity.LastCalculationValueAsync(asOf));

            var iv = await me.IV_Async(asOf, underlyingLastQuote);

            var ret = OptionPricing.Vomma(
                underlyingLastQuote,
                me.Strike,
                me.TimeToExpiration(asOf),
                iv,
                RiskFreeRate,
                0.0);

            return ret;
        }
        public async static Task<double> Veta_Async(this Option me, DateTime asOf)
        {
            var underlyingLastQuote = (await me.UnderlyingSecurity.LastCalculationValueAsync(asOf));

            var iv = await me.IV_Async(asOf, underlyingLastQuote);

            var ret = OptionPricing.Veta(
                underlyingLastQuote,
                me.Strike,
                me.TimeToExpiration(asOf),
                iv,
                RiskFreeRate,
                0.0);

            return ret;
        }
        public async static Task<double> Speed_Async(this Option me, DateTime asOf)
        {
            var underlyingLastQuote = (await me.UnderlyingSecurity.LastCalculationValueAsync(asOf));

            var iv = await me.IV_Async(asOf, underlyingLastQuote);

            var ret = OptionPricing.Speed(
                underlyingLastQuote,
                me.Strike,
                me.TimeToExpiration(asOf),
                iv,
                RiskFreeRate,
                0.0);

            return ret;
        }
        public async static Task<double> Zomma_Async(this Option me, DateTime asOf)
        {
            var underlyingLastQuote = (await me.UnderlyingSecurity.LastCalculationValueAsync(asOf));

            var iv = await me.IV_Async(asOf, underlyingLastQuote);

            var ret = OptionPricing.Zomma(
                underlyingLastQuote,
                me.Strike,
                me.TimeToExpiration(asOf),
                iv,
                RiskFreeRate,
                0.0);

            return ret;
        }
        public async static Task<double> Color_Async(this Option me, DateTime asOf)
        {
            var underlyingLastQuote = (await me.UnderlyingSecurity.LastCalculationValueAsync(asOf));

            var iv = await me.IV_Async(asOf, underlyingLastQuote);

            var ret = OptionPricing.Color(
                underlyingLastQuote,
                me.Strike,
                me.TimeToExpiration(asOf),
                iv,
                RiskFreeRate,
                0.0);

            return ret;
        }

        //
        // Synchronous methods with parameters provided
        //

        public static double Delta(this Option me, DateTime asOf, double underlyingPrice, double optionPrice)
        {
            var iv = me.IV(asOf, underlyingPrice, optionPrice);

            var ret = OptionPricing.Delta(
                underlyingPrice,
                me.OptionType.ToOptionMathOptionType(),
                me.Strike,
                me.TimeToExpiration(asOf),
                iv,
                RiskFreeRate,
                0.0);

            return ret;
        }
        public static double Gamma(this Option me, DateTime asOf, double underlyingPrice, double optionPrice)
        {
            var iv = me.IV(asOf, underlyingPrice, optionPrice);

            var ret = OptionPricing.Gamma(
                underlyingPrice,
                me.Strike,
                me.TimeToExpiration(asOf),
                iv,
                RiskFreeRate,
                0.0);

            return ret;
        }
        public static double Theta(this Option me, DateTime asOf, double underlyingPrice, double optionPrice)
        {
            var iv = me.IV(asOf, underlyingPrice, optionPrice);

            var ret = OptionPricing.Theta(
                underlyingPrice,
                me.OptionType.ToOptionMathOptionType(),
                me.Strike,
                me.TimeToExpiration(asOf),
                iv,
                RiskFreeRate,
                0.0);

            return ret;
        }
        public static double Vega(this Option me, DateTime asOf, double underlyingPrice, double optionPrice)
        {
            var iv = me.IV(asOf, underlyingPrice, optionPrice);

            var ret = OptionPricing.Vega(
                underlyingPrice,
                me.Strike,
                me.TimeToExpiration(asOf),
                iv,
                RiskFreeRate,
                0.0);

            return ret;
        }
        public static double Vanna(this Option me, DateTime asOf, double underlyingPrice, double optionPrice)
        {
            var iv = me.IV(asOf, underlyingPrice, optionPrice);

            var ret = OptionPricing.Vanna(
                underlyingPrice,
                me.OptionType.ToOptionMathOptionType(),
                me.Strike,
                me.TimeToExpiration(asOf),
                iv,
                RiskFreeRate,
                0.0);

            return ret;
        }
        public static double Charm(this Option me, DateTime asOf, double underlyingPrice, double optionPrice)
        {
            var iv = me.IV(asOf, underlyingPrice, optionPrice);

            var ret = OptionPricing.Charm(
                underlyingPrice,
                me.OptionType.ToOptionMathOptionType(),
                me.Strike,
                me.TimeToExpiration(asOf),
                iv,
                RiskFreeRate,
                0.0);

            return ret;
        }
        public static double Lambda(this Option me, DateTime asOf, double underlyingPrice, double optionPrice)
        {

            var delta = me.Delta(asOf, underlyingPrice, optionPrice);

            var ret = OptionPricing.Lambda(underlyingPrice, optionPrice, delta);

            return ret;
        }
        public static double Vomma(this Option me, DateTime asOf, double underlyingPrice, double optionPrice)
        {
            var iv = me.IV(asOf, underlyingPrice, optionPrice);

            var ret = OptionPricing.Vomma(
                underlyingPrice,
                me.Strike,
                me.TimeToExpiration(asOf),
                iv,
                RiskFreeRate,
                0.0);

            return ret;
        }
        public static double Veta(this Option me, DateTime asOf, double underlyingPrice, double optionPrice)
        {
            var iv = me.IV(asOf, underlyingPrice, optionPrice);

            var ret = OptionPricing.Veta(
                underlyingPrice,
                me.Strike,
                me.TimeToExpiration(asOf),
                iv,
                RiskFreeRate,
                0.0);

            return ret;
        }
        public static double Speed(this Option me, DateTime asOf, double underlyingPrice, double optionPrice)
        {
            var iv = me.IV(asOf, underlyingPrice, optionPrice);

            var ret = OptionPricing.Speed(
                underlyingPrice,
                me.Strike,
                me.TimeToExpiration(asOf),
                iv,
                RiskFreeRate,
                0.0);

            return ret;
        }
        public static double Zomma(this Option me, DateTime asOf, double underlyingPrice, double optionPrice)
        {
            var iv = me.IV(asOf, underlyingPrice, optionPrice);

            var ret = OptionPricing.Zomma(
                underlyingPrice,
                me.Strike,
                me.TimeToExpiration(asOf),
                iv,
                RiskFreeRate,
                0.0);

            return ret;
        }
        public static double Color(this Option me, DateTime asOf, double underlyingPrice, double optionPrice)
        {
            var iv = me.IV(asOf, underlyingPrice, optionPrice);

            var ret = OptionPricing.Color(
                underlyingPrice,
                me.Strike,
                me.TimeToExpiration(asOf),
                iv,
                RiskFreeRate,
                0.0);

            return ret;
        }


    }
}
