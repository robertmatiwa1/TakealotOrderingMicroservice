using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Ordering.Domain.Interfaces;
using Ordering.Infrastructure.Persistence;
using Ordering.Infrastructure.Outbox;

namespace Ordering.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            string connectionString,
            string kafkaBootstrap,
            string outboxTopic)
        {
            // Register DbContext
            services.AddDbContext<OrderingContext>(options =>
                options.UseNpgsql(connectionString));

            // Register repositories
            services.AddScoped<IOrderRepository, OrderingContext>();

            // Register outbox - use the correct type name
            services.AddScoped<IOutboxWriter, OutboxWriter>();

            return services;
        }
    }
}
