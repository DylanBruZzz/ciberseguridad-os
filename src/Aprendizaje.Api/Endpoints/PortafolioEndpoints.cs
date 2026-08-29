using Aprendizaje.Aplicacion.Portafolio;
using Aprendizaje.Api.Endpoints.Nucleo;
using Aprendizaje.Aplicacion.Nucleo.Usuarios;
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
        Guid? usuarioId,
        TipoEvidencePortafolio? tipoEvidence,
        EstadoMadurez? estadoMadurez,
        IHostEnvironment environment,
        IUsuarioActual usuarioActual,
        ObtenerPortafolioCasoUso casoUso,
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

            var resultado = await casoUso.EjecutarAsync(
                new ObtenerPortafolioSolicitud(usuario.UsuarioId, tipoEvidence, estadoMadurez),
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
