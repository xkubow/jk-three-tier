using System.Net.Mime;
using JK.Playground.Stores;

namespace JK.Playground.Middlewares;

public class IdempotencyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IIdempotencyStore _idempotencyStore;

    private const string IdempotencyHeader = "Idempotency-Key";

    public IdempotencyMiddleware(RequestDelegate next, IIdempotencyStore idempotencyStore)
    {
        _next = next;
        _idempotencyStore = idempotencyStore;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var isIdempotentMethod = new string[] { HttpMethods.Post, HttpMethods.Put, HttpMethods.Delete }.Contains(context.Request.Method);
        if (!isIdempotentMethod)
        {
            await _next(context);
            return;
        }

        var idempotencyKey = context.Request.Headers[IdempotencyHeader];
        if (!idempotencyKey.Any())
        {
            await _next(context);
            return;
        }

        var idempotencyEntry = _idempotencyStore.Get(idempotencyKey.First()!);
        if (idempotencyEntry == null || idempotencyEntry.ExpiresAt < DateTime.UtcNow)
        {
            var originalResponseBody = context.Response.Body;
            try
            {
                using var responseBody = new MemoryStream();
                context.Response.Body = responseBody;

                await _next(context);
                responseBody.Seek(0, SeekOrigin.Begin);
                var responseContent = await new StreamReader(responseBody).ReadToEndAsync();

                _idempotencyStore.Add(idempotencyKey, responseContent, context.Response.StatusCode, context.Response.ContentType ?? MediaTypeNames.Application.Json);

                context.Response.Body = originalResponseBody;
                responseBody.Seek(0, SeekOrigin.Begin);
                await responseBody.CopyToAsync(originalResponseBody);
            }
            finally
            {
                context.Response.Body = originalResponseBody;
            }
        }

        if (idempotencyEntry.Lock)
        {
            context.Response.StatusCode = 409;
            return;
        }

        var isLocked = _idempotencyStore.TrySetLock(idempotencyKey.First()!, true);
        if (!isLocked)
        {
            context.Response.StatusCode = 409;
            return;
        }

        context.Response.StatusCode = idempotencyEntry.StatusCode;
        context.Response.ContentType = idempotencyEntry.ContentType;
        context.Response.ContentLength = idempotencyEntry.ResponseBody.Length;
        await context.Response.WriteAsync(idempotencyEntry.ResponseBody);

    }
}