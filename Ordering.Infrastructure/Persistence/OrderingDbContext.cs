using Microsoft.EntityFrameworkCore;
using Ordering.Domain;

namespace Ordering.Infrastructure.Persistence
{
    public class OrderingDbContext : DbContext
    {
        public OrderingDbContext(DbContextOptions<OrderingDbContext> options) : base(options) { }

        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure Order aggregate
            modelBuilder.Entity<Order>(b =>
            {
                b.HasKey(x => x.Id);
                b.Property(x => x.CustomerId).IsRequired();
                b.Property(x => x.Status).IsRequired();

                // Map the Money value object (Total)
                b.OwnsOne(o => o.Total, money =>
                {
                    money.Property(m => m.Amount)
                        .HasColumnName("TotalAmount")
                        .IsRequired();

                    money.Property(m => m.Currency)
                        .HasColumnName("Currency")
                        .IsRequired()
                        .HasDefaultValue("ZAR");
                });

                // Map the collection of OrderLine value objects
                b.OwnsMany(o => o.Lines, l =>
                {
                    l.WithOwner().HasForeignKey("OrderId");
                    l.Property<Guid>("Id");
                    l.HasKey("Id");

                    l.Property(p => p.Sku)
                        .IsRequired()
                        .HasMaxLength(50);

                    l.Property(p => p.Quantity)
                        .IsRequired();

                    // Map the Money value object (UnitPrice)
                    l.OwnsOne(p => p.UnitPrice, up =>
                    {
                        up.Property(m => m.Amount)
                            .HasColumnName("UnitPriceAmount")
                            .IsRequired();

                        up.Property(m => m.Currency)
                            .HasColumnName("UnitPriceCurrency")
                            .IsRequired();
                    });
                });
            });

            // Configure OutboxMessage for event publishing
            modelBuilder.Entity<OutboxMessage>(b =>
            {
                b.HasKey(x => x.Id);
                b.Property(x => x.Topic).IsRequired();
                b.Property(x => x.Payload).IsRequired();
                b.Property(x => x.OccurredAt).IsRequired();
                b.Property(x => x.DispatchedAt);

                // Legacy aliases (backward compatibility)
                b.Ignore(x => x.Type);
                b.Ignore(x => x.OccurredAtUtc);
                b.Ignore(x => x.PublishedUtc);
            });
        }
    }

    // OutboxMessage for event storage (with legacy alias support)
    public class OutboxMessage
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Topic { get; set; } = "ordering-events";
        public string Payload { get; set; } = string.Empty;
        public DateTimeOffset OccurredAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? DispatchedAt { get; set; }

        // Legacy aliases (safe for backward compatibility)
        public string? Type { get => Topic; set => Topic = value ?? "ordering-events"; }
        public DateTime OccurredAtUtc { get => OccurredAt.UtcDateTime; set => OccurredAt = value; }
        public DateTime? PublishedUtc { get => DispatchedAt?.UtcDateTime; set => DispatchedAt = value; }
    }
}
