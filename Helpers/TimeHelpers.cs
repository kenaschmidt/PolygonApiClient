using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PolygonApiClient.Helpers
{

    public static class TimeHelpers
    {
        static TimeZoneInfo eastern = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
        static TimeZoneInfo central = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");
        static TimeZoneInfo UTC = TimeZoneInfo.FindSystemTimeZoneById("UTC");

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
    }
}
