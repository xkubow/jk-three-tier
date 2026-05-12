namespace JK.Platform.Core.Correlation;

public static class CorrelationIdConstants
{
    public const string HeaderName = "X-Correlation-ID";
    public const string LogPropertyName = "CorrelationId";
    public const string ActivityTagName = "correlation.id";
    public const int MaxLength = 128;
}
