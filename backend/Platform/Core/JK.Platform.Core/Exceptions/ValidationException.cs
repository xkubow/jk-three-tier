using System.Net;
using JK.Platform.Core.Models;
using JK.Platform.Core.Validations;

namespace JK.Platform.Core.Exceptions;

public class ValidationException : ApiExceptionBase
{
    public Dictionary<string, ValidationError> ValidationErrors { get; } = new();

    public ValidationException(string errorCode, string message = "Validation failed.")
        : base(errorCode, message, HttpStatusCode.BadRequest, nameof(ValidationException))
    {
    }

    public ValidationException AddValidationError(string name, string errorCode, string message)
    {
        Guard.NotNullAndNotWhiteSpace(name, nameof(name));
        ValidationErrors.Add(name, new ValidationError(errorCode, message));
        return this;
    }
}
