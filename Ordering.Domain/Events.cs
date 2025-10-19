namespace Ordering.Domain.Events
{
    public record OrderLineDto(string Sku, int Quantity, decimal UnitPrice);
    public record OrderPlaced(Guid OrderId, Guid CustomerId, decimal Total, string Currency, List<OrderLineDto> Lines);
    public record OrderAccepted(Guid OrderId);
    public record OrderCancelled(Guid OrderId, string Reason);
    public record OrderCompleted(Guid OrderId);
    public record PaymentSucceeded(Guid OrderId, string PaymentRef);
    public record PaymentFailed(Guid OrderId, string Reason);
}
