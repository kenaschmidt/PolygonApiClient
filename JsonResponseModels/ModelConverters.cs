using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Database_Common_Trading;
using Database_Common_Trading.Models;
using static Database_Common_Trading.Helpers;

namespace DataFarm_Polygon.Models
{
    internal static class ModelConverters
    {
        /// <summary>
        /// Convert a Polygon trade response (REST) to a normalized database object
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public static TradeTick ToTradeTick(RestTrades_Result result, string symbol)
        {
            var ret = new TradeTick()
            {
                Symbol = symbol,
                ExchangeTimeStamp_Nano = result.participant_timestamp,
                SipTimestamp_Nano = result.sip_timestamp,
                Price = result.price,
                Size = result.size,
                TradeId = result.id,
                SequenceNumber = result.sequence_number,
                Tape = (Tape)result.tape,
                Exchange = (Exchange)result.exchange,
                CorrectedTrade = result.correction
            };

            if (result.conditions != null)
                foreach (var conditionCode in result.conditions)
                {
                    ret.TradeConditions.Add(conditionCode.ToTradeConditionFlag());
                }

            return ret;
        }

        /// <summary>
        /// Convert a Polygon trade response (SOCKET) to a normalized database object
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public static TradeTick ToTradeTick(SocketTrade result)
        {
            var ret = new TradeTick()
            {
                Symbol = result.Symbol,
                ExchangeTimeStamp_Nano = result.ExchangeTimestamp_Ms.MsToNano(),
                SipTimestamp_Nano = result.TRFTimestamp_Ms.MsToNano(),
                Price = result.Price,
                Size = result.Size,
                TradeId = result.TradeId,
                SequenceNumber = result.SequenceNumber,
                Tape = (Tape)result.Tape,
                Exchange = (Exchange)result.ExchangeId
            };

            if (result.TradeConditions != null)
                foreach (var conditionCode in result.TradeConditions)
                {
                    ret.TradeConditions.Add(conditionCode.ToTradeConditionFlag());
                }

            return ret;
        }

        /// <summary>
        /// Converts a Polygon quote response (REST) to a normalized database object
        /// </summary>
        /// <param name="result"></param>
        /// <param name="symbol"></param>
        /// <returns></returns>
        public static QuoteTick ToQuoteTick(RestQuotes_Result result, string symbol)
        {
            var ret = new QuoteTick()
            {
                Symbol = symbol,
                ExchangeTimeStamp_Nano = result.participant_timestamp,
                SipTimestamp_Nano = result.sip_timestamp,
                //
                // NOTE: We convert from integral size to round-lot size here by multiplying by 100
                //
                Bid_Size = result.bid_size * 100,
                Ask_Size = result.ask_size * 100,
                Bid_Price = result.bid_price,
                Ask_Price = result.ask_price,
                Bid_Exchange = (Exchange)result.bid_exchange,
                Ask_Exchange = (Exchange)result.ask_exchange,
                SequenceNumber = result.sequence_number,
                Tape = (Tape)result.tape
            };
            if (result.conditions != null)
                foreach (var conditionCode in result.conditions)
                {
                    ret.QuoteConditions |= conditionCode.ToQuoteConditionFlag();
                }
            if (result.indicators != null)
            {
                ret.QuoteIndicators = Array.ConvertAll<int, QuoteIndicators>(result.indicators, x => (QuoteIndicators)x);
            }

            return ret;
        }

        /// <summary>
        /// Converts a Polygon quote response (SOCKET) to a normalized database object
        /// </summary>
        /// <param name="result"></param>
        /// <param name="symbol"></param>
        /// <returns></returns>
        public static QuoteTick ToQuoteTick(SocketQuote result)
        {
            var ret = new QuoteTick()
            {
                Symbol = result.Symbol,
                ExchangeTimeStamp_Nano = result.Timestamp_Ms.MsToNano(),
                SipTimestamp_Nano = result.Timestamp_Ms.MsToNano(),
                //
                // NOTE: We convert from integral size to round-lot size here by multiplying by 100
                //
                Bid_Size = result.Bid_Size * 100,
                Ask_Size = result.Ask_Size * 100,
                Bid_Price = result.Bid_Price,
                Ask_Price = result.Ask_Price,
                Bid_Exchange = (Exchange)result.Bid_Exchange,
                Ask_Exchange = (Exchange)result.Ask_Exchange,
                SequenceNumber = result.SequenceNumber,
                Tape = (Tape)result.Tape
            };

            ret.QuoteConditions |= result.ConditionCode.ToQuoteConditionFlag();

            if (result.IndicatorCodes != null)
            {
                ret.QuoteIndicators = Array.ConvertAll<int, QuoteIndicators>(result.IndicatorCodes, x => (QuoteIndicators)x);
            }

            return ret;
        }

        /// <summary>
        /// Converts a Polygon Aggregate object (SOCKET) to a local PriceBar
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public static PriceBar ToPriceBar(SocketAggregate result, PriceBarSize barSize, int barSizeMultiplier)
        {
            var ret = new PriceBar(barSize, barSizeMultiplier)
            {
                Symbol = result.Symbol,
                StartTimestamp_Ms = result.TimeStamp_Start_Ms,
                EndTimestamp_Ms = result.TimeStamp_End_Ms,
                Open = result.Open,
                High = result.High,
                Low = result.Low,
                Close = result.Close,
                Volume = result.Volume,
                VWAP = result.DayVWAP,
                AverageTradeSize = result.AverageTradeSize,
                DayOpen = result.DayOpen
            };
            return ret;
        }

        /// <summary>
        /// Converts a Polygon Aggregate object (REST) to a local PriceBar
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public static PriceBar ToPriceBar(RestAggregates_Result result, string symbol, int multiplier, PriceBarSize barSize)
        {
            var ret = new PriceBar(barSize, multiplier)
            {
                Symbol = symbol,
                Open = result.Open,
                Close = result.Close,
                Volume = result.Volume,
                High = result.High,
                Low = result.Low,
                VWAP = result.VWAP,
                AverageTradeSize = (double)result.Volume / result.NumberOfTrades,
                StartTimestamp_Ms = result.Timestamp,
                EndTimestamp_Ms = result.Timestamp + (barSize.ToMilliseconds() * multiplier) - 1
            };
            return ret;
        }

        /// <summary>
        /// Converts a Polygon Options Chain Result object (REST) to a normalized Option object
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public static Option ToOption(RestOptionsChain_Result result)
        {
            var ret = new Option()
            {
                Symbol = result.details.ticker,
                UnderlyingSymbol = result.underlying_asset.ticker,
                Expiry = DateTime.ParseExact(result.details.expiration_date, "yyyy-MM-dd", CultureInfo.CurrentCulture),
                Strike = result.details.strike_price,
                OptionType = result.details.contract_type == "call" ? OptionType.Call : OptionType.Put,
                delta = result.greeks.delta,
                gamma = result.greeks.gamma,
                theta = result.greeks.theta,
                vega = result.greeks.vega,
                ImpliedVolatility = result.implied_volatility,
                OpenInterest = result.open_interest
            };

            return ret;
        }

        /// <summary>
        /// Converts Polygon Option Chain results to Option data (captures OI, etc)
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public static OptionHistoricalData ToOptionHistoricalData(RestOptionsChain_Result result)
        {
            if (result.details.ticker.StartsWith("O:"))
                result.details.ticker.Remove(0, 2);

            var ret = new OptionHistoricalData()
            {
                Symbol = result.details.ticker,
                UnderlyingSymbol = result.underlying_asset.ticker,
                Expiry = DateTime.ParseExact(result.details.expiration_date, "yyyy-MM-dd", CultureInfo.CurrentCulture),
                Strike = result.details.strike_price,
                OptionType = result.details.contract_type == "call" ? OptionType.Call : OptionType.Put,
                OpenInterest = result.open_interest,
                ImpliedVolatility = result.implied_volatility
            };

            //
            // ThroughDate is calculated at 6AM EST / 5 AM CST (previous day's data arrives)
            //
            if (DateTime.Now.TimeOfDay.Hours < 5)
                ret.ThroughDate = DateTime.Today.AddDays(-2);
            else
                ret.ThroughDate = DateTime.Today.AddDays(-1);

            ret.CollectedDate = DateTime.Today;

            return ret;
        }
    }
}
