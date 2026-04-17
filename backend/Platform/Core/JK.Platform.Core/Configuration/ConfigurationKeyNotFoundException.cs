namespace JK.Platform.Core.Configuration;

public class ConfigurationKeyNotFoundException: Exception
{
    public ConfigurationKeyNotFoundException(string key) : base($"Configuration key '{key}' not found.")
    {
    }
}