using FluentValidation;
using JK.Offer.Contracts;

namespace JK.Offer.Validations;

public class UpdateOfferRequestValidator : AbstractValidator<UpdateOfferRequest>
{
    public UpdateOfferRequestValidator()
    {
        RuleFor(x => x.Status).NotEmpty().MaximumLength(50);
        RuleFor(x => x.TotalAmount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.ExpiresAt).GreaterThan(DateTime.UtcNow);
    }
}
