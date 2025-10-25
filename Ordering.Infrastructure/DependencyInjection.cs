using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Ordering.Domain.Interfaces;
using Ordering.Infrastructure.Persistence;
using Ordering.Infrastructure.Outbox;
using Ordering.Infrastructure.Repository;   

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
            // 1. Register resilient EF Core DbContext (PostgreSQL)
            services.AddDbContext<OrderingDbContext>(options =>
            {
                options.UseNpgsql(connectionString, npgsqlOptions =>
                {
                    npgsqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorCodesToAdd: null);
                    npgsqlOptions.CommandTimeout(30);
                });
            });

            // 2. Register repository abstraction
            // (Assumes you have or will add OrderRepository.cs in Ordering.Infrastructure/Repositories)
            services.AddScoped<IOrderRepository, OrderRepository>();

            // 3. Register Outbox writer
            services.AddScoped<IOutboxWriter, OutboxWriter>();

            // (Optional) 4. Add background hosted services, event bus, etc.
            // services.AddHostedService<OutboxDispatcherHostedService>();
            // services.AddSingleton<IEventBus>(sp =>
            //     new KafkaEventBus(kafkaBootstrap, outboxTopic));

            return services;
        }
    }
}
