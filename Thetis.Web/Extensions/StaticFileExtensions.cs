using Microsoft.Extensions.FileProviders;

namespace Thetis.Web.Extensions;

public static class StaticFileExtensions
{
    public static IApplicationBuilder UseBrowserStaticFiles(this IApplicationBuilder app, string contentRootPath)
    {
        var browserPath = Path.Combine(contentRootPath, "wwwroot", "browser");

        app.UseDefaultFiles(new DefaultFilesOptions
        {
            FileProvider = new PhysicalFileProvider(browserPath),
            RequestPath = ""
        });

        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(browserPath),
            RequestPath = ""
        });

        return app;
    }
}