using Microsoft.Extensions.FileProviders;

namespace Aprendizaje.Api.Configuracion;

public static class FrontendHosting
{
    public static WebApplication UseFrontendEstatico(this WebApplication app)
    {
        app.UseDefaultFiles();
        app.UseStaticFiles();

        return app;
    }

    public static IEndpointRouteBuilder MapFrontendFallback(this IEndpointRouteBuilder app)
    {
        app.MapFallback(async context =>
        {
            var path = context.Request.Path;

            if (EsRutaApi(path) || EsArchivo(path))
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                return;
            }

            var environment = context.RequestServices.GetRequiredService<IWebHostEnvironment>();
            var index = environment.WebRootFileProvider.GetFileInfo("index.html");

            if (!index.Exists)
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                return;
            }

            context.Response.ContentType = "text/html; charset=utf-8";
            await context.Response.SendFileAsync(index, context.RequestAborted);
        });

        return app;
    }

    private static bool EsRutaApi(PathString path) =>
        path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase);

    private static bool EsArchivo(PathString path) =>
        Path.HasExtension(path.Value);
}
