namespace Ordering.Application.Models
{
    public record OrderLineDto(string Sku, int Quantity, decimal UnitPrice);
    public record OrderDto(Guid Id, Guid CustomerId, decimal Total, string Currency, string Status);
}
