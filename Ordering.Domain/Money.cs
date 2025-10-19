namespace Ordering.Domain
{
    public sealed record Money(decimal Amount, string Currency)
    {
        public static Money Of(decimal amount, string currency) => new(amount, currency);
        public static Money Zero(string currency) => new(0m, currency);
        public Money Add(Money other)
            => Currency == other.Currency ? new Money(Amount + other.Amount, Currency) : throw new InvalidOperationException("Currency mismatch");
    }
}
