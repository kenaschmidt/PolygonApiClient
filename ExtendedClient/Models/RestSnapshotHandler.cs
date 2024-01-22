namespace PolygonApiClient.ExtendedClient
{
    public class RestSnapshotHandler
    {
        public double SecondsInterval => timer.Interval;
        private System.Timers.Timer timer { get; }

        public RestSnapshotHandler(System.Timers.Timer callbackTimer)
        {
            timer = callbackTimer;
        }

        public void Stop()
        {
            timer.Stop();
        }
    }

}
