using FluentValidation;
using JK.Order.Contracts;

namespace JK.Order.Validations;

public class ListOrdersRequestValidator : AbstractValidator<ListOrdersRequest>
{
    public ListOrdersRequestValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.SortDirection)
            .NotEmpty()
            .Must(d => d.Equals("asc", StringComparison.OrdinalIgnoreCase) ||
                       d.Equals("desc", StringComparison.OrdinalIgnoreCase))
            .WithMessage("SortDirection must be 'asc' or 'desc'.");
    }
}

