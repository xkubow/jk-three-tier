namespace JK.Platform.Http.Models;

public sealed class CorsConfiguration
{
    public const string SectionName = "Cors";

    public string[] AllowedOrigins { get; set; } = Array.Empty<string>();

    public string[] AllowedMethods { get; set; } = new[] { "GET", "POST", "PUT", "DELETE", "PATCH", "OPTIONS" };

    public string[] AllowedHeaders { get; set; } = new[] { "*" };

    public bool AllowCredentials { get; set; }

    public string PolicyName { get; set; } = "DefaultCorsPolicy";
}