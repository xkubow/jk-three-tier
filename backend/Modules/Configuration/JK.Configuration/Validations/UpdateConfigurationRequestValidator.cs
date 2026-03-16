using FluentValidation;
using JK.Configuration.Contracts;

namespace JK.Configuration.Validations;

public class UpdateConfigurationRequestValidator : AbstractValidator<UpdateConfigurationRequest>
{
    public UpdateConfigurationRequestValidator()
    {
        RuleFor(x => x.Value).NotEmpty().WithMessage("Value is required");
        RuleFor(x => x.UpdatedBy).MaximumLength(200).When(x => x.UpdatedBy != null);
    }
}
