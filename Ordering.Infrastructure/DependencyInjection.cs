using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Ordering.Infrastructure.Persistence;
using Ordering.Infrastructure.Outbox;
using Ordering.Infrastructure.Messaging;

namespace Ordering.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
        {
            services.AddDbContext<AppDbContext>(opts =>
                opts.UseNpgsql(config.GetConnectionString("Default")));

            services.AddScoped<IOutboxWriter, OutboxWriter>();
            services.AddHostedService<OutboxDispatcher>();

            services.AddSingleton<IEventBus, KafkaEventBus>();

            return services;
        }
    }
}
