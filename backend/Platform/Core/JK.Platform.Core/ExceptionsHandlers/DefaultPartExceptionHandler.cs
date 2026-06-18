using System.Net;
using JK.Platform.Core.Models;

namespace JK.Platform.Core.ExceptionsHandlers;

public class DefaultPartExceptionHandler
{
    public ResponseMessage Handle(Exception exception)
    {
        return new ResponseMessage(HttpStatusCode.InternalServerError, HttpStatusCode.InternalServerError.ToString(), exception.Message, exception.Source);
    }
}