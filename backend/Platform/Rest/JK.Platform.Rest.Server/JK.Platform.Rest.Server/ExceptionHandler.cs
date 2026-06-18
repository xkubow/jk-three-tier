using JK.Platform.Core.ExceptionsHandlers;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace JK.Platform.Rest.Server;

public class ExceptionHandler: IExceptionHandler
{
    private readonly IExceptionChainHandler _exceptionChainHandler;

    public ExceptionHandler(IExceptionChainHandler exceptionChainHandler)
    {
        _exceptionChainHandler = exceptionChainHandler;
    }
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var responseMessage = _exceptionChainHandler.Handle(exception);
        httpContext.Response.StatusCode = (int)responseMessage.HttpStatusCode;
        httpContext.Response.Headers.TryAdd("x-server-response", "true");
        if (!string.IsNullOrEmpty(responseMessage.ErrorCode))
        {
            httpContext.Response.Headers.TryAdd("x-error-code", responseMessage.ErrorCode);
        }
        await httpContext.Response.WriteAsJsonAsync(responseMessage);

        //TODO log error
        return true;
    }
}