using FluentValidation;
using JK.Configuration.Contracts;

namespace JK.Configuration.Validations;

public class CreateConfigurationRequestValidator : AbstractValidator<CreateConfigurationRequest>
{
    public CreateConfigurationRequestValidator()
    {
        RuleFor(x => x.Key)
            .NotEmpty().WithMessage("Key is required")
            .MaximumLength(500).WithMessage("Key must not exceed 500 characters");
        RuleFor(x => x.Value).NotEmpty().WithMessage("Value is required");
        RuleFor(x => x.MarketCode).MaximumLength(50).When(x => x.MarketCode != null);
        RuleFor(x => x.ServiceCode).MaximumLength(100).When(x => x.ServiceCode != null);
        RuleFor(x => x.CreatedBy).MaximumLength(200).When(x => x.CreatedBy != null);
    }
}
