using FluentValidation;
using Ordering.Api.Contracts.Requests;

namespace Ordering.Api.Validation
{
    public class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
    {
        public CreateOrderRequestValidator()
        {
            RuleFor(x => x.CustomerId).NotEmpty();
            RuleFor(x => x.Lines).NotEmpty();
            RuleForEach(x => x.Lines).SetValidator(new OrderLineItemValidator());
        }
    }

    public class OrderLineItemValidator : AbstractValidator<OrderLineItem>
    {
        public OrderLineItemValidator()
        {
            RuleFor(x => x.Sku).NotEmpty().MaximumLength(64);
            RuleFor(x => x.Quantity).GreaterThan(0);
            RuleFor(x => x.UnitPrice).GreaterThan(0);
        }
    }

    public class CancelOrderRequestValidator : AbstractValidator<CancelOrderRequest>
    {
        public CancelOrderRequestValidator()
        {
            RuleFor(x => x.Reason).MaximumLength(256);
        }
    }
}
