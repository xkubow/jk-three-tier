using FluentValidation;
using JK.Offer.Contracts;

namespace JK.Offer.Validations;

public class CreateOfferRequestValidator : AbstractValidator<CreateOfferRequest>
{
    public CreateOfferRequestValidator()
    {
        RuleFor(x => x.Number).NotEmpty().MaximumLength(100);
        RuleFor(x => x.TotalAmount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.ExpiresAt).GreaterThan(DateTime.UtcNow);
    }
}
