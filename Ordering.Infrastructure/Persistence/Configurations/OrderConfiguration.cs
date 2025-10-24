using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ordering.Domain;

namespace Ordering.Infrastructure.Persistence.Configurations
{
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.ToTable("Orders");

            builder.HasKey(o => o.Id);
            
            builder.Property(o => o.Id)
                .ValueGeneratedNever();
                
            builder.Property(o => o.CustomerId)
                .IsRequired();
                
            builder.Property(o => o.Status)
                .HasConversion<string>()
                .IsRequired();
                
            builder.OwnsOne(o => o.Total, moneyBuilder =>
            {
                moneyBuilder.Property(m => m.Amount)
                    .HasColumnName("TotalAmount")
                    .IsRequired();
                    
                moneyBuilder.Property(m => m.Currency)
                    .HasColumnName("Currency")
                    .HasMaxLength(3)
                    .IsRequired();
            });
            
            builder.OwnsMany(o => o.Lines, lineBuilder =>
            {
                lineBuilder.ToTable("OrderLine");
                
                lineBuilder.WithOwner().HasForeignKey("OrderId");
                lineBuilder.Property<Guid>("Id");
                lineBuilder.HasKey("Id");
                
                lineBuilder.Property(l => l.Sku)
                    .IsRequired()
                    .HasMaxLength(100);
                    
                lineBuilder.Property(l => l.Quantity)
                    .IsRequired();
                    
                lineBuilder.OwnsOne(l => l.UnitPrice, moneyBuilder =>
                {
                    moneyBuilder.Property(m => m.Amount)
                        .HasColumnName("UnitPrice")
                        .IsRequired();
                        
                    moneyBuilder.Property(m => m.Currency)
                        .HasColumnName("Currency")
                        .HasMaxLength(3)
                        .IsRequired();
                });
            });
            
            // REMOVE THIS LINE - DomainEvents doesn't exist in the AggregateRoot
            // builder.Ignore(o => o.DomainEvents);
        }
    }
}
