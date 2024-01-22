using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PolygonApiClient.ExtendedClient
{
    /// <summary>
    /// Provides user-friendly methods for storing and accessing Bar (aggregate) data
    /// </summary>
    public class BarList
    {
        //
        // Bars are stored in a dictionary with keys of (Timespan, Multiplier) - e.g., (minute,5) or (day,1) - corresponding to the type of aggregate; each key has a list specifically for those bars.
        //
        protected Dictionary<(PolygonTimespan, int), List<Bar>> barCollection { get; } = new Dictionary<(PolygonTimespan, int), List<Bar>>();

        public void AddBars(List<Bar> bars)
        {
            try
            {
                // Make sure all the bars in the list are of the same type. If mixed, break out each type.
                if (bars.Count == 0)
                    return;

                if (!bars.All(x => x.BarTimespan == bars.First().BarTimespan && x.BarTimespanMultiplier == bars.First().BarTimespanMultiplier))
                {
                    var barTypes = (from bar in bars
                                    select (bar.BarTimespan, bar.BarTimespanMultiplier)).ToList();

                    foreach (var barType in barTypes)
                    {
                        AddBars(bars.Where(x => x.BarTimespan == barType.BarTimespan && x.BarTimespanMultiplier == barType.BarTimespanMultiplier).ToList());
                    }
                }
                else
                {
                    if (barCollection.TryGetValue((bars.First().BarTimespan, bars.First().BarTimespanMultiplier), out List<Bar> barList))
                    {
                        barList.AddRange(bars);
                    }
                    else
                    {
                        barCollection.AddAndReturn((bars.First().BarTimespan, bars.First().BarTimespanMultiplier), new List<Bar>()).AddRange(bars);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void AddBar(Bar bar)
        {
            if (barCollection.TryGetValue((bar.BarTimespan, bar.BarTimespanMultiplier), out List<Bar> barList))
            {
                barList.Add(bar);
            }
            else
            {
                barCollection.AddAndReturn((bar.BarTimespan, bar.BarTimespanMultiplier), new List<Bar>()).Add(bar);
            }
        }
        public List<Bar> GetBars(int timespanMultiplier, PolygonTimespan barTimespan, DateTime start, DateTime end)
        {
            if (barCollection.TryGetValue((barTimespan, timespanMultiplier), out List<Bar> barList))
            {
                return barList.Where(x => x.Timestamp.EST >= start && x.Timestamp.EST < end).ToList();
            }
            else
                return new List<Bar>();
        }
        public List<Bar> GetBars(int timespanMultiplier, PolygonTimespan barTimespan, DateTime day)
        {
            return GetBars(timespanMultiplier, barTimespan, day, day.AddDays(1).AddTicks(-1));
        }
        public List<Bar> GetBars(int timespanMultiplier, PolygonTimespan barTimespan)
        {
            if (barCollection.TryGetValue((barTimespan, timespanMultiplier), out List<Bar> barList))
            {
                return barList.ToList();
            }
            else
                return new List<Bar>();
        }


        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var barList in barCollection)
                sb.Append($"{barList.Key.Item2}{barList.Key.Item1}:{barList.Value.Count} ");
            return sb.ToString();
        }
    }

}
