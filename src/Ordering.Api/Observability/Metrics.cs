using System.Diagnostics.Metrics;

namespace Ordering.Api.Observability
{
    public static class Metrics
    {
        private static readonly Meter Meter = new("Ordering.Api");
        public static readonly Counter<long> OrdersCreated = Meter.CreateCounter<long>("orders_created_total");
        public static readonly Counter<long> OrdersCancelled = Meter.CreateCounter<long>("orders_cancelled_total");
    }
}
