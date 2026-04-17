using FluentValidation;
using JK.Messaging.Contracts;

namespace JK.Messaging.Validations;

public class CreateMessagingRequestValidator : AbstractValidator<CreateMessagingRequest>
{
    public CreateMessagingRequestValidator()
    {
        RuleFor(x => x.ThreadId).NotEmpty();
        RuleFor(x => x.SenderId).NotEmpty();
        RuleFor(x => x.Content).NotEmpty().MaximumLength(8000);
    }
}
