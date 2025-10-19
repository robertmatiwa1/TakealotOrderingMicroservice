using FluentAssertions;
using Ordering.Domain;
using Xunit;

namespace Ordering.Domain.Tests
{
    public class OrderTests
    {
        [Fact]
        public void PlaceOrder_Computes_Total_And_Status_Placed()
        {
            var order = Order.Place(Guid.NewGuid(), new List<OrderLine> {
                new OrderLine("SKU-1", 2, Money.Of(100m, "ZAR")),
                new OrderLine("SKU-2", 1, Money.Of(50m, "ZAR"))
            });

            order.Status.Should().Be(OrderStatus.Placed);
            order.Total.Amount.Should().Be(250m);
        }

        [Fact]
        public void Cancel_From_Placed_Succeeds()
        {
            var order = Order.Place(Guid.NewGuid(), new List<OrderLine> {
                new OrderLine("SKU-1", 1, Money.Of(100m, "ZAR"))
            });
            order.Cancel("test");
            order.Status.Should().Be(OrderStatus.Cancelled);
        }
    }
}
