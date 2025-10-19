namespace Ordering.Domain
{
    public enum OrderStatus { Placed, Accepted, Cancelled, Completed }

    public sealed class Order : AggregateRoot
    {
        public Guid CustomerId { get; private set; }
        public OrderStatus Status { get; private set; }
        public List<OrderLine> Lines { get; private set; } = new();
        public Money Total { get; private set; } = Money.Zero("ZAR");

        private Order() { }

        public static Order Place(Guid customerId, List<OrderLine> lines)
        {
            if (customerId == Guid.Empty) throw new ArgumentException("Invalid customer");
            if (lines == null || lines.Count == 0) throw new InvalidOperationException("Order must have at least one line");
            var order = new Order
            {
                CustomerId = customerId,
                Status = OrderStatus.Placed,
                Lines = lines
            };
            order.Total = lines.Select(l => l.Total).Aggregate(Money.Zero("ZAR"), (acc, x) => acc.Add(x));
            order.Raise(new Events.OrderPlaced(order.Id, order.CustomerId, order.Total.Amount, order.Total.Currency, order.Lines.Select(l => new Events.OrderLineDto(l.Sku, l.Quantity, l.UnitPrice.Amount)).ToList()));
            return order;
        }

        public void Cancel(string reason)
        {
            if (Status == OrderStatus.Completed || Status == OrderStatus.Cancelled)
                throw new InvalidOperationException("Order cannot be cancelled in its current state.");
            Status = OrderStatus.Cancelled;
            Raise(new Events.OrderCancelled(Id, reason));
        }

        public void Accept()
        {
            if (Status != OrderStatus.Placed) throw new InvalidOperationException("Only placed orders can be accepted.");
            Status = OrderStatus.Accepted;
            Raise(new Events.OrderAccepted(Id));
        }

        public void Complete()
        {
            if (Status != OrderStatus.Accepted) throw new InvalidOperationException("Only accepted orders can be completed.");
            Status = OrderStatus.Completed;
            Raise(new Events.OrderCompleted(Id));
        }
    }

    public sealed class OrderLine
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public string Sku { get; private set; }
        public int Quantity { get; private set; }
        public Money UnitPrice { get; private set; }
        public Money Total => Money.Of(UnitPrice.Amount * Quantity, UnitPrice.Currency);

        private OrderLine() { }

        public OrderLine(string sku, int quantity, Money unitPrice)
        {
            if (string.IsNullOrWhiteSpace(sku)) throw new ArgumentException("SKU required");
            if (quantity <= 0) throw new ArgumentException("Quantity must be >= 1");
            Sku = sku; Quantity = quantity; UnitPrice = unitPrice;
        }
    }
}
