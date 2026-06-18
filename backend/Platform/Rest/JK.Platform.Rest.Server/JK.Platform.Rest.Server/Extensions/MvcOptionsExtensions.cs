using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace JK.Platform.Rest.Server.Extensions;

public static class MvcOptionsExtensions
{
    public static void ConfigurePlatformRestMvc(this MvcOptions options)
    {
        options.RespectBrowserAcceptHeader = false;
        options.OutputFormatters.RemoveType<StringOutputFormatter>();
    }
}