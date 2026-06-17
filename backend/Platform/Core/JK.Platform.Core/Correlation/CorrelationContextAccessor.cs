using JK.Platform.Core.DependencyInjection.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace JK.Platform.Core.Correlation;

[Injectable(ServiceLifetime.Singleton)]
public sealed class CorrelationContextAccessor : ICorrelationContextAccessor
{
    private readonly AsyncLocal<CorrelationHolder?> _currentCorrelation = new();

    public string? CorrelationId => _currentCorrelation.Value?.Value;

    public string GetOrCreateCorrelationId()
    {
        if (TryNormalize(CorrelationId, out var existingCorrelationId))
        {
            return existingCorrelationId;
        }

        var correlationId = Guid.NewGuid().ToString("N");
        _currentCorrelation.Value = new CorrelationHolder(correlationId);

        return correlationId;
    }

    public IDisposable Push(string correlationId)
    {
        var previousCorrelationId = CorrelationId;
        _currentCorrelation.Value = new CorrelationHolder(NormalizeOrCreate(correlationId));

        return new RestoreScope(this, previousCorrelationId);
    }

    public static string NormalizeOrCreate(string? candidate)
    {
        return TryNormalize(candidate, out var correlationId)
            ? correlationId
            : Guid.NewGuid().ToString("N");
    }

    private static bool TryNormalize(string? candidate, out string correlationId)
    {
        correlationId = string.Empty;

        if (string.IsNullOrWhiteSpace(candidate))
        {
            return false;
        }

        var normalized = candidate.Trim();

        if (normalized.Length > CorrelationIdConstants.MaxLength)
        {
            return false;
        }

        if (normalized.Any(char.IsControl))
        {
            return false;
        }

        correlationId = normalized;
        return true;
    }

    private sealed class RestoreScope : IDisposable
    {
        private readonly CorrelationContextAccessor _accessor;
        private readonly string? _previousCorrelationId;
        private bool _disposed;

        public RestoreScope(CorrelationContextAccessor accessor, string? previousCorrelationId)
        {
            _accessor = accessor;
            _previousCorrelationId = previousCorrelationId;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _accessor._currentCorrelation.Value = string.IsNullOrWhiteSpace(_previousCorrelationId)
                ? null
                : new CorrelationHolder(_previousCorrelationId);

            _disposed = true;
        }
    }

    private sealed record CorrelationHolder(string Value);
}
