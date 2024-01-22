using PolygonApiClient.Helpers;

namespace PolygonApiClient.ExtendedClient
{
    public abstract class Tick
    {
        public PolygonTimestamp Timestamp { get; protected set; }

        protected Tick()
        {
        }
    }

}
