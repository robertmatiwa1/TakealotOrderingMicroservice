// Backward-compatibility wrapper: some legacy code references AppDbContext.
// This inherits OrderingDbContext and should not add legacy property mappings.
using Microsoft.EntityFrameworkCore;

namespace Ordering.Infrastructure.Persistence
{
    public class AppDbContext : OrderingDbContext
    {
        public AppDbContext(DbContextOptions<OrderingDbContext> options) : base(options) { }
    }
}
