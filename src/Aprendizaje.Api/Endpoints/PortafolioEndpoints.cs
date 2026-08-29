using Aprendizaje.Aplicacion.Portafolio;
using Aprendizaje.Dominio.Evidence;

namespace Aprendizaje.Api.Endpoints;

public static class PortafolioEndpoints
{
    public static IEndpointRouteBuilder MapPortafolioEndpoints(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/portafolio");

        grupo.MapGet("/", ObtenerPortafolioAsync);

        return app;
    }

    private static async Task<IResult> ObtenerPortafolioAsync(
        Guid usuarioId,
        TipoEvidencePortafolio? tipoEvidence,
        EstadoMadurez? estadoMadurez,
        ObtenerPortafolioCasoUso casoUso,
        CancellationToken cancellationToken)
    {
        try
        {
            var resultado = await casoUso.EjecutarAsync(
                new ObtenerPortafolioSolicitud(usuarioId, tipoEvidence, estadoMadurez),
                cancellationToken);

            return resultado.Encontrado
                ? Results.Ok(resultado.Portafolio)
                : Results.NotFound();
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }
}
