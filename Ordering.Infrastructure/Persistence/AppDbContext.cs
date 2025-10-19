using Microsoft.EntityFrameworkCore;
using Ordering.Domain;

namespace Ordering.Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderLine> OrderLines => Set<OrderLine>();
        public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Order>(b =>
            {
                b.HasKey(o => o.Id);
                b.Property(o => o.CustomerId).IsRequired();
                b.Property(o => o.Status).IsRequired();
                b.OwnsOne(o => o.Total, nb =>
                {
                    nb.Property(p => p.Amount).HasColumnName("TotalAmount");
                    nb.Property(p => p.Currency).HasColumnName("Currency").HasMaxLength(3);
                });
                b.HasMany(o => o.Lines).WithOne().OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<OrderLine>(b =>
            {
                b.HasKey(x => x.Id);
                b.Property(x => x.Sku).IsRequired().HasMaxLength(64);
                b.Property(x => x.Quantity).IsRequired();
                b.OwnsOne(x => x.UnitPrice, nb =>
                {
                    nb.Property(p => p.Amount).HasColumnName("UnitAmount");
                    nb.Property(p => p.Currency).HasColumnName("UnitCurrency").HasMaxLength(3);
                });
            });

            modelBuilder.Entity<OutboxMessage>(b =>
            {
                b.HasKey(x => x.Id);
                b.Property(x => x.Type).IsRequired();
                b.Property(x => x.Payload).IsRequired();
                b.Property(x => x.OccurredAtUtc).IsRequired();
                b.Property(x => x.PublishedUtc);
            });
        }
    }
}
