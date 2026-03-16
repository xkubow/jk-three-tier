using FluentValidation;
using JK.Configuration.Contracts;

namespace JK.Configuration.Validations;

public class ListConfigurationRequestValidator : AbstractValidator<ListConfigurationRequest>
{
    public ListConfigurationRequestValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0).WithMessage("Page must be at least 1");
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100).WithMessage("PageSize must be between 1 and 100");
        RuleFor(x => x.SortDirection)
            .Must(d => d == "asc" || d == "desc")
            .When(x => !string.IsNullOrEmpty(x.SortDirection))
            .WithMessage("SortDirection must be 'asc' or 'desc'");
    }
}
