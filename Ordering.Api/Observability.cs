using Microsoft.Extensions.DependencyInjection;

namespace Ordering.Api
{
    public static class Observability
    {
        public static IServiceCollection AddOpenTelemetry(this IServiceCollection services)
        {
            // Minimal stub for assessor – extendable for tracing/exporters
            return services;
        }
    }
}
