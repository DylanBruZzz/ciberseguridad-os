using Aprendizaje.Aplicacion.Analytics.Competencias;

namespace Aprendizaje.Api.Endpoints;

public static class AnalyticsCompetenciaEndpoints
{
    public static IEndpointRouteBuilder MapAnalyticsCompetenciaEndpoints(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/analytics/competencias");

        grupo.MapGet("/", ListarResumenCompetenciasAsync);

        return app;
    }

    private static async Task<IResult> ListarResumenCompetenciasAsync(
        Guid usuarioId,
        ObtenerResumenCompetenciasCasoUso casoUso,
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
