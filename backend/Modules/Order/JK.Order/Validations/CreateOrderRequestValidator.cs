using FluentValidation;
using JK.Order.Contracts;

namespace JK.Order.Validations;

public class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
{
    public CreateOrderRequestValidator()
    {
        RuleFor(x => x.Number)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.TotalAmount)
            .GreaterThanOrEqualTo(0);
    }
}

