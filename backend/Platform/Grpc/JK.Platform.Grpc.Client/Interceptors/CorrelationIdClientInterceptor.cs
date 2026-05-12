using Grpc.Core;
using Grpc.Core.Interceptors;
using JK.Platform.Core.Correlation;
using JK.Platform.Core.DependencyInjection.Attributes;
using JK.Platform.Grpc.Abstraction;
using Microsoft.Extensions.DependencyInjection;

namespace JK.Platform.Grpc.Client.Interceptors;

[Injectable(lifetime: ServiceLifetime.Singleton)]
public sealed class CorrelationIdClientInterceptor(ICorrelationContextAccessor correlationContextAccessor)
    : Interceptor, IClientInterceptor
{
    public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(
        TRequest request,
        ClientInterceptorContext<TRequest, TResponse> context,
        AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
    {
        var nextContext = AddCorrelationHeader(context);
        return continuation(request, nextContext);
    }

    public override TResponse BlockingUnaryCall<TRequest, TResponse>(
        TRequest request,
        ClientInterceptorContext<TRequest, TResponse> context,
        BlockingUnaryCallContinuation<TRequest, TResponse> continuation)
    {
        var nextContext = AddCorrelationHeader(context);
        return continuation(request, nextContext);
    }

    private ClientInterceptorContext<TRequest, TResponse> AddCorrelationHeader<TRequest, TResponse>(
        ClientInterceptorContext<TRequest, TResponse> context)
        where TRequest : class
        where TResponse : class
    {
        var correlationId = correlationContextAccessor.GetOrCreateCorrelationId();
        var headers = CloneHeadersWithoutCorrelationId(context.Options.Headers);
        headers.Add(CorrelationIdConstants.HeaderName, correlationId);

        var options = context.Options.WithHeaders(headers);
        return new ClientInterceptorContext<TRequest, TResponse>(context.Method, context.Host, options);
    }

    private static Metadata CloneHeadersWithoutCorrelationId(Metadata? sourceHeaders)
    {
        var headers = new Metadata();
        if (sourceHeaders is null)
        {
            return headers;
        }

        foreach (var header in sourceHeaders)
        {
            if (header.Key.Equals(CorrelationIdConstants.HeaderName, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (header.IsBinary)
            {
                headers.Add(header.Key, header.ValueBytes);
                continue;
            }

            headers.Add(header.Key, header.Value);
        }

        return headers;
    }
}
