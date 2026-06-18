using JK.Platform.Core.DependencyInjection.Attributes;
using JK.Platform.Core.Exceptions;
using JK.Platform.Core.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace JK.Platform.Core.ExceptionsHandlers;

[MultipleInjectable(lifetime: ServiceLifetime.Singleton)]
public class ValidationPartExceptionHandler: PartExceptionHandlerBase<ValidationException, ValidationPartExceptionHandler>
{
    public ValidationPartExceptionHandler(ILogger<ValidationPartExceptionHandler> logger) : base(logger)
    {
    }

    public override ResponseMessage Handle(ValidationException exception) => new ResponseMessage(exception.ValidationErrors);
}