using FluentValidation;
using JK.Offer.Contracts;

namespace JK.Offer.Validations;

public class ListOffersRequestValidator : AbstractValidator<ListOffersRequest>
{
    public ListOffersRequestValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}
