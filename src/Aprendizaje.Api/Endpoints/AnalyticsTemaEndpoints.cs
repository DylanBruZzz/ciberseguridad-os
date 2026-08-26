using Aprendizaje.Aplicacion.Analytics.Temas;

namespace Aprendizaje.Api.Endpoints;

public static class AnalyticsTemaEndpoints
{
    public static IEndpointRouteBuilder MapAnalyticsTemaEndpoints(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/analytics/temas");

        grupo.MapGet("/{temaId:guid}", ObtenerResumenTemaAsync);

        return app;
    }

    private static async Task<IResult> ObtenerResumenTemaAsync(
        Guid temaId,
        Guid usuarioId,
        ObtenerResumenTemaCasoUso casoUso,
        CancellationToken cancellationToken)
    {
        try
        {
            var resultado = await casoUso.EjecutarAsync(
                new ObtenerResumenTemaSolicitud(usuarioId, temaId),
                cancellationToken);

            return resultado.Encontrado
                ? Results.Ok(resultado.Resumen)
                : Results.NotFound();
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }
}
