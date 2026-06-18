using JK.Platform.Core.Models;

namespace JK.Platform.Core.ExceptionsHandlers;

public interface IPartExceptionHandlerBase
{
    ResponseMessage? TryHandle(Exception exception);
}