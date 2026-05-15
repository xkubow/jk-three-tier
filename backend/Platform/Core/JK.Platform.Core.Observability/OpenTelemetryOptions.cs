namespace JK.Platform.Core.Observability;

public sealed class OpenTelemetryOptions
{
    public const string SectionName = "OpenTelemetry";

    /// <summary>
    /// When false, OpenTelemetry tracing and OTLP export are not registered (no overhead).
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// OTLP/gRPC endpoint (default port 4317). Override via env <c>OpenTelemetry__OtlpEndpoint</c>.
    /// </summary>
    public string OtlpEndpoint { get; set; } = "http://otel-collector.observability:4317";
}
