using System.Net;

namespace JK.Platform.Core.Exceptions;

public class ApiExceptionBase: Exception
{
    public string MemberName { get; }
    public HttpStatusCode StatusCode { get; }
    public string ErrorCode { get; }
    public Dictionary<string, string>? ErrorDetails { get; private set; }

    public ApiExceptionBase(string? errorCode, string? message, HttpStatusCode statusCode, string memberName)
    {
        ErrorCode = string.IsNullOrEmpty(errorCode) ? $"{memberName} Failed" : errorCode;
        StatusCode = statusCode;
        MemberName = memberName;
    }
}