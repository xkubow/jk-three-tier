using FluentValidation;
using JK.Messaging.Contracts;

namespace JK.Messaging.Validations;

public class EchoViaOrleansRequestValidator : AbstractValidator<EchoViaOrleansRequest>
{
    public EchoViaOrleansRequestValidator()
    {
        RuleFor(x => x.ThreadId).NotEmpty();
        RuleFor(x => x.SenderId).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Content).NotEmpty().MaximumLength(8000);
    }
}
