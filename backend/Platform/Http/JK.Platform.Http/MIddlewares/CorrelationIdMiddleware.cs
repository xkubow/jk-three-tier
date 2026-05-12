using System.Diagnostics;
using JK.Platform.Core.Correlation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace JK.Platform.Http.MIddlewares;

public sealed class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ICorrelationContextAccessor _correlationContextAccessor;
    private readonly ILogger<CorrelationIdMiddleware> _logger;

    public CorrelationIdMiddleware(
        RequestDelegate next,
        ICorrelationContextAccessor correlationContextAccessor,
        ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _correlationContextAccessor = correlationContextAccessor;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var incomingCorrelationId = context.Request.Headers[CorrelationIdConstants.HeaderName].FirstOrDefault();
        var correlationId = CorrelationContextAccessor.NormalizeOrCreate(incomingCorrelationId ?? context.TraceIdentifier);

        context.TraceIdentifier = correlationId;
        context.Response.Headers[CorrelationIdConstants.HeaderName] = correlationId;

        if (Activity.Current is { } activity)
        {
            activity.SetTag(CorrelationIdConstants.ActivityTagName, correlationId);
        }

        using var correlationScope = _correlationContextAccessor.Push(correlationId);
        using (_logger.BeginScope(new Dictionary<string, object?>
               {
                   [CorrelationIdConstants.LogPropertyName] = correlationId
               }))
        {
            await _next(context);
        }
    }
}
