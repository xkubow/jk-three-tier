using FluentValidation;
using JK.Messaging.Contracts;

namespace JK.Messaging.Validations;

public class UpdateMessagingRequestValidator : AbstractValidator<UpdateMessagingRequest>
{
    public UpdateMessagingRequestValidator()
    {
        RuleFor(x => x.Status).NotEmpty().MaximumLength(50);
    }
}
