using Microsoft.Extensions.DependencyInjection;
using Ordering.Application.Interfaces;
using Ordering.Application.Services;

namespace Ordering.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IOrderService, OrderService>();
            return services;
        }
    }
}