
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PolygonApiClient
{
    public static class PolygonHelpers
    {
        #region Option Symbol Helpers

        /// <summary>
        /// Determines if a symbol is an Options symbol, with the format [SYMBOL][YYMMDD][C/P][00000000]
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        public static bool IsOptionSymbol(this string symbol)
        {
            return (Regex.Match(symbol, @"^\D{1,6}\d{6}[cCpP]\d{8}").Success);
        }

        /// <summary>
        /// Normalizes a PolygonIO option symbol to OCC standard
        /// </summary>
        /// <param name="polygonOptionSymbol"></param>
        /// <returns></returns>
        public static string PolygonOptionSymbolToOcc(string polygonOptionSymbol)
        {

            if (polygonOptionSymbol.StartsWith("O:"))
                polygonOptionSymbol = polygonOptionSymbol.Remove(0, 2);

            var underlyingSymbol = Regex.Match(polygonOptionSymbol, @"^\D+").Value;

            var expiry = Regex.Match(polygonOptionSymbol, @"\d{6}").Value;

            char cp = Regex.Match(polygonOptionSymbol, @"\d[CP]{1}\d").Value[1];

            var strike = Regex.Match(polygonOptionSymbol, @"\d{8}").Value;

            return $"{underlyingSymbol.ToUpper().PadRight(6)}{expiry}{cp}{strike}";
        }

        /// <summary>
        /// Converts a standard OCC option symbol to PolygonIO standard
        /// </summary>
        /// <param name="occOptionSymbol"></param>
        /// <returns></returns>
        public static string OccOptionSymbolToPolygon(string occOptionSymbol)
        {
            var c = ParseOCCOptionSymbol(occOptionSymbol);
            return $"O:{c.symbol}{c.expiry:yyMMdd}{c.optionType}{(c.strike * 1000).ToString().PadLeft(8, '0')}";
        }

        /// <summary>
        /// Returns parsed components of an OCC-standard option symbol
        /// </summary>
        /// <param name="me"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static (string symbol, DateTime expiry, string optionType, double strike, int size) ParseOCCOptionSymbol(this string me)
        {
            string symbol = (me.Substring(0, 6).TrimEnd(' '));

            // Determine if this is a mini contract 
            int size = symbol.EndsWith("7") ? 10 : 100;
            symbol = symbol.TrimEnd('7');

            // Parse expiry date
            DateTime expiry = DateTime.ParseExact(me.Substring(6, 6), "yyMMdd", null);

            // Parse Put or Call
            string optionType = me.Substring(12, 1);

            // Parse strike price
            double strike = Double.Parse(me.Substring(13, 8).TrimStart('0')) / 1000.0;

            return (symbol, expiry, optionType, strike, size);
        }

        /// <summary>
        /// Adds a leading 'O:' if the symbol is identified as an option and if not present already
        /// </summary>
        /// <param name="me"></param>
        /// <returns></returns>
        private static string AppendOptionIdentifier(this string me)
        {
            if (me.IsOptionSymbol() && me.Substring(0, 2) != "O:")
                return $"O:{me}";

            return me;
        }

        #endregion

        #region Enum/Code Helpers
        public static string GetDescription(Enum value)
        {
            return value
                    .GetType()
                    .GetMember(value.ToString())
                    .FirstOrDefault()?
                    .GetCustomAttribute<DescriptionAttribute>()?
                    .Description
                ?? value.ToString();
        }

        #endregion

        #region Parameter Normalization Helpers

        public static string NormalizeSymbol(string symbol)
        {
            try
            {
                symbol = symbol.ToUpper();
                return symbol.AppendOptionIdentifier();
            }
            catch (Exception)
            {
                throw new ArgumentException($"Invalid symbol: {symbol}");
            }
        }

        #endregion

        #region REST Request Formatters

        /// <summary>
        /// Returns a formatted optional parameter string to be used in any REST request... that is, everything after the ? in the reequest URL
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static string OptionalParametersStringBuilder(params (string name, object value)[] values)
        {
            if (values.Length == 0)
                return string.Empty;

            StringBuilder sb = new StringBuilder();

            sb.Append("?");

            for (int i = 0; i < values.Length; i++)
            {
                if (values[i].value != null)
                {
                    // Check if there is a filter associated with this parameter
                    if (i + 1 < values.Length && values[i + 1].name.Contains("Filter") && values[i + 1].value != null)
                    {
                        // Add a filter value to the current parameter
                        sb.Append($"{values[i].name}.{values[i + 1].value}={values[i].value}{(i + 1 < values.Length ? "&" : "")}");
                    }
                    else
                    {
                        sb.Append($"{values[i].name}={values[i].value}{(i + 1 < values.Length ? "&" : "")}");
                    }

                    // Skip the next vlue if it is a filter
                    if (i + 1 < values.Length && values[i + 1].name.Contains("Filter"))
                    {
                        i += 1;
                    }
                }
            }

            return sb.ToString();
        }

        #endregion
    }
}
