using JK.Platform.Core.DependencyInjection.Attributes;
using JK.Platform.Core.Models;
using Microsoft.Extensions.DependencyInjection;

namespace JK.Platform.Core.ExceptionsHandlers;

[MultipleInjectable(lifetime: ServiceLifetime.Singleton)]
public class ExceptionChainHandler(IEnumerable<IPartExceptionHandlerBase> exceptionHandlers) : IExceptionChainHandler
{
    private readonly IEnumerable<IPartExceptionHandlerBase> _exceptionHandlers = exceptionHandlers;
    private readonly DefaultPartExceptionHandler _defaultExceptionHandler = new ();

    public ResponseMessage Handle(Exception exception)
    {
        if(_exceptionHandlers.Any())
        {
            ResponseMessage? responseMessage;
            foreach (var handler in _exceptionHandlers)
            {
                responseMessage = handler.TryHandle(exception);
                if (responseMessage is not null)
                    return responseMessage;
            }
        }

        return _defaultExceptionHandler.Handle(exception);
    }

}