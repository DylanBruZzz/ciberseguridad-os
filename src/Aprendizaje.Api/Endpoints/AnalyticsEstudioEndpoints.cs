using Aprendizaje.Aplicacion.Analytics.Estudio;

namespace Aprendizaje.Api.Endpoints;

public static class AnalyticsEstudioEndpoints
{
    public static IEndpointRouteBuilder MapAnalyticsEstudioEndpoints(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/analytics/estudio");

        grupo.MapGet("/", ObtenerResumenEstudioAsync);

        return app;
    }

    private static async Task<IResult> ObtenerResumenEstudioAsync(
        Guid usuarioId,
        ObtenerResumenEstudioCasoUso casoUso,
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
