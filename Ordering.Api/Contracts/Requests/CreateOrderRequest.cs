namespace Ordering.Api.Contracts.Requests
{
    public record CreateOrderRequest
    (
        Guid CustomerId,
        List<OrderLineItem> Lines
    );

    public record OrderLineItem(string Sku, int Quantity, decimal UnitPrice);
}
