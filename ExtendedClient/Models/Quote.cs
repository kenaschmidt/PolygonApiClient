using PolygonApiClient.Helpers;
using System;
using System.Diagnostics;

namespace PolygonApiClient.ExtendedClient
{
    public class Quote : Tick
    {
        protected IPolygonQuote TickBase { get; }

        public double BidPrice => TickBase?.Bid_Price ?? 0;
        public long BidSize => TickBase?.Bid_Size ?? 0;
        public double AskPrice => TickBase?.Ask_Price ?? 0;
        public double AskSize => TickBase?.Ask_Size ?? 0;
        public double Spread => (AskPrice - BidPrice);
        public double MidpointPrice => (AskPrice + BidPrice) / 2.0;

        private Quote() { }

        public Quote(IPolygonQuote polygonQuoteBase)
        {
            //
            // Need to differentiate what type of tick we are getting due to timestamp accuracy differences
            //
            TickBase = polygonQuoteBase;

            if (polygonQuoteBase is RestQuotes_Result q1)
                Timestamp = PolygonTimestamp.FromNanoseconds(q1.SIP_Timestamp_Ns);
            else if (polygonQuoteBase is RestLastQuote_Result q2)
                Timestamp = PolygonTimestamp.FromNanoseconds(q2.SIP_Timestamp_Ns);
            else if (polygonQuoteBase is Socket_Quote q3)
                Timestamp = PolygonTimestamp.FromMilliseconds(q3.Timestamp_Ms);
            else
                throw new ArgumentException($"Unable to create quote from object {polygonQuoteBase.GetType().Name}");
        }

        public static Quote EmptyQuote()
        {
            Debug.WriteLine($"*** WARNING: Empty quote generated ***");

            return new Quote() { Timestamp = PolygonTimestamp.EmptyTimestamp() };
        }
    }

}
