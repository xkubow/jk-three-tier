namespace JK.Platform.Core.AspNetCore.Abstractions;

public interface IOrderedStartupConfigurator
{
    int Order { get; }
}

