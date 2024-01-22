using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PolygonApiClient.Helpers
{
    //
    // This class provides a simplified/convenient way of handling the various timestamp formats encountered in Polygon, including conversions.
    // All properties are values calculated from the original provided value.
    //
    public class PolygonTimestamp : IComparable<PolygonTimestamp>
    {
        private long? _Milliseconds { get; set; } = null;
        private long? _Nanoseconds { get; set; } = null;
        private DateTime? _EST { get; set; } = null;
        private DateTime? _UTC { get; set; } = null;

        public DateTime UTC
        {
            get
            {
                return
                    _UTC ?? (_EST.HasValue ? _EST.Value.EST_to_UTC() :
                    _Milliseconds.HasValue ? _Milliseconds.Value.UnixMillisecondsToUTC() :
                    _Nanoseconds.HasValue ? _Nanoseconds.Value.UnixNanosecondsToUTC() :
                    throw new NullReferenceException());
            }
        }
        public DateTime EST
        {
            get
            {
                return
                    _EST ?? (_UTC.HasValue ? _UTC.Value.UTC_to_EST() :
                    _Milliseconds.HasValue ? _Milliseconds.Value.UnixMillisecondsToEST() :
                    _Nanoseconds.HasValue ? _Nanoseconds.Value.UnixNanosecondsToEST() :
                    throw new NullReferenceException());
            }
        }
        public long Milliseconds
        {
            get
            {
                return
                    _Milliseconds ??
                    (_Nanoseconds.HasValue ? _Nanoseconds.Value.NanoToMilli() :
                    _UTC.HasValue ? _UTC.Value.UTCToUnixMilliseconds() :
                    _EST.HasValue ? _EST.Value.ESTToUnixMilliseconds() :
                    throw new NullReferenceException());
            }
        }
        public long Nanoseconds
        {
            get
            {
                return
                    _Nanoseconds ??
                    (_Milliseconds.HasValue ? _Milliseconds.Value.MilliToNano() :
                    _UTC.HasValue ? _UTC.Value.UTCToUnixNanoseconds() :
                    _EST.HasValue ? _EST.Value.ESTToUnixNanoseconds() :
                    throw new NullReferenceException());
            }
        }
        public string yyyy_MM_dd
        {
            get => EST.ToString("yyyy-MM-dd");
        }
        public string yy_MM_dd
        {
            get => EST.ToString("yy-MM-dd");
        }

        private PolygonTimestamp()
        {
        }

        /// <summary>
        /// Creates a new Timestamp from a UNIX Milliseconds (UTC) long
        /// </summary>
        /// <param name="milliseconds"></param>
        /// <returns></returns>
        public static PolygonTimestamp FromMilliseconds(long milliseconds)
        {
            return new PolygonTimestamp() { _Milliseconds = milliseconds };
        }

        /// <summary>
        /// Creates a new Timestamp from a UNIX Nanoseconds (UTC) long
        /// </summary>
        /// <param name="nanoseconds"></param>
        /// <returns></returns>
        public static PolygonTimestamp FromNanoseconds(long nanoseconds)
        {
            return new PolygonTimestamp() { _Nanoseconds = nanoseconds };
        }

        /// <summary>
        /// Creates a new Timestamp from a UTC DateTime
        /// </summary>
        /// <param name="dateTime_UTC"></param>
        /// <returns></returns>
        public static PolygonTimestamp FromUtcDateTime(DateTime dateTime_UTC)
        {
            return new PolygonTimestamp() { _UTC = dateTime_UTC };
        }

        /// <summary>
        /// Creates a new Timestamp from an EST DateTime
        /// </summary>
        /// <param name="dateTime_UTC"></param>
        /// <returns></returns>
        public static PolygonTimestamp FromEstDateTime(DateTime dateTime_EST)
        {
            return new PolygonTimestamp() { _EST = dateTime_EST };
        }

        /// <summary>
        /// Creates a new Timestamp from a YY-MM-DD or YYYY-MM-DD string. Timestamp will be midnight EST time
        /// </summary>
        /// <param name="dateString"></param>
        /// <returns></returns>
        public static PolygonTimestamp FromYYYYMMDD(string dateString)
        {
            return new PolygonTimestamp()
            {
                _EST = DateTime.Parse(dateString)
            };
        }

        public int CompareTo(PolygonTimestamp other)
        {
            return this.Nanoseconds.CompareTo(other.Nanoseconds);
        }

        public static PolygonTimestamp EmptyTimestamp()
        {
            return new PolygonTimestamp() { _UTC = DateTime.MinValue };
        }
    }
}
