using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace JK.Platform.Rest.Server.Configurations;

public static class JsonOptionsExtensions
{
    public static void ConfigurePlatformRestJson(this JsonOptions options)
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    }
}