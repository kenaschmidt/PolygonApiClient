using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace PolygonApiClient
{
    /// <summary>
    /// Provides data on trading calendar, holidays, etc
    /// </summary>
    public static class Calendar
    {
        public static DateTime CurrentTimeEst => DateTime.UtcNow.UTC_to_EST();

        public static TimeSpan RegularTradingHoursOpen = new TimeSpan(9, 30, 0);
        public static TimeSpan RegularTradingHoursClose = new TimeSpan(16, 0, 0);
        public static TimeSpan ExtendedTradingHoursOpen = new TimeSpan(4, 0, 0);
        public static TimeSpan ExtendedTradingHoursClose = new TimeSpan(20, 0, 0);
        public static bool IsRegularTradingHours(DateTime dateTime)
        {
            return (dateTime.TimeOfDay >= RegularTradingHoursOpen && dateTime.TimeOfDay < RegularTradingHoursClose);
        }
        public static bool IsExtendedHours(DateTime dateTime)
        {
            return (dateTime.TimeOfDay >= ExtendedTradingHoursOpen && dateTime.TimeOfDay < RegularTradingHoursOpen) ||
                (dateTime.TimeOfDay >= RegularTradingHoursClose && dateTime.TimeOfDay < ExtendedTradingHoursClose);
        }

        #region Trading Holidays

        // Holidays which fall on a Saturday are observed on Friday, those which fall on Sunday are observed on Monday

        [Holiday]
        public static DateTime NewYearsDay(int Year)
        {
            DateTime ret = new DateTime(Year, 1, 1);

            if (ret.DayOfWeek == DayOfWeek.Saturday)
                ret = ret.AddDays(-1);
            if (ret.DayOfWeek == DayOfWeek.Sunday)
                ret = ret.AddDays(1);

            return ret;
        }

        /// <summary>
        /// Third Monday in January
        /// </summary>
        /// <param name="Year"></param>
        /// <returns></returns>
        [Holiday]
        public static DateTime MLKJrDay(int Year)
        {
            // Get first Monday in January
            DateTime ret = new DateTime(Year, 1, 1);
            while (ret.DayOfWeek != DayOfWeek.Monday)
                ret = ret.AddDays(1);

            // Add two weeks
            ret = ret.AddDays(14);

            return ret;
        }

        /// <summary>
        /// Third Monday in February
        /// </summary>
        /// <param name="Year"></param>
        /// <returns></returns>
        [Holiday]
        public static DateTime WashingtonBirthday(int Year)
        {
            // Get first Monday in February
            DateTime ret = new DateTime(Year, 2, 1);
            while (ret.DayOfWeek != DayOfWeek.Monday)
                ret = ret.AddDays(1);

            // Add two weeks
            ret = ret.AddDays(14);

            return ret;
        }

        /// <summary>
        /// Hardcoded list of Good Friday dates in YEAR, MONTH, DAY format
        /// </summary>
        private static List<Tuple<int, int, int>> GoodFridayDates = new List<Tuple<int, int, int>>()
        {
            new Tuple<int,int,int>(1980,4,4),
            new Tuple<int,int,int>(1981,4,17),
            new Tuple<int,int,int>(1982,4,9),
            new Tuple<int,int,int>(1983,4,1),
            new Tuple<int,int,int>(1984,4,20),
            new Tuple<int,int,int>(1985,4,5),
            new Tuple<int,int,int>(1986,3,28),
            new Tuple<int,int,int>(1987,4,17),
            new Tuple<int,int,int>(1988,4,1),
            new Tuple<int,int,int>(1989,3,24),
            new Tuple<int,int,int>(1990,4,13),
            new Tuple<int,int,int>(1991,3,29),
            new Tuple<int,int,int>(1992,4,17),
            new Tuple<int,int,int>(1993,4,9),
            new Tuple<int,int,int>(1994,4,1),
            new Tuple<int,int,int>(1995,4,14),
            new Tuple<int,int,int>(1996,4,5),
            new Tuple<int,int,int>(1997,3,28),
            new Tuple<int,int,int>(1998,4,10),
            new Tuple<int,int,int>(1999,4,2),
            new Tuple<int,int,int>(2000,4,21),
            new Tuple<int,int,int>(2001,4,13),
            new Tuple<int,int,int>(2002,3,29),
            new Tuple<int,int,int>(2003,4,18),
            new Tuple<int,int,int>(2004,4,9),
            new Tuple<int,int,int>(2005,3,25),
            new Tuple<int,int,int>(2006,4,14),
            new Tuple<int,int,int>(2007,4,6),
            new Tuple<int,int,int>(2008,3,21),
            new Tuple<int,int,int>(2009,4,10),
            new Tuple<int,int,int>(2010,4,2),
            new Tuple<int,int,int>(2011,4,22),
            new Tuple<int,int,int>(2012,4,6),
            new Tuple<int,int,int>(2013,3,29),
            new Tuple<int,int,int>(2014,4,18),
            new Tuple<int,int,int>(2015,4,3),
            new Tuple<int,int,int>(2016,3,25),
            new Tuple<int,int,int>(2017,4,14),
            new Tuple<int,int,int>(2018,3,30),
            new Tuple<int,int,int>(2019,4,19),
            new Tuple<int,int,int>(2020,4,10),
            new Tuple<int,int,int>(2021,4,2),
            new Tuple<int,int,int>(2022,4,15),
            new Tuple<int,int,int>(2023,4,7),
            new Tuple<int,int,int>(2024,3,29),
            new Tuple<int,int,int>(2025,4,18),
            new Tuple<int,int,int>(2026,4,3),
            new Tuple<int,int,int>(2027,3,26),
            new Tuple<int,int,int>(2028,4,14),
            new Tuple<int,int,int>(2029,3,30),
            new Tuple<int,int,int>(2030,4,19),
            new Tuple<int,int,int>(2031,4,11),
            new Tuple<int,int,int>(2032,3,26),
            new Tuple<int,int,int>(2033,4,15),
            new Tuple<int,int,int>(2034,4,7),
            new Tuple<int,int,int>(2035,3,23),
            new Tuple<int,int,int>(2036,4,11),
            new Tuple<int,int,int>(2037,4,3),
            new Tuple<int,int,int>(2038,4,23),
            new Tuple<int,int,int>(2039,4,8)
        };

        /// <summary>
        /// Specified for each year
        /// </summary>
        [Holiday]
        public static DateTime GoodFriday(int Year)
        {
            try
            {
                var val = GoodFridayDates.Find(x => x.Item1 == Year) ?? throw new Exception(message: "Outside range for known Good Friday dates");

                return new DateTime(val.Item1, val.Item2, val.Item3);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        /// <summary>
        /// Last Monday of May
        /// </summary>
        [Holiday]
        public static DateTime MemorialDay(int Year)
        {
            // Get last Monday in May
            DateTime ret = new DateTime(Year, 5, 31);
            while (ret.DayOfWeek != DayOfWeek.Monday)
                ret = ret.AddDays(-1);

            return ret;
        }

        /// <summary>
        /// July 4th
        /// </summary>
        [Holiday]
        public static DateTime IndependenceDay(int Year)
        {
            DateTime ret = new DateTime(Year, 7, 4);

            if (ret.DayOfWeek == DayOfWeek.Saturday)
                ret = ret.AddDays(-1);
            if (ret.DayOfWeek == DayOfWeek.Sunday)
                ret = ret.AddDays(1);

            return ret;
        }

        /// <summary>
        /// First Monday of September
        /// </summary>
        [Holiday]
        public static DateTime LaborDay(int Year)
        {
            // Get first Monday in January
            DateTime ret = new DateTime(Year, 9, 1);
            while (ret.DayOfWeek != DayOfWeek.Monday)
                ret = ret.AddDays(1);

            return ret;
        }

        /// <summary>
        /// Fourth Thursday in November
        /// </summary>
        /// <param name="Year"></param>
        /// <returns></returns>
        [Holiday]
        public static DateTime ThanksgivingDay(int Year)
        {
            // Get first Thursday in November
            DateTime ret = new DateTime(Year, 11, 1);
            while (ret.DayOfWeek != DayOfWeek.Thursday)
                ret = ret.AddDays(1);

            // Add three weeks
            ret = ret.AddDays(21);

            return ret;
        }

        [Holiday]
        public static DateTime Christmas(int Year)
        {
            DateTime ret = new DateTime(Year, 12, 25);

            if (ret.DayOfWeek == DayOfWeek.Saturday)
                ret = ret.AddDays(-1);
            if (ret.DayOfWeek == DayOfWeek.Sunday)
                ret = ret.AddDays(1);

            return ret;
        }

        /// <summary>
        /// Used to simplify running of holiday checks
        /// </summary>
        [System.AttributeUsage(System.AttributeTargets.Method)]
        private class HolidayAttribute : Attribute { }

        #endregion

        //
        // Dictionary populated with known good trading dates as encountered to speed up future checks
        //
        private static ConcurrentDictionary<DateTime, bool> KnownTradingDays = new ConcurrentDictionary<DateTime, bool>();

        private static List<DateTime> NonStandardClosureDates = new List<DateTime>()
        {
            new DateTime(2018,12,5),  // Former President GW Bush national day of mourning
            new DateTime(2012,10,29), // Hurricane Sandy
            new DateTime(2012,10,30), // Hurricane Sandy
            new DateTime(2004,06,11), // Former President Reagan national day of mourning
            new DateTime(2001,09,11), // September 11
            new DateTime(2001,09,12), // September 11
            new DateTime(2001,09,13), // September 11
            new DateTime(2001,09,14)  // September 11
        };

        public static bool IsTradingDay(DateTime date)
        {
            try
            {
                if (KnownTradingDays.TryGetValue(date, out bool value) == true)
                    return value;

                if (IsWeekend(date))
                {
                    KnownTradingDays.TryAdd(date, false);
                    return false;
                }

                // Try each Holiday function to determine if this date is a Holiday
                try
                {

                    foreach (var func in typeof(Calendar).GetMethods().Where(m => Attribute.IsDefined(m, typeof(HolidayAttribute))))
                    {
                        if ((DateTime)func.Invoke(null, new object[] { date.Year }) == date)
                        {
                            KnownTradingDays.TryAdd(date, false);
                            return false;
                        }
                    }

                }
                catch (StackOverflowException ex)
                {
                    throw ex;
                }
                // Check for any non-standard closures
                if (NonStandardClosureDates.Contains(date))
                {
                    KnownTradingDays.TryAdd(date, false);
                    return false;
                }

                // If no Holidays are returned, assume this is a valid trading day
                KnownTradingDays.TryAdd(date, true);
                return true;

            }
            catch (Exception ex)
            {
                var t = date;
                throw ex;
            }
        }
        public static bool IsWeekend(DateTime date)
        {
            return (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday);
        }
        public static int TradingDayCount(DateTime startDate, DateTime endDate)
        {
            int count = 0;
            while (startDate <= endDate)
            {
                if (IsTradingDay(startDate))
                    count += 1;
                startDate = NextTradingDay(startDate);
            }
            return count;
        }

        public static DateTime NextTradingDay(DateTime date, int Days = 1)
        {
            DateTime ret = date.Date;

            while (Days > 0)
            {
                ret = date.AddDays(1);
                while (!IsTradingDay(ret))
                    ret = ret.AddDays(1);
                Days -= 1;
            }

            return ret;
        }

        public static DateTime PriorTradingDay(DateTime date, int Days = 1)
        {
            DateTime ret = date;

            while (Days-- > 0)
            {
                ret = ret.AddDays(-1).Date;
                while (!IsTradingDay(ret))
                    ret = ret.AddDays(-1);
            }

            return ret;
        }

        public static DateTime CurrentOrPriorTradingDay(DateTime date)
        {
            return (IsTradingDay(date) ? date : PriorTradingDay(date));
        }


        public static DateTime NextTradingWeekStart(DateTime date, int Weeks = 1)
        {
            DateTime ret = date.Date;

            // Add days to move the date the the next Monday
            if (ret.DayOfWeek == DayOfWeek.Monday)
                ret = ret.AddDays(7);
            else
                ret = ret.AddDays((8 - (int)ret.DayOfWeek) % 7);

            // Add additional weeks if user specified
            while (--Weeks > 0)
                ret = ret.AddDays(7);

            if (!IsTradingDay(ret))
                ret = NextTradingDay(ret);

            return ret;
        }
     
        public static DateTime NextTradingMonthStart(DateTime date, int Months = 1)
        {
            DateTime ret = date.Date.AddMonths(1);
            ret = new DateTime(ret.Year, ret.Month, 1);

            while (--Months > 0)
                ret = ret.AddMonths(1);

            if (!IsTradingDay(ret))
                ret = NextTradingDay(ret);

            return ret;
        }
    
        public static DateTime NextTradingQuarterStart(DateTime date, int Quarters = 1)
        {
            DateTime ret = date.Date;
            ret = new DateTime(ret.Year, ret.Month, 1);

            int nextMonths = ((12 - ret.Month) % 3) + 1;
            ret = NextTradingMonthStart(ret, nextMonths);

            while (--Quarters > 0)
                ret = NextTradingMonthStart(ret, 3);

            return ret;
        }


        public static DateTime PriorTradingWeekStart(DateTime date, int Weeks = 1)
        {
            DateTime ret = date.Date;

            // Add days to move the date the the prior Monday
            if (ret.DayOfWeek != DayOfWeek.Monday)
                ret = ret.AddDays(((8 - (int)ret.DayOfWeek) % 7) - 7);

            while (Weeks-- > 0)
                ret = ret.AddDays(-7);

            if (!IsTradingDay(ret))
                ret = NextTradingDay(ret);

            return ret;
        }
    
        public static DateTime PriorTradingMonthStart(DateTime date, int Months = 1)
        {
            DateTime ret = new DateTime(date.Year, date.Month, 1);

            while (Months-- > 0)
                ret = ret.AddMonths(-1);

            if (!IsTradingDay(ret))
                ret = NextTradingDay(ret);

            return ret;
        }
       
        public static DateTime PriorTradingQuarterStart(DateTime date, int Quarters = 1)
        {
            DateTime ret = date.Date;
            ret = new DateTime(ret.Year, ret.Month, 1);

            int nextMonths = 2 - ((12 - ret.Month) % 3);
            ret = PriorTradingMonthStart(ret, nextMonths);

            while (Quarters-- > 0)
                ret = PriorTradingMonthStart(ret, 3);

            return ret;
        }

        public static DateTime FirstTradingDayOfMonth(DateTime date)
        {
            date = new DateTime(date.Year, date.Month, 1);
            while (!IsTradingDay(date))
                date = NextTradingDay(date);

            return date;
        }

        public static DateTime FirstTradingDayOfQuarter(DateTime date)
        {
            return PriorTradingQuarterStart(date, 0);
        }

        public static DateTime FirstTradingDayOfWeek(DateTime date)
        {
            DateTime ret = date.Date;

            if (ret.DayOfWeek != DayOfWeek.Monday)
                ret = ret.AddDays(((8 - (int)ret.DayOfWeek) % 7) - 7);
            if (!IsTradingDay(ret))
                ret = NextTradingDay(ret);

            return ret;
        }

        public static DateTime LastTradingDayOfWeek(DateTime date)
        {
            return PriorTradingDay(NextTradingWeekStart(date));
        }


        public static List<DateTime> AllHolidays(DateTime start, DateTime end)
        {
            List<DateTime> ret = new List<DateTime>();

            while (start <= end)
            {
                if (!IsWeekend(start) && !IsTradingDay(start))
                    ret.Add(start);
                if (NonStandardClosureDates.Contains(start))
                    ret.Add(start);

                start = start.AddDays(1);
            }

            return ret;
        }

    
    }
}
