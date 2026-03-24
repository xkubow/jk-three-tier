namespace JK.Platform.Rest.Swagger.Models;

public sealed class SwaggerConfiguration
{
    public const string SectionName = "Swagger";

    public bool Enabled { get; set; } = true;

    public string RoutePrefix { get; set; } = "swagger";

    public string Title { get; set; } = "JK API";

    public string Description { get; set; } = "JK API documentation";

    public string ContactName { get; set; } = string.Empty;

    public string ContactEmail { get; set; } = string.Empty;

    public bool EnableAnnotations { get; set; }

    public bool AddJwtBearerSecurity { get; set; }
}