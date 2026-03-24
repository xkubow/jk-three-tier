namespace JK.Platform.Core.AspNetCore.Abstractions;

public interface IOrderedHostConfigurator
{
    int Order { get; }
}
