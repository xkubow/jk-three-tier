namespace JK.Platform.Core.Correlation;

public interface ICorrelationContextAccessor
{
    string? CorrelationId { get; }
    string GetOrCreateCorrelationId();
    IDisposable Push(string correlationId);
}