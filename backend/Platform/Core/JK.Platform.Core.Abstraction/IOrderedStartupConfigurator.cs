namespace JK.Platform.Core.Abstraction;

public interface IOrderedStartupConfigurator
{
    int Order { get; }
}

