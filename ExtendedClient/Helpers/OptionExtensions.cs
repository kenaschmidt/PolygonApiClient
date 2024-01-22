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

namespace PolygonApiClient.ExtendedClient
{
    public static class OptionExtensions
    {
        /// <summary>
        /// Calculates the whole/fraction time between now and expiry, based on EST and a 1600 cut off.
        /// </summary>
        /// <param name="me"></param>
        /// <returns></returns>
        public static double CalendarDaysToExpiry(this Option me, DateTime asOf)
        {
            double wholeDays = 0.0;
            double fractionDay = 0.0;

            if (me.Expiry.Date > asOf.Date)
            {
                wholeDays = (me.Expiry.Date - asOf.Date).Days;
            }

            // Fractions of a day (390 minutes)
            fractionDay = TimeHelpers.FractionOfTradingDayRemaining(asOf);

            return (wholeDays + fractionDay);
        }

        public async static Task<double> IV_Async(this Option me, DateTime asOf)
        {
            var underlyingLastQuote = await me.UnderlyingStock.LastQuoteAsync(asOf);
            var optionLastQuote = await me.LastQuoteAsync(asOf);

            return OptionMath.ImpliedVolatility(
                me.OptionType,
                (underlyingLastQuote).MidpointPrice,
                me.Strike,
                me.CalendarDaysToExpiry(asOf),
                OptionMath.RiskFreeRate_SOFR,
                (optionLastQuote).MidpointPrice);
        }
        public async static Task<double> IV_Async(this Option me, DateTime asOf, double underlyingPrice)
        {
            var optionLastQuote = me.LastQuoteAsync(asOf);

            return OptionMath.ImpliedVolatility(
                me.OptionType,
                underlyingPrice,
                me.Strike,
                me.CalendarDaysToExpiry(asOf),
                OptionMath.RiskFreeRate_SOFR,
                (await optionLastQuote).MidpointPrice);
        }
        public static double IV(this Option me, DateTime asOf, double underlyingPrice, double optionPrice)
        {
            return OptionMath.ImpliedVolatility(
                me.OptionType,
                underlyingPrice,
                me.Strike,
                me.CalendarDaysToExpiry(asOf),
                OptionMath.RiskFreeRate_SOFR,
                optionPrice);
        }

        public async static Task<Greeks> Greeks(this Option me, DateTime asOf)
        {
            var underlyingLastQuote = (await me.UnderlyingStock.LastQuoteAsync(asOf)).MidpointPrice;

            var optionLastQuote = (await me.LastQuoteAsync(asOf)).MidpointPrice;

            return new Greeks(
                asOf,
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
            var underlyingLastQuote = (await me.UnderlyingStock.LastQuoteAsync(asOf)).MidpointPrice;

            var iv = await me.IV_Async(asOf, underlyingLastQuote);

            var ret = OptionMath.SpotDelta(
                underlyingLastQuote,
                me.OptionType,
                me.Strike,
                me.CalendarDaysToExpiry(asOf),
                iv,
                OptionMath.RiskFreeRate_SOFR,
                0.0);

            return ret;
        }
        public async static Task<double> Gamma_Async(this Option me, DateTime asOf)
        {
            var underlyingLastQuote = (await me.UnderlyingStock.LastQuoteAsync(asOf)).MidpointPrice;

            var iv = await me.IV_Async(asOf, underlyingLastQuote);

            var ret = OptionMath.SpotGamma(
                underlyingLastQuote,
                me.Strike,
                me.CalendarDaysToExpiry(asOf),
                iv,
                OptionMath.RiskFreeRate_SOFR,
                0.0);

            return ret;
        }
        public async static Task<double> Theta_Async(this Option me, DateTime asOf)
        {
            var underlyingLastQuote = (await me.UnderlyingStock.LastQuoteAsync(asOf)).MidpointPrice;

            var iv = await me.IV_Async(asOf, underlyingLastQuote);

            var ret = OptionMath.SpotTheta(
                underlyingLastQuote,
                me.OptionType,
                me.Strike,
                me.CalendarDaysToExpiry(asOf),
                iv,
                OptionMath.RiskFreeRate_SOFR,
                0.0);

            return ret;
        }
        public async static Task<double> Vega_Async(this Option me, DateTime asOf)
        {
            var underlyingLastQuote = (await me.UnderlyingStock.LastQuoteAsync(asOf)).MidpointPrice;

            var iv = await me.IV_Async(asOf, underlyingLastQuote);

            var ret = OptionMath.SpotVega(
                underlyingLastQuote,
                me.Strike,
                me.CalendarDaysToExpiry(asOf),
                iv,
                OptionMath.RiskFreeRate_SOFR,
                0.0);

            return ret;
        }
        public async static Task<double> Vanna_Async(this Option me, DateTime asOf)
        {
            var underlyingLastQuote = (await me.UnderlyingStock.LastQuoteAsync(asOf)).MidpointPrice;

            var iv = await me.IV_Async(asOf, underlyingLastQuote);

            var ret = OptionMath.Vanna_DeltaVolSensitivity(
                underlyingLastQuote,
                me.OptionType,
                me.Strike,
                me.CalendarDaysToExpiry(asOf),
                iv,
                OptionMath.RiskFreeRate_SOFR,
                0.0);

            return ret;
        }
        public static async Task<double> Charm_Async(this Option me, DateTime asOf)
        {
            var underlyingLastQuote = (await me.UnderlyingStock.LastQuoteAsync(asOf)).MidpointPrice;

            var iv = await me.IV_Async(asOf, underlyingLastQuote);

            var ret = OptionMath.Charm_DeltaTimeDecay(
                underlyingLastQuote,
                me.OptionType,
                me.Strike,
                me.CalendarDaysToExpiry(asOf),
                iv,
                OptionMath.RiskFreeRate_SOFR,
                0.0);

            return ret;
        }
        public async static Task<double> Lambda_Async(this Option me, DateTime asOf)
        {
            var underlyingLastQuote = (await me.UnderlyingStock.LastQuoteAsync(asOf)).MidpointPrice;

            var optionLastQuote = (await me.LastQuoteAsync(asOf)).MidpointPrice;

            var iv = me.IV(asOf, underlyingLastQuote, optionLastQuote);

            var ret = OptionMath.Lambda_Elasticity(
                underlyingLastQuote,
                optionLastQuote,
                me.OptionType,
                me.Strike,
                me.CalendarDaysToExpiry(asOf),
                iv,
                OptionMath.RiskFreeRate_SOFR,
                0.0);

            return ret;
        }
        public async static Task<double> Vomma_Async(this Option me, DateTime asOf)
        {
            var underlyingLastQuote = (await me.UnderlyingStock.LastQuoteAsync(asOf)).MidpointPrice;

            var iv = await me.IV_Async(asOf, underlyingLastQuote);

            var ret = OptionMath.Vomma_VegaVolSensitivity(
                underlyingLastQuote,
                me.OptionType,
                me.Strike,
                me.CalendarDaysToExpiry(asOf),
                iv,
                OptionMath.RiskFreeRate_SOFR,
                0.0);

            return ret;
        }
        public async static Task<double> Veta_Async(this Option me, DateTime asOf)
        {
            var underlyingLastQuote = (await me.UnderlyingStock.LastQuoteAsync(asOf)).MidpointPrice;

            var iv = await me.IV_Async(asOf, underlyingLastQuote);

            var ret = OptionMath.Veta_VegaTimeDecay(
                underlyingLastQuote,
                me.OptionType,
                me.Strike,
                me.CalendarDaysToExpiry(asOf),
                iv,
                OptionMath.RiskFreeRate_SOFR,
                0.0);

            return ret;
        }
        public async static Task<double> Speed_Async(this Option me, DateTime asOf)
        {
            var underlyingLastQuote = (await me.UnderlyingStock.LastQuoteAsync(asOf)).MidpointPrice;

            var iv = await me.IV_Async(asOf, underlyingLastQuote);

            var ret = OptionMath.Speed_GammaSensitivityToUnderlying(
                underlyingLastQuote,
                me.OptionType,
                me.Strike,
                me.CalendarDaysToExpiry(asOf),
                iv,
                OptionMath.RiskFreeRate_SOFR,
                0.0);

            return ret;
        }
        public async static Task<double> Zomma_Async(this Option me, DateTime asOf)
        {
            var underlyingLastQuote = (await me.UnderlyingStock.LastQuoteAsync(asOf)).MidpointPrice;

            var iv = await me.IV_Async(asOf, underlyingLastQuote);

            var ret = OptionMath.Zomma_GammaVolSensitivity(
                underlyingLastQuote,
                me.OptionType,
                me.Strike,
                me.CalendarDaysToExpiry(asOf),
                iv,
                OptionMath.RiskFreeRate_SOFR,
                0.0);

            return ret;
        }
        public async static Task<double> Color_Async(this Option me, DateTime asOf)
        {
            var underlyingLastQuote = (await me.UnderlyingStock.LastQuoteAsync(asOf)).MidpointPrice;

            var iv = await me.IV_Async(asOf, underlyingLastQuote);

            var ret = OptionMath.Color_GammaTimeDecay(
                underlyingLastQuote,
                me.OptionType,
                me.Strike,
                me.CalendarDaysToExpiry(asOf),
                iv,
                OptionMath.RiskFreeRate_SOFR,
                0.0);

            return ret;
        }

        //
        // Synchronous methods with parameters provided
        //

        public static double Delta(this Option me, DateTime asOf, double underlyingPrice, double optionPrice)
        {
            var iv = me.IV(asOf, underlyingPrice, optionPrice);

            var ret = OptionMath.SpotDelta(
                underlyingPrice,
                me.OptionType,
                me.Strike,
                me.CalendarDaysToExpiry(asOf),
                iv,
                OptionMath.RiskFreeRate_SOFR,
                0.0);

            return ret;
        }
        public static double Gamma(this Option me, DateTime asOf, double underlyingPrice, double optionPrice)
        {
            var iv = me.IV(asOf, underlyingPrice, optionPrice);

            var ret = OptionMath.SpotGamma(
                underlyingPrice,
                me.Strike,
                me.CalendarDaysToExpiry(asOf),
                iv,
                OptionMath.RiskFreeRate_SOFR,
                0.0);

            return ret;
        }
        public static double Theta(this Option me, DateTime asOf, double underlyingPrice, double optionPrice)
        {
            var iv = me.IV(asOf, underlyingPrice, optionPrice);

            var ret = OptionMath.SpotTheta(
                underlyingPrice,
                me.OptionType,
                me.Strike,
                me.CalendarDaysToExpiry(asOf),
                iv,
                OptionMath.RiskFreeRate_SOFR,
                0.0);

            return ret;
        }
        public static double Vega(this Option me, DateTime asOf, double underlyingPrice, double optionPrice)
        {
            var iv = me.IV(asOf, underlyingPrice, optionPrice);

            var ret = OptionMath.SpotVega(
                underlyingPrice,
                me.Strike,
                me.CalendarDaysToExpiry(asOf),
                iv,
                OptionMath.RiskFreeRate_SOFR,
                0.0);

            return ret;
        }
        public static double Vanna(this Option me, DateTime asOf, double underlyingPrice, double optionPrice)
        {
            var iv = me.IV(asOf, underlyingPrice, optionPrice);

            var ret = OptionMath.Vanna_DeltaVolSensitivity(
                underlyingPrice,
                me.OptionType,
                me.Strike,
                me.CalendarDaysToExpiry(asOf),
                iv,
                OptionMath.RiskFreeRate_SOFR,
                0.0);

            return ret;
        }
        public static double Charm(this Option me, DateTime asOf, double underlyingPrice, double optionPrice)
        {
            var iv = me.IV(asOf, underlyingPrice, optionPrice);

            var ret = OptionMath.Charm_DeltaTimeDecay(
                underlyingPrice,
                me.OptionType,
                me.Strike,
                me.CalendarDaysToExpiry(asOf),
                iv,
                OptionMath.RiskFreeRate_SOFR,
                0.0);

            return ret;
        }
        public static double Lambda(this Option me, DateTime asOf, double underlyingPrice, double optionPrice)
        {
            var iv = me.IV(asOf, underlyingPrice, optionPrice);

            var ret = OptionMath.Lambda_Elasticity(
                underlyingPrice,
                optionPrice,
                me.OptionType,
                me.Strike,
                me.CalendarDaysToExpiry(asOf),
                iv,
                OptionMath.RiskFreeRate_SOFR,
                0.0);

            return ret;
        }
        public static double Vomma(this Option me, DateTime asOf, double underlyingPrice, double optionPrice)
        {
            var iv = me.IV(asOf, underlyingPrice, optionPrice);

            var ret = OptionMath.Vomma_VegaVolSensitivity(
                underlyingPrice,
                me.OptionType,
                me.Strike,
                me.CalendarDaysToExpiry(asOf),
                iv,
                OptionMath.RiskFreeRate_SOFR,
                0.0);

            return ret;
        }
        public static double Veta(this Option me, DateTime asOf, double underlyingPrice, double optionPrice)
        {
            var iv = me.IV(asOf, underlyingPrice, optionPrice);

            var ret = OptionMath.Veta_VegaTimeDecay(
                underlyingPrice,
                me.OptionType,
                me.Strike,
                me.CalendarDaysToExpiry(asOf),
                iv,
                OptionMath.RiskFreeRate_SOFR,
                0.0);

            return ret;
        }
        public static double Speed(this Option me, DateTime asOf, double underlyingPrice, double optionPrice)
        {
            var iv = me.IV(asOf, underlyingPrice, optionPrice);

            var ret = OptionMath.Speed_GammaSensitivityToUnderlying(
                underlyingPrice,
                me.OptionType,
                me.Strike,
                me.CalendarDaysToExpiry(asOf),
                iv,
                OptionMath.RiskFreeRate_SOFR,
                0.0);

            return ret;
        }
        public static double Zomma(this Option me, DateTime asOf, double underlyingPrice, double optionPrice)
        {
            var iv = me.IV(asOf, underlyingPrice, optionPrice);

            var ret = OptionMath.Zomma_GammaVolSensitivity(
                underlyingPrice,
                me.OptionType,
                me.Strike,
                me.CalendarDaysToExpiry(asOf),
                iv,
                OptionMath.RiskFreeRate_SOFR,
                0.0);

            return ret;
        }
        public static double Color(this Option me, DateTime asOf, double underlyingPrice, double optionPrice)
        {
            var iv = me.IV(asOf, underlyingPrice, optionPrice);

            var ret = OptionMath.Color_GammaTimeDecay(
                underlyingPrice,
                me.OptionType,
                me.Strike,
                me.CalendarDaysToExpiry(asOf),
                iv,
                OptionMath.RiskFreeRate_SOFR,
                0.0);

            return ret;
        }


        public async static Task<List<(double price, double delta)>> DeltaCurve_Async(this Option me, DateTime asOf, double lowPrice, double highPrice, double step = 0.01)
        {
            var iv = me.IV_Async(asOf);

            var ret = new List<(double price, double delta)>();

            double dte = me.CalendarDaysToExpiry(asOf);

            // Calculate a Delta value for each percentage step 
            for (double p = lowPrice; p <= highPrice; p += step)
            {
                var d = OptionMath.SpotDelta(
                    p,
                    me.OptionType,
                    me.Strike,
                    dte,
                    await iv,
                    OptionMath.RiskFreeRate_SOFR,
                    0.0);

                ret.Add((p, d));
            }

            return ret;
        }
        public async static Task<List<(double price, double gamma)>> GammaCurve_Async(this Option me, DateTime asOf, double lowPrice, double highPrice, double step = 0.01)
        {

            var iv = me.IV_Async(asOf);

            var ret = new List<(double price, double gamma)>();

            double dte = me.CalendarDaysToExpiry(asOf);

            // Calculate a Delta value for each percentage step 
            for (double p = lowPrice; p <= highPrice; p += step)
            {
                double g = OptionMath.SpotGamma(
                    p,
                    me.Strike,
                    dte,
                    await iv,
                    OptionMath.RiskFreeRate_SOFR,
                    0.0);

                ret.Add((p, g));
            }

            return ret;
        }
        public async static Task<List<(double price, double theta)>> ThetaCurve_Async(this Option me, DateTime asOf, double lowPrice, double highPrice, double step = 0.01)
        {
            var iv = me.IV_Async(asOf);

            var ret = new List<(double price, double theta)>();

            double dte = me.CalendarDaysToExpiry(asOf);

            // Calculate a Delta value for each percentage step 
            for (double p = lowPrice; p <= highPrice; p += step)
            {
                var d = OptionMath.SpotTheta(
                    p,
                    me.OptionType,
                    me.Strike,
                    dte,
                    await iv,
                    OptionMath.RiskFreeRate_SOFR,
                    0.0);

                ret.Add((p, d));
            }

            return ret;
        }
        public async static Task<List<(double price, double vega)>> VegaCurve_Async(this Option me, DateTime asOf, double lowPrice, double highPrice, double step = 0.01)
        {
            var iv = me.IV_Async(asOf);

            var ret = new List<(double price, double vega)>();

            double dte = me.CalendarDaysToExpiry(asOf);

            // Calculate a Delta value for each percentage step 
            for (double p = lowPrice; p <= highPrice; p += step)
            {
                var d = OptionMath.SpotVega(
                    p,
                    me.Strike,
                    dte,
                    await iv,
                    OptionMath.RiskFreeRate_SOFR,
                    0.0);

                ret.Add((p, d));
            }

            return ret;
        }
        public async static Task<List<(double price, double vanna)>> VannaCurve_Async(this Option me, DateTime asOf, double lowPrice, double highPrice, double step = 0.01)
        {
            var iv = me.IV_Async(asOf);

            var ret = new List<(double price, double vanna)>();

            double dte = me.CalendarDaysToExpiry(asOf);

            // Calculate a Delta value for each percentage step 
            for (double p = lowPrice; p <= highPrice; p += step)
            {
                var d = OptionMath.Vanna_DeltaVolSensitivity(
                    p,
                    me.OptionType,
                    me.Strike,
                    dte, await iv,
                    OptionMath.RiskFreeRate_SOFR,
                    0.0);

                ret.Add((p, d));
            }

            return ret;
        }
        public async static Task<List<(double price, double charm)>> CharmCurve_Async(this Option me, DateTime asOf, double lowPrice, double highPrice, double step = 0.01)
        {
            var iv = me.IV_Async(asOf);

            var ret = new List<(double price, double charm)>();

            double dte = me.CalendarDaysToExpiry(asOf);

            // Calculate a Delta value for each percentage step 
            for (double p = lowPrice; p <= highPrice; p += step)
            {
                var d = OptionMath.Charm_DeltaTimeDecay(
                    p,
                    me.OptionType,
                    me.Strike,
                    dte,
                    await iv,
                    OptionMath.RiskFreeRate_SOFR,
                    0.0);

                ret.Add((p, d));
            }

            return ret;
        }
        public async static Task<List<(double price, double lambda)>> LambdaCurve_Async(this Option me, DateTime asOf, double lowPrice, double highPrice, double step = 0.01)
        {
            var underlyingLastQuote = (await me.UnderlyingStock.LastQuoteAsync(asOf)).MidpointPrice;

            var iv = await me.IV_Async(asOf, underlyingLastQuote);

            var ret = new List<(double price, double lambda)>();

            double dte = me.CalendarDaysToExpiry(asOf);

            // Calculate a Delta value for each percentage step 
            for (double p = lowPrice; p <= highPrice; p += step)
            {
                var optPrice = OptionMath.AmericanOptionPrice(me.OptionType, p, me.Strike, dte, OptionMath.RiskFreeRate_SOFR, iv);

                var d = OptionMath.Lambda_Elasticity(p, optPrice, me.OptionType, me.Strike, dte, iv, OptionMath.RiskFreeRate_SOFR, 0.0);
                ret.Add((p, d));
            }

            return ret;
        }
        public async static Task<List<(double price, double vomma)>> VommaCurve_Async(this Option me, DateTime asOf, double lowPrice, double highPrice, double step = 0.01)
        {
            var iv = me.IV_Async(asOf);

            var ret = new List<(double price, double vomma)>();

            double dte = me.CalendarDaysToExpiry(asOf);

            // Calculate a Delta value for each percentage step 
            for (double p = lowPrice; p <= highPrice; p += step)
            {
                var d = OptionMath.Vomma_VegaVolSensitivity(
                    p,
                    me.OptionType,
                    me.Strike,
                    dte, await iv,
                    OptionMath.RiskFreeRate_SOFR,
                    0.0);

                ret.Add((p, d));
            }

            return ret;
        }
        public async static Task<List<(double price, double veta)>> VetaCurve_Async(this Option me, DateTime asOf, double lowPrice, double highPrice, double step = 0.01)
        {
            var iv = me.IV_Async(asOf);

            var ret = new List<(double price, double veta)>();

            double dte = me.CalendarDaysToExpiry(asOf);

            // Calculate a Delta value for each percentage step 
            for (double p = lowPrice; p <= highPrice; p += step)
            {
                var d = OptionMath.Veta_VegaTimeDecay(
                    p,
                    me.OptionType,
                    me.Strike,
                    dte, await iv,
                    OptionMath.RiskFreeRate_SOFR,
                    0.0);

                ret.Add((p, d));
            }

            return ret;
        }
        public async static Task<List<(double price, double speed)>> SpeedCurve_Async(this Option me, DateTime asOf, double lowPrice, double highPrice, double step = 0.01)
        {
            var iv = me.IV_Async(asOf);

            var ret = new List<(double price, double speed)>();

            double dte = me.CalendarDaysToExpiry(asOf);

            // Calculate a Delta value for each percentage step 
            for (double p = lowPrice; p <= highPrice; p += step)
            {
                var d = OptionMath.Speed_GammaSensitivityToUnderlying(
                    p,
                    me.OptionType,
                    me.Strike,
                    dte, await iv,
                    OptionMath.RiskFreeRate_SOFR,
                    0.0);

                ret.Add((p, d));
            }

            return ret;
        }
        public async static Task<List<(double price, double zomma)>> ZommaCurve_Async(this Option me, DateTime asOf, double lowPrice, double highPrice, double step = 0.01)
        {
            var iv = me.IV_Async(asOf);

            var ret = new List<(double price, double zomma)>();

            double dte = me.CalendarDaysToExpiry(asOf);

            // Calculate a Delta value for each percentage step 
            for (double p = lowPrice; p <= highPrice; p += step)
            {
                var d = OptionMath.Zomma_GammaVolSensitivity(
                    p,
                    me.OptionType,
                    me.Strike,
                    dte, await iv,
                    OptionMath.RiskFreeRate_SOFR,
                    0.0);

                ret.Add((p, d));
            }

            return ret;
        }
        public async static Task<List<(double price, double color)>> ColorCurve_Async(this Option me, DateTime asOf, double lowPrice, double highPrice, double step = 0.01)
        {
            var iv = me.IV_Async(asOf);

            var ret = new List<(double price, double color)>();

            double dte = me.CalendarDaysToExpiry(asOf);

            // Calculate a Delta value for each percentage step 
            for (double p = lowPrice; p <= highPrice; p += step)
            {
                var d = OptionMath.Color_GammaTimeDecay(
                    p,
                    me.OptionType,
                    me.Strike,
                    dte, await iv,
                    OptionMath.RiskFreeRate_SOFR,
                    0.0);

                ret.Add((p, d));
            }

            return ret;
        }
    }
}
