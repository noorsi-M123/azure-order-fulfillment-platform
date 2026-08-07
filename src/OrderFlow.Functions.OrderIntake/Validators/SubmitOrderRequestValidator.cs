using FluentValidation;
using OrderFlow.Contracts.Orders;

namespace OrderFlow.Functions.OrderIntake.Validators;

public sealed class SubmitOrderRequestValidator
    : AbstractValidator<SubmitOrderRequest>
{
    public SubmitOrderRequestValidator()
    {
        RuleFor(order => order.OrderId)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(order => order.CustomerId)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(order => order.Items)
            .NotNull()
            .NotEmpty();

        RuleForEach(order => order.Items)
            .SetValidator(new SubmitOrderItemRequestValidator());
    }
}

internal sealed class SubmitOrderItemRequestValidator
    : AbstractValidator<SubmitOrderItemRequest>
{
    public SubmitOrderItemRequestValidator()
    {
        RuleFor(item => item.ProductId)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(item => item.Quantity)
            .GreaterThan(0);

        RuleFor(item => item.UnitPrice)
            .GreaterThanOrEqualTo(0);

        RuleFor(item => item.Currency)
            .NotEmpty()
            .Length(3);
    }
}