using FluentValidation;
using JK.Messaging.Contracts;

namespace JK.Messaging.Validations;

public class ListMessagingRequestValidator : AbstractValidator<ListMessagingRequest>
{
    public ListMessagingRequestValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}
