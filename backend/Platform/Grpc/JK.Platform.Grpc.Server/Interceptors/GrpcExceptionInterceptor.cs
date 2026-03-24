using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.Logging;

namespace JK.Platform.Grpc.Server.Interceptors;

public class GrpcExceptionInterceptor : Interceptor
{
    private readonly ILogger<GrpcExceptionInterceptor> _logger;

    public GrpcExceptionInterceptor(ILogger<GrpcExceptionInterceptor> logger)
    {
        _logger = logger;
    }

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        try
        {
            return await continuation(request, context);
        }
        catch (InvalidOperationException ex) when (IsTransientDbFailure(ex))
        {
            _logger.LogError(ex, "A transient database failure occurred while executing gRPC method {Method}", context.Method);
            throw new RpcException(new Status(StatusCode.Unavailable, "Database service is temporarily unavailable. Please try again later."));
        }
        catch (RpcException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred while executing gRPC method {Method}", context.Method);
            throw new RpcException(new Status(StatusCode.Internal, "Internal server error."));
        }
    }

    private static bool IsTransientDbFailure(Exception ex)
    {
        // Based on the issue description, the exception is InvalidOperationException with a message about transient failure
        // and contains NpgsqlException -> TimeoutException
        return ex.Message.Contains("likely due to a transient failure") || 
               ex.InnerException is { Message: var innerMsg } && innerMsg.Contains("Failed to connect");
    }
}
