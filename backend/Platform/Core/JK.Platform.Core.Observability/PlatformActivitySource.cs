using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Reflection;

namespace JK.Platform.Core.Observability;

public class PlatformActivitySource: IDisposable
{
    public string Name { get; }
    public string Version { get; }
    public ActivitySource ActivitySource { get; }
    public Meter Meter { get; }

    public PlatformActivitySource()
    {
        var assemblyName = Assembly.GetCallingAssembly().GetName();

        Name = assemblyName.Name ?? assemblyName.FullName;
        Version = assemblyName.Version?.ToString() ?? "Unknown";

        ActivitySource = new(Name, Version);
        Meter = new Meter(Name, Version);
    }

    public void Dispose()
    {
        ActivitySource.Dispose();
        Meter.Dispose();
    }
}