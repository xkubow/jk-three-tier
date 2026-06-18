using JK.Platform.Core.Models;
using Microsoft.Extensions.Logging;

namespace JK.Platform.Core.ExceptionsHandlers;

public abstract class PartExceptionHandlerBase<TException, THandler>: IPartExceptionHandlerBase
    where THandler : PartExceptionHandlerBase<TException, THandler>
    where TException : Exception
{
    private readonly ILogger<THandler> _logger;

    public PartExceptionHandlerBase(ILogger<THandler> logger)
    {
        _logger = logger;
    }

    public abstract ResponseMessage Handle(TException exception);

    public ResponseMessage? TryHandle(Exception exception)
    {
        if (exception is not TException specificException)
            return null;

        return Handle(specificException);
    }
}