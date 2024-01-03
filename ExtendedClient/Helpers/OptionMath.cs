using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace PolygonApiClient.ExtendedClient
{
    public static partial class OptionsPricing
    {

        public const double CalendarDaysPerYear = 365.0;

        private static double RiskFreeRate_SOFR = 0.045;
        public static double RiskFreeRate(DateTime date)
        {
            return RiskFreeRate_SOFR;
        }

        //public static double RealizedVolatility(List<PriceBar> bars)
        //{
        //    // As defined on page 286

        //    bars = bars.OrderBy(x => x.Start).ToList();

        //    double n = bars.Count;

        //    double t = 1 / CalendarDaysPerYear;

        //    switch (bars[0].BarSize)
        //    {
        //        case BarSize.second:
        //            t /= (TimeSpan.FromDays(1).TotalSeconds / bars[0].BarSizeMultiplier);
        //            break;
        //        case BarSize.minute:
        //            t /= (TimeSpan.FromDays(1).TotalMinutes / bars[0].BarSizeMultiplier);
        //            break;
        //        case BarSize.hour:
        //            t /= (TimeSpan.FromDays(1).TotalHours / bars[0].BarSizeMultiplier);
        //            break;
        //        case BarSize.day:
        //            break;
        //        case BarSize.week:
        //            t = 1.0 / 52.0;
        //            break;
        //        case BarSize.month:
        //            t = 1.0 / 12.0;
        //            break;
        //        case BarSize.quarter:
        //            t = .25;
        //            break;
        //        case BarSize.year:
        //            t = 1.0;
        //            break;
        //        default:
        //            break;
        //    }

        //    Func<double, double, double> u_i = (price, priorPrice) => Math.Log(price / priorPrice);

        //    double sumation_1 = 0; // 1->n (u_i^2)
        //    double sumation_2 = 0; // 1->n (u_i)^2

        //    for (int i = 1; i < n; i++)
        //    {
        //        sumation_1 += Math.Pow(u_i(bars[i].Close, bars[i - 1].Close), 2);
        //        sumation_2 += u_i(bars[i].Close, bars[i - 1].Close);
        //    }

        //    sumation_2 = Math.Pow(sumation_2, 2);

        //    double s = Math.Sqrt((1.0 / (n - 1.0)) * sumation_1 - (1.0 / (n * (n - 1.0)) * sumation_2));

        //    double vol = s / Math.Sqrt(t);

        //    return vol;
        //}

        #region Statistical Functions

        // Cumulative distribution function for the standard normal distribution
        public static double N(double x)
        {
            double a1 = 0.31938153;
            double a2 = -0.356563782;
            double a3 = 1.781477937;
            double a4 = -1.821255978;
            double a5 = 1.330274429;
            double k = 1 / (1 + 0.2316419 * Math.Abs(x));
            double y = 1 - 1 / Math.Sqrt(2 * Math.PI) * Math.Exp(-Math.Pow(x, 2) / 2)
                       * (a1 * k + a2 * Math.Pow(k, 2) + a3 * Math.Pow(k, 3)
                          + a4 * Math.Pow(k, 4) + a5 * Math.Pow(k, 5));
            return x < 0 ? 1 - y : y;
        }

        // Standardized Normal Density Function n(x)
        public static double n(double x)
        {
            // As defined on page 353

            var a = 1 / (Math.Sqrt(2 * Math.PI));
            var b = Math.Exp(-Math.Pow(x, 2) / 2);

            return a * b;
        }

        public static double d1(double underlyingPrice, double optionStrikePrice, double calendarDaysToExpiry, double riskFreeRatePercent, double volatilityPercent, double dividendYieldPercent)
        {
            // As defined on page 295

            double So = underlyingPrice;
            double T = calendarDaysToExpiry / CalendarDaysPerYear;
            double v = volatilityPercent;
            double r = riskFreeRatePercent;
            double q = dividendYieldPercent;
            double K = optionStrikePrice;

            double a1 = Math.Log(So / K);
            double a2 = (r - q + (Math.Pow(v, 2) / 2)) * T;
            double b = v * Math.Sqrt(T);

            var ret = (a1 + a2) / b;

            return ret;
        }

        public static double d2(double underlyingPrice, double optionStrikePrice, double calendarDaysToExpiry, double riskFreeRatePercent, double volatilityPercent, double dividendYieldPercent)
        {
            // As defined on page 295

            double T = calendarDaysToExpiry / CalendarDaysPerYear;
            double v = volatilityPercent;

            var d_2 = d1(underlyingPrice, optionStrikePrice, calendarDaysToExpiry, riskFreeRatePercent, volatilityPercent, dividendYieldPercent) - (v * Math.Sqrt(T));

            return d_2;
        }

        #endregion

        #region First Order Greeks

        public static double SpotDelta(double underlyingPrice, OptionType optionType, double optionStrikePrice, double calendarDaysToExpiry, double volatilityPercent, double riskFreeRatePercent, double dividendYieldPercent)
        {
            double T = calendarDaysToExpiry / CalendarDaysPerYear;
            double q = dividendYieldPercent;

            if (optionType == OptionType.call)
            {
                return N(d1(underlyingPrice, optionStrikePrice, calendarDaysToExpiry, riskFreeRatePercent, volatilityPercent, dividendYieldPercent));
            }
            else if (optionType == OptionType.put)
            {
                return N(d1(underlyingPrice, optionStrikePrice, calendarDaysToExpiry, riskFreeRatePercent, volatilityPercent, dividendYieldPercent)) - 1;
            }
            else
                throw new ArgumentException();
        }
        public static double SpotGamma(double underlyingPrice, double optionStrikePrice, double calendarDaysToExpiry, double volatilityPercent, double riskFreeRatePercent, double dividendYieldPercent)
        {
            var T = calendarDaysToExpiry / CalendarDaysPerYear;

            var a = n(d1(underlyingPrice, optionStrikePrice, calendarDaysToExpiry, riskFreeRatePercent, volatilityPercent, dividendYieldPercent));
            var b = underlyingPrice * volatilityPercent * Math.Sqrt(T);

            Console.WriteLine($"My num: {a:0.0000}  My denom: {b:0.0000}");

            return a / b;
        }
        public static double SpotTheta(double underlyingPrice, OptionType optionType, double optionStrikePrice, double calendarDaysToExpiry, double volatilityPercent, double riskFreeRatePercent, double dividendYieldPercent)
        {
            // As defined on page 353

            double So = underlyingPrice;
            double T = calendarDaysToExpiry / CalendarDaysPerYear;

            if (optionType == OptionType.call)
            {
                double a1 = So * (n(d1(underlyingPrice, optionStrikePrice, calendarDaysToExpiry, riskFreeRatePercent, volatilityPercent, dividendYieldPercent))) * volatilityPercent * Math.Exp(-dividendYieldPercent * T);
                double a2 = 2 * Math.Sqrt(T);

                double c1 = (dividendYieldPercent * So * N(d1(underlyingPrice, optionStrikePrice, calendarDaysToExpiry, riskFreeRatePercent, volatilityPercent, dividendYieldPercent)) * Math.Exp(-dividendYieldPercent * T));

                double b1 = riskFreeRatePercent * optionStrikePrice * Math.Exp(-riskFreeRatePercent * T) * N(d2(underlyingPrice, optionStrikePrice, calendarDaysToExpiry, riskFreeRatePercent, volatilityPercent, dividendYieldPercent));

                double ret = -(a1 / a2) + c1 - b1;
                ret /= CalendarDaysPerYear;

                return ret;
            }
            else if (optionType == OptionType.put)
            {
                double a1 = So * n(d1(underlyingPrice, optionStrikePrice, calendarDaysToExpiry, riskFreeRatePercent, volatilityPercent, dividendYieldPercent)) * volatilityPercent;
                double a2 = 2 * Math.Sqrt(T);

                double c1 = (dividendYieldPercent * So * N(-d1(underlyingPrice, optionStrikePrice, calendarDaysToExpiry, riskFreeRatePercent, volatilityPercent, dividendYieldPercent)) * Math.Exp(-dividendYieldPercent * T));

                double b1 = riskFreeRatePercent * optionStrikePrice * Math.Exp(-riskFreeRatePercent * T) * N(-d2(underlyingPrice, optionStrikePrice, calendarDaysToExpiry, riskFreeRatePercent, volatilityPercent, dividendYieldPercent));

                double ret = -(a1 / a2) - c1 + b1;
                ret /= CalendarDaysPerYear;

                return ret;
            }
            else
                throw new ArgumentException();
        }
        public static double SpotVega(double underlyingPrice, double optionStrikePrice, double calendarDaysToExpiry, double volatilityPercent, double riskFreeRatePercent, double dividendYieldPercent)
        {
            // As defined on page 361

            double So = underlyingPrice;
            double T = calendarDaysToExpiry / CalendarDaysPerYear;
            double v = volatilityPercent;
            double r = riskFreeRatePercent;
            double K = optionStrikePrice;
            double q = dividendYieldPercent;

            double d_1 = d1(underlyingPrice, optionStrikePrice, calendarDaysToExpiry, riskFreeRatePercent, volatilityPercent, dividendYieldPercent);

            //Console.WriteLine($"So: {So}  Sqrt(T): {Math.Sqrt(T)}  d1: {d_1}  N1(d1): {N1(d_1)}");

            return (So * Math.Sqrt(T) * n(d_1)) / 100;

        }

        #endregion

        #region Other Order Greeks

        //
        // Elasticity (leverage): gives the percentage change in an option price per 1% change in the underlying stock price
        //
        public static double Elasticity(double underlyingPrice, double optionPrice, OptionType optionType, double optionStrikePrice, double calendarDaysToExpiry, double volatilityPercent, double riskFreeRatePercent, double dividendYieldPercent)
        {
            return SpotDelta(underlyingPrice, optionType, optionStrikePrice, calendarDaysToExpiry, volatilityPercent, riskFreeRatePercent, dividendYieldPercent) * (underlyingPrice / optionPrice);
        }

        //
        // Vanna: gives the change in Delta for every 1% change in Implied Volatility
        //
        public static double Vanna_DeltaVolSensitivity(double underlyingPrice, OptionType optionType, double optionStrikePrice, double calendarDaysToExpiry, double volatilityPercent, double riskFreeRatePercent, double dividendYieldPercent)
        {
            double delta1 = SpotDelta(underlyingPrice, optionType, optionStrikePrice, calendarDaysToExpiry, volatilityPercent, riskFreeRatePercent, dividendYieldPercent);
            double delta2 = SpotDelta(underlyingPrice, optionType, optionStrikePrice, calendarDaysToExpiry, (volatilityPercent + .01), riskFreeRatePercent, dividendYieldPercent);

            return (delta2 - delta1);
        }

        //
        // Charm: gives the change in Delta per passage of 1 calendar day
        //
        public static double Charm_DeltaTimeDecay(double underlyingPrice, OptionType optionType, double optionStrikePrice, double calendarDaysToExpiry, double volatilityPercent, double riskFreeRatePercent, double dividendYieldPercent)
        {
            double delta1 = SpotDelta(underlyingPrice, optionType, optionStrikePrice, calendarDaysToExpiry, volatilityPercent, riskFreeRatePercent, dividendYieldPercent);
            double delta2 = SpotDelta(underlyingPrice, optionType, optionStrikePrice, calendarDaysToExpiry - 1, volatilityPercent, riskFreeRatePercent, dividendYieldPercent);

            return (delta2 - delta1);
        }

        //
        // Vomma: gives the change in Vega for  change in IV 
        //
        public static double Vomma_VegaSensitivity(double underlyingPrice, OptionType optionType, double optionStrikePrice, double calendarDaysToExpiry, double volatilityPercent, double riskFreeRatePercent, double dividendYieldPercent)
        {
            double vega1 = SpotVega(underlyingPrice, optionStrikePrice, calendarDaysToExpiry, volatilityPercent, riskFreeRatePercent, dividendYieldPercent);
            double vega2 = SpotVega(underlyingPrice, optionStrikePrice, calendarDaysToExpiry, (volatilityPercent + .01), riskFreeRatePercent, dividendYieldPercent);

            return (vega2 - vega1);
        }

        //
        // Veta: gives the change in Vega per passage of 1 calendar day
        //
        public static double Veta_VegaTimeDecay(double underlyingPrice, OptionType optionType, double optionStrikePrice, double calendarDaysToExpiry, double volatilityPercent, double riskFreeRatePercent, double dividendYieldPercent)
        {
            double vega1 = SpotVega(underlyingPrice, optionStrikePrice, calendarDaysToExpiry, volatilityPercent, riskFreeRatePercent, dividendYieldPercent);
            double vega2 = SpotVega(underlyingPrice, optionStrikePrice, (calendarDaysToExpiry - 1), volatilityPercent, riskFreeRatePercent, dividendYieldPercent);

            return (vega2 - vega1);
        }

        //
        // Speed: gives the rate of change for Gamma for a change in the underlying price
        //
        public static double Speed_GammaSensitivityToUnderlying(double underlyingPrice, OptionType optionType, double optionStrikePrice, double calendarDaysToExpiry, double volatilityPercent, double riskFreeRatePercent, double dividendYieldPercent)
        {
            double gamma = SpotGamma(underlyingPrice, optionStrikePrice, calendarDaysToExpiry, volatilityPercent, riskFreeRatePercent, dividendYieldPercent);
            double d_1 = d1(underlyingPrice, optionStrikePrice, calendarDaysToExpiry, riskFreeRatePercent, volatilityPercent, dividendYieldPercent);
            double T = (calendarDaysToExpiry / CalendarDaysPerYear);

            Console.WriteLine(T);

            return -(gamma * (1 + (d_1 / (volatilityPercent * Math.Sqrt(T)))) / underlyingPrice);
        }

        //
        // Zomma: gives the rate of change for Gamma for a change in IV
        //
        public static double Zomma_GammaSensitivityVol(double underlyingPrice, OptionType optionType, double optionStrikePrice, double calendarDaysToExpiry, double volatilityPercent, double riskFreeRatePercent, double dividendYieldPercent)
        {
            double gamma1 = SpotGamma(underlyingPrice, optionStrikePrice, calendarDaysToExpiry, volatilityPercent, riskFreeRatePercent, dividendYieldPercent);
            double gamma2 = SpotGamma(underlyingPrice, optionStrikePrice, calendarDaysToExpiry, (volatilityPercent + .01), riskFreeRatePercent, dividendYieldPercent);

            return (gamma2 - gamma1);
        }

        //
        // Color: gives change in Gamma per passage of 1 calendar day
        //
        public static double Color_GammaTimeDecay(double underlyingPrice, OptionType optionType, double optionStrikePrice, double calendarDaysToExpiry, double volatilityPercent, double riskFreeRatePercent, double dividendYieldPercent)
        {
            double gamma1 = SpotGamma(underlyingPrice, optionStrikePrice, calendarDaysToExpiry, volatilityPercent, riskFreeRatePercent, dividendYieldPercent);
            double gamma2 = SpotGamma(underlyingPrice, optionStrikePrice, (calendarDaysToExpiry - 1), volatilityPercent, riskFreeRatePercent, dividendYieldPercent);

            return (gamma2 - gamma1);
        }

        #endregion
    }

    /// <summary>
    /// ChatGPT generated code
    /// </summary>
    public static partial class OptionsPricing
    {
        #region Public Calculation Methods

        public static double EuropeanOptionPrice(OptionType optionType, double underlyingPrice, double strikePrice, double calendarDaysToExpiration, double riskFreeRate, double volatility)
        {
            double timeToExpiration = (calendarDaysToExpiration / CalendarDaysPerYear);

            if (optionType == OptionType.call)
                return _CalculateEuroCallOptionPrice(underlyingPrice, strikePrice, timeToExpiration, riskFreeRate, volatility);
            else
                return _CalculateEuroPutOptionPrice(underlyingPrice, strikePrice, timeToExpiration, riskFreeRate, volatility);
        }

        public static double AmericanOptionPrice(OptionType optionType, double underlyingPrice, double strikePrice, double calendarDaysToExpiration, double riskFreeRate, double volatility)
        {
            double timeToExpiration = (calendarDaysToExpiration / CalendarDaysPerYear);

            if (optionType == OptionType.call)
                return _CalculateAmericanCallOptionPrice(underlyingPrice, strikePrice, timeToExpiration, riskFreeRate, volatility);
            else
                return _CalculateAmericanPutOptionPrice(underlyingPrice, strikePrice, timeToExpiration, riskFreeRate, volatility);
        }

        #endregion

        public static double _CalculateEuroCallOptionPrice(double underlyingPrice, double strikePrice, double timeToExpiration, double riskFreeRate, double volatility)
        {
            double d1 = (Math.Log(underlyingPrice / strikePrice)
                        + (riskFreeRate + Math.Pow(volatility, 2) / 2) * timeToExpiration)
                        / (volatility * Math.Sqrt(timeToExpiration));

            double d2 = d1 - volatility * Math.Sqrt(timeToExpiration);

            double callPrice = underlyingPrice * N(d1)
                             - strikePrice * Math.Exp(-riskFreeRate * timeToExpiration) * N(d2);

            return callPrice;
        }
        public static double _CalculateEuroPutOptionPrice(double underlyingPrice, double strikePrice, double timeToExpiration, double riskFreeRate, double volatility)
        {
            double d1 = (Math.Log(underlyingPrice / strikePrice) + (riskFreeRate + volatility * volatility / 2.0) * timeToExpiration) / (volatility * Math.Sqrt(timeToExpiration));
            double d2 = d1 - volatility * Math.Sqrt(timeToExpiration);

            double putOptionPrice = strikePrice * Math.Exp(-riskFreeRate * timeToExpiration) * N(-d2)
                - underlyingPrice * N(-d1);

            return putOptionPrice;
        }
        public static double _CalculateAmericanCallOptionPrice(double underlyingPrice, double strikePrice, double timeToExpiration, double riskFreeRate, double volatility, int numberOfSteps = 100)
        {
            double deltaT = timeToExpiration / numberOfSteps;
            double up = Math.Exp(volatility * Math.Sqrt(deltaT));
            double down = 1.0 / up;
            double pUp = (Math.Exp(riskFreeRate * deltaT) - down) / (up - down);
            double pDown = 1.0 - pUp;

            double[] underlyingPrices = new double[numberOfSteps + 1];
            double[] optionValues = new double[numberOfSteps + 1];

            for (int i = 0; i <= numberOfSteps; i++)
            {
                underlyingPrices[i] = underlyingPrice * Math.Pow(up, numberOfSteps - i) * Math.Pow(down, i);
                optionValues[i] = Math.Max(underlyingPrices[i] - strikePrice, 0);
            }

            for (int i = numberOfSteps - 1; i >= 0; i--)
            {
                for (int j = 0; j <= i; j++)
                {
                    underlyingPrices[j] = underlyingPrices[j] / up;
                    optionValues[j] = (pUp * optionValues[j] + pDown * optionValues[j + 1]) * Math.Exp(-riskFreeRate * deltaT);
                    optionValues[j] = Math.Max(optionValues[j], underlyingPrices[j] - strikePrice);
                }
            }

            return optionValues[0];
        }
        public static double _CalculateAmericanPutOptionPrice(double underlyingPrice, double strikePrice, double timeToExpiration, double riskFreeRate, double volatility, int numberOfSteps = 100)
        {
            double deltaT = timeToExpiration / numberOfSteps;
            double up = Math.Exp(volatility * Math.Sqrt(deltaT));
            double down = 1.0 / up;
            double pUp = (Math.Exp(riskFreeRate * deltaT) - down) / (up - down);
            double pDown = 1.0 - pUp;

            double[] underlyingPrices = new double[numberOfSteps + 1];
            double[] optionValues = new double[numberOfSteps + 1];

            for (int i = 0; i <= numberOfSteps; i++)
            {
                underlyingPrices[i] = underlyingPrice * Math.Pow(up, numberOfSteps - i) * Math.Pow(down, i);
                optionValues[i] = Math.Max(strikePrice - underlyingPrices[i], 0);
            }

            for (int i = numberOfSteps - 1; i >= 0; i--)
            {
                for (int j = 0; j <= i; j++)
                {
                    underlyingPrices[j] = underlyingPrices[j] / up;
                    optionValues[j] = (pUp * optionValues[j] + pDown * optionValues[j + 1]) * Math.Exp(-riskFreeRate * deltaT);
                    optionValues[j] = Math.Max(optionValues[j], strikePrice - underlyingPrices[j]);
                }
            }

            return optionValues[0];
        }

        public static double OptionLambda(double underlyingPrice, double optionPrice, double optionDelta)
        {
            // Calculate the percentage change in the underlying price
            double underlyingPercentChange = 0.01;

            // Calculate the change in the option price for a 1% change in the underlying price
            double optionPriceChange = optionDelta * underlyingPrice * underlyingPercentChange;

            // Calculate the Lambda of the option
            double optionLambda = optionPriceChange / optionPrice * 100;

            return optionLambda;
        }

        public static double ImpliedVolatility(
       OptionType optionType,
       double underlyingPrice,
       double strikePrice,
       double calendarDaysToExpiration,
       double riskFreeRate,
       double optionPrice)
        {
            // Initial guess for implied volatility
            double volatilityGuess = 1.00;
            double step = 1.00;

            // Maximum number of iterations
            int maxIterations = 100;

            // Tolerance for convergence
            double tolerance = 0.001;

            // Iteratively solve for the implied volatility

            double guessPrice = AmericanOptionPrice(optionType, underlyingPrice, strikePrice, calendarDaysToExpiration, riskFreeRate, volatilityGuess);

            int iterations = 0;
            while (Math.Abs(guessPrice - optionPrice) > tolerance && iterations < maxIterations)
            {

                // If the calculated price is too low, raise the IV.  If it is too high, lower.

                if (guessPrice < optionPrice)
                {
                    volatilityGuess += step;
                }
                else
                {
                    step /= 2.0;
                    volatilityGuess -= step;
                }

                guessPrice = AmericanOptionPrice(optionType, underlyingPrice, strikePrice, calendarDaysToExpiration, riskFreeRate, volatilityGuess);
                iterations++;
            }

            if (iterations == maxIterations)
            {
                throw new ApplicationException("Implied volatility did not converge");
            }

            return volatilityGuess;
        }
    }
}
