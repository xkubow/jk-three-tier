using System.Net;

namespace JK.Platform.Core.Models;

public class ResponseMessage
{
    public HttpStatusCode HttpStatusCode { get; set; }
    public string? ErrorCode { get; set; }
    public List<string>? Messages { get; private set; }
    public Dictionary<string, ValidationError>? ValidationErrors { get; }
    public Dictionary<string, string>? ErrorDetails { get; }

    public ResponseMessage(Dictionary<string, ValidationError> validationErrors)
    {
        ValidationErrors = validationErrors;
        HttpStatusCode = HttpStatusCode.BadRequest;
        ErrorCode = "ValidationFailed";
    }

    public ResponseMessage(HttpStatusCode httpStatusCode, string? errorCode = null, string? message = null, string? source = null)
    {
        HttpStatusCode = httpStatusCode;
        ErrorCode = errorCode;
        Messages = new List<string> { message };
        ErrorDetails = new Dictionary<string, string> { { source, message } };
    }
}