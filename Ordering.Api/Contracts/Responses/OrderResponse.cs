namespace Ordering.Api.Contracts.Responses
{
    public record OrderResponse
    {
        public required string Id { get; init; }
        public required string Status { get; init; }
        public required string CustomerId { get; init; }
        public required decimal Total { get; init; }
        public required string Currency { get; init; }
    }
}
