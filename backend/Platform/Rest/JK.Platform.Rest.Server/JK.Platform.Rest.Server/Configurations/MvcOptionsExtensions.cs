using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace JK.Platform.Rest.Server.Configurations;

public static class MvcOptionsExtensions
{
    public static void ConfigurePlatformRestMvc(this MvcOptions options)
    {
        options.RespectBrowserAcceptHeader = false;
        options.OutputFormatters.RemoveType<StringOutputFormatter>();
    }
}