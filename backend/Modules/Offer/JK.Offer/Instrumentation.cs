using System.Diagnostics;
using System.Diagnostics.Metrics;
using JK.Platform.Core.Observability;

namespace JK.Offer;

public static class Instrumentation
{
    private static readonly PlatformActivitySource _instrumentation;
    public static ActivitySource ActivitySource => _instrumentation.ActivitySource;
    public static Meter Meter => _instrumentation.Meter;
    public static Counter<long> TestCounter { get; }

    static Instrumentation()
    {
        _instrumentation = new PlatformActivitySource();
        TestCounter = Meter.CreateCounter<long>("TestCounter", "Test Counter");
    }
}