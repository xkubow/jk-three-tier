using JK.Platform.Core.Models;

namespace JK.Platform.Core.ExceptionsHandlers;

public interface IExceptionChainHandler
{
    ResponseMessage Handle(Exception exception);
}