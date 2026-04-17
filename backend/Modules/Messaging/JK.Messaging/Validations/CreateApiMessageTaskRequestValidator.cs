using FluentValidation;
using JK.Messaging.Contracts;

namespace JK.Messaging.Validations;

public class CreateApiMessageTaskRequestValidator : AbstractValidator<CreateApiMessageTaskRequest>
{
    public CreateApiMessageTaskRequestValidator()
    {
        RuleFor(x => x.TaskId)
            .NotEmpty().WithMessage("TaskId is required.");

        RuleFor(x => x.TaskName)
            .NotEmpty().WithMessage("TaskName is required.");

        RuleFor(x => x.TargetUrl)
            .NotEmpty().WithMessage("TargetUrl is required.");
    }
}
