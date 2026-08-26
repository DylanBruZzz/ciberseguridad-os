using Aprendizaje.Aplicacion.Analytics.Certificaciones;

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
        Guid usuarioId,
        ObtenerResumenCertificacionesCasoUso casoUso,
        CancellationToken cancellationToken)
    {
        try
        {
            var resumen = await casoUso.EjecutarAsync(usuarioId, cancellationToken);

            return Results.Ok(resumen);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }
}
