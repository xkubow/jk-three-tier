using FluentValidation;
using JK.Order.Contracts;

namespace JK.Order.Validations;

public class UpdateOrderRequestValidator : AbstractValidator<UpdateOrderRequest>
{
    public UpdateOrderRequestValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty()
            .MaximumLength(50);
    }
}

