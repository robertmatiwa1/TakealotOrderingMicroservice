using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ordering.Infrastructure.Messaging;
using Ordering.Infrastructure.Persistence;

namespace Ordering.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString, string kafkaBootstrap, string topic)
    {
        services.AddDbContext<OrderingDbContext>(opts => opts.UseNpgsql(connectionString));

        services.Configure<KafkaOptions>(opt =>
        {
            opt.BootstrapServers = kafkaBootstrap;
            opt.DefaultTopic = topic;
        });
        services.AddSingleton<IKafkaProducer, KafkaProducer>();

        return services;
    }
}
