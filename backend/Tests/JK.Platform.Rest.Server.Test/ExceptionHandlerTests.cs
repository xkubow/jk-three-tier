using System.Net;
using JK.Platform.Core.Exceptions;
using JK.Platform.Core.ExceptionsHandlers;
using JK.Platform.Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace JK.Platform.Rest.Server.Test;

public class ExceptionHandlerTests
{
    private readonly IExceptionChainHandler _exceptionChainHandler;
    private readonly ExceptionHandler _sut;

    public ExceptionHandlerTests()
    {
        var logger = Substitute.For<ILogger<ValidationPartExceptionHandler>>();
        var validationHandler = new ValidationPartExceptionHandler(logger);
        var handlers = new List<IPartExceptionHandlerBase> { validationHandler };
        _exceptionChainHandler = new ExceptionChainHandler(handlers);
        _sut = new ExceptionHandler(_exceptionChainHandler);
    }

    [Fact]
    public async Task TryHandleAsync_ShouldReturnTrue_AndSetResponseDetails_ForGeneralException()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        
        var exception = new Exception("Test exception") { Source = "TestApp" };

        // Act
        var result = await _sut.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.Equal((int)HttpStatusCode.InternalServerError, context.Response.StatusCode);
        Assert.Equal("true", context.Response.Headers["x-server-response"]);
        Assert.Equal("InternalServerError", context.Response.Headers["x-error-code"]);
        
        context.Response.Body.Position = 0;
        using var reader = new StreamReader(context.Response.Body);
        var body = await reader.ReadToEndAsync();
        Assert.Contains("InternalServerError", body);
        Assert.Contains("Test exception", body);
    }

    [Fact]
    public async Task TryHandleAsync_ShouldHandleValidationException()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        
        var exception = new ValidationException("ValError", "Validation failed")
            .AddValidationError("Field1", "Required", "Field is required");

        // Act
        var result = await _sut.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.Equal((int)HttpStatusCode.BadRequest, context.Response.StatusCode);
        Assert.Equal("true", context.Response.Headers["x-server-response"]);
        Assert.Equal("ValidationFailed", context.Response.Headers["x-error-code"]);
        
        context.Response.Body.Position = 0;
        using var reader = new StreamReader(context.Response.Body);
        var body = await reader.ReadToEndAsync();
        Assert.Contains("ValidationFailed", body);
        Assert.Contains("Field1", body);
        Assert.Contains("Field is required", body);
    }
}
