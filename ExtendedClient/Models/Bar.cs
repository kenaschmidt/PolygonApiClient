using PolygonApiClient.Helpers;
using System;
using System.Collections.Generic;

namespace PolygonApiClient.ExtendedClient
{
    public class Bar : IEquatable<Bar>
    {
        protected IPolygonBar barBase { get; }

        public PolygonTimestamp Timestamp { get; }

        private double _Open { get; set; }
        public double Open => barBase?.Open ?? _Open;

        private double _Close { get; set; }
        public double Close => barBase?.Close ?? _Close;

        private double _High { get; set; }
        public double High => barBase?.High ?? _High;

        private double _Low { get; set; }
        public double Low => barBase?.Low ?? _Low;

        private long _Volume { get; set; }
        public long Volume => barBase?.Volume ?? _Volume;

        public double Change => (Close - Open);
        public double ChangePercent => (Change / Open);
        public double Range => (High - Low);

        public PolygonTimespan BarTimespan { get; }
        public int BarTimespanMultiplier { get; }

        public Bar(IPolygonBar barBase, PolygonTimespan barTimespan, int barTimespanMultiplier)
        {
            this.barBase = barBase;
            BarTimespan = barTimespan;
            BarTimespanMultiplier = barTimespanMultiplier;
            Timestamp = PolygonTimestamp.FromMilliseconds(barBase.Timestamp_Start_Ms);
        }

        public Bar(DateTime barStart, double open, double high, double low, double close, long volume, PolygonTimespan barTimespan, int barTimespanMultiplier)
        {
            Timestamp = PolygonTimestamp.FromEstDateTime(barStart);
            _Open = open;
            _High = high;
            _Low = low;
            _Close = close;
            _Volume = volume;
            BarTimespan = barTimespan;
            BarTimespanMultiplier = barTimespanMultiplier;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Bar);
        }
        public bool Equals(Bar other)
        {
            return !(other is null) &&
                   EqualityComparer<PolygonTimestamp>.Default.Equals(Timestamp, other.Timestamp) &&
                   BarTimespan == other.BarTimespan &&
                   BarTimespanMultiplier == other.BarTimespanMultiplier;
        }
        public override int GetHashCode()
        {
            int hashCode = -1397146322;
            hashCode = hashCode * -1521134295 + EqualityComparer<PolygonTimestamp>.Default.GetHashCode(Timestamp);
            hashCode = hashCode * -1521134295 + BarTimespan.GetHashCode();
            hashCode = hashCode * -1521134295 + BarTimespanMultiplier.GetHashCode();
            return hashCode;
        }
        public static bool operator ==(Bar left, Bar right)
        {
            return EqualityComparer<Bar>.Default.Equals(left, right);
        }
        public static bool operator !=(Bar left, Bar right)
        {
            return !(left == right);
        }
    }

}
