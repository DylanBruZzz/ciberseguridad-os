using Aprendizaje.Aplicacion.Analytics.Certificaciones;
using Aprendizaje.Api.Endpoints.Nucleo;
using Aprendizaje.Aplicacion.Nucleo.Usuarios;

namespace Aprendizaje.Api.Endpoints;

public static class AnalyticsCertificacionEndpoints
{
    public static IEndpointRouteBuilder MapAnalyticsCertificacionEndpoints(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/analytics/certificaciones");

        grupo.MapGet("/", ListarResumenCertificacionesAsync);

        return app;
    }

    private static async Task<IResult> ListarResumenCertificacionesAsync(
        Guid? usuarioId,
        IHostEnvironment environment,
        IUsuarioActual usuarioActual,
        ObtenerResumenCertificacionesCasoUso casoUso,
        CancellationToken cancellationToken)
    {
        try
        {
            var usuario = await UsuarioHttpContexto.ResolverAsync(
                usuarioId,
                environment,
                usuarioActual,
                cancellationToken);

            if (!usuario.Exitosa)
                return usuario.Error!;

            var resumen = await casoUso.EjecutarAsync(usuario.UsuarioId, cancellationToken);

            return Results.Ok(resumen);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }
}
