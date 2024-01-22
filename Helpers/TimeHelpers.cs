using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PolygonApiClient
{
    public static class TimeHelpers
    {
        static TimeZoneInfo eastern = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
        static TimeZoneInfo central = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");
        static TimeZoneInfo UTC = TimeZoneInfo.FindSystemTimeZoneById("UTC");

        public static TimeSpan MarketPremarketOpenEST = new TimeSpan(4, 0, 0);
        public static TimeSpan MarketRTHOpenEST = new TimeSpan(9, 30, 0);
        public static TimeSpan MarketRTHCloseEST = new TimeSpan(16, 0, 0);
        public static TimeSpan MarketAfterHoursCloseEST = new TimeSpan(20, 0, 0);

        // Value used for options pricing calculations
        static double SecondsInTradingDayRTH = (MarketRTHCloseEST - MarketRTHOpenEST).TotalSeconds;

        public const double CalendarDaysPerYear = 365.0;

        public static DateTime NowEST => DateTime.UtcNow.UTC_to_EST();

        public static long MilliToNano(this long me)
        {
            return me * 1000 * 1000;
        }
        public static long NanoToMilli(this long me)
        {
            return me / 1000 / 1000;
        }
        public static DateTime UTC_to_EST(this DateTime UtcTime)
        {
            return TimeZoneInfo.ConvertTimeFromUtc(UtcTime, eastern);
        }
        public static DateTime EST_to_UTC(this DateTime EstTime)
        {
            return TimeZoneInfo.ConvertTimeToUtc(EstTime, eastern);
        }
        public static DateTime Local_to_EST(this DateTime LocalTime)
        {
            LocalTime = DateTime.SpecifyKind(LocalTime, DateTimeKind.Local);
            return TimeZoneInfo.ConvertTime(LocalTime, eastern);
        }
        public static DateTime UnixNanosecondsToUTC(this long nanoseconds)
        {
            // Unix timestamp is seconds past epoch
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddTicks(nanoseconds / 100);

            return dateTime;
        }
        public static DateTime UnixNanosecondsToEST(this long nanoseconds)
        {
            // Unix timestamp is seconds past epoch
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddTicks(nanoseconds / 100);

            return dateTime.UTC_to_EST();
        }
        public static DateTime UnixMillisecondsToEST(this long milliseconds)
        {
            // Unix timestamp is seconds past epoch
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddMilliseconds(milliseconds);

            return dateTime.UTC_to_EST();
        }
        public static DateTime UnixMillisecondsToUTC(this long milliseconds)
        {
            // Unix timestamp is seconds past epoch
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddMilliseconds(milliseconds);

            return dateTime;
        }
        public static long ESTToUnixNanoseconds(this DateTime dateTime)
        {
            return ESTToUnixMilliseconds(dateTime).MilliToNano();
        }
        public static long ESTToUnixMilliseconds(this DateTime dateTime)
        {
            var ret = (long)(dateTime.EST_to_UTC().Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds);
            return ret;
        }
        public static long UTCToUnixNanoseconds(this DateTime dateTime)
        {
            return UTCToUnixMilliseconds(dateTime).MilliToNano();
        }
        public static long UTCToUnixMilliseconds(this DateTime dateTime)
        {
            var ret = (long)(dateTime.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds);
            return ret;
        }

        /// <summary>
        /// Returns a fraction (0.0-1.0) of a standard trading day remaining based on RTH (9:30-16:00 EST)
        /// </summary>
        /// <param name="dateTimeEST"></param>
        /// <returns></returns>
        public static double FractionOfTradingDayRemaining(this DateTime dateTimeEST)
        {
            if (dateTimeEST.TimeOfDay <= MarketRTHOpenEST)
                return 1.0;

            if (dateTimeEST.TimeOfDay >= MarketRTHCloseEST)
                return 0.0;

            return ((MarketRTHCloseEST - dateTimeEST.TimeOfDay).TotalSeconds) / SecondsInTradingDayRTH;
        }

        /// <summary>
        /// Calculates the fraction of a calendar year (365 days) that a particular subdivision represents
        /// </summary>
        /// <param name="timespan"></param>
        /// <param name="multiplier"></param>
        /// <returns></returns>
        public static double FractionOfCalendarYear(PolygonTimespan timespan, int multiplier)
        {
            double ret = 1.0;

            switch (timespan)
            {
                case PolygonTimespan.second:
                    ret /= (CalendarDaysPerYear * 24 * 60 * 60);
                    break;
                case PolygonTimespan.minute:
                    ret /= (CalendarDaysPerYear * 24 * 60);
                    break;
                case PolygonTimespan.hour:
                    ret /= (CalendarDaysPerYear * 24);
                    break;
                case PolygonTimespan.day:
                    ret /= CalendarDaysPerYear;
                    break;
                case PolygonTimespan.week:
                    ret /= 52;
                    break;
                case PolygonTimespan.month:
                    ret /= 12;
                    break;
                case PolygonTimespan.quarter:
                    ret /= 4;
                    break;
                case PolygonTimespan.year:
                    break;
                default:
                    break;
            }

            return ret * multiplier;
        }

        /// <summary>
        /// Converts a polygon timespan to local Timespan object; NOTE: should not be used for month or above as it will not account for differing days/month
        /// </summary>
        /// <param name="me"></param>
        /// <param name="multiplier"></param>
        /// <returns></returns>
        public static TimeSpan ToTimespan(this PolygonTimespan me, int multiplier = 1)
        {
            switch (me)
            {
                case PolygonTimespan.second:
                    return new TimeSpan(0, 0, 0, 1 * multiplier);
                case PolygonTimespan.minute:
                    return new TimeSpan(0, 0, 1 * multiplier, 0);
                case PolygonTimespan.hour:
                    return new TimeSpan(0, 1 * multiplier, 0, 0);
                case PolygonTimespan.day:
                    return new TimeSpan(1 * multiplier, 0, 0, 0);
                case PolygonTimespan.week:
                    return new TimeSpan(7 * multiplier, 0, 0, 0);
                case PolygonTimespan.month:
                    return new TimeSpan(30 * multiplier, 0, 0, 0);
                case PolygonTimespan.quarter:
                    return new TimeSpan(90 * multiplier, 0, 0, 0);
                case PolygonTimespan.year:
                    return new TimeSpan((int)CalendarDaysPerYear * multiplier);
                default:
                    return new TimeSpan(0, 0, 0, 0);
            }
        }

        /// <summary>
        /// Returns a normalized value for the start datetime of a price bar for given length
        /// </summary>
        /// <param name="me"></param>
        /// <param name="timespan"></param>
        /// <param name="multiplier"></param>
        /// <returns></returns>
        public static DateTime BarStartValue(this DateTime me, PolygonTimespan timespan, int multiplier)
        {
            DateTime start = me.Date;

            int timeout = 86001;

            while (start < me && timeout-- > 0)
            {
                if (start.Increment(timespan, multiplier) > me)
                    return start;
                else
                    start = start.Increment(timespan, multiplier);
            }

            throw new Exception("Bar increment error unknown");
        }

        /// <summary>
        /// Increments a DateTime by the provided bar size
        /// </summary>
        /// <param name="me"></param>
        /// <param name="timespan"></param>
        /// <param name="multiplier"></param>
        /// <returns></returns>
        public static DateTime Increment(this DateTime me, PolygonTimespan timespan, int multiplier)
        {
            return me.Add(timespan.ToTimespan(multiplier));

            // Clean this up to skip non trading hours

        }

    }
}
