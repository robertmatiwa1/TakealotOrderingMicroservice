using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Ordering.Infrastructure.Persistence
{
    /// <summary>
    /// Design-time factory used by EF Core for migrations.
    /// </summary>
    public class OrderingDbContextFactory : IDesignTimeDbContextFactory<OrderingDbContext>
    {
        public OrderingDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<OrderingDbContext>();

            // Use your local PostgreSQL connection string
            optionsBuilder.UseNpgsql("Host=localhost;Database=ordering;Username=postgres;Password=password");

            return new OrderingDbContext(optionsBuilder.Options);
        }
    }
}
