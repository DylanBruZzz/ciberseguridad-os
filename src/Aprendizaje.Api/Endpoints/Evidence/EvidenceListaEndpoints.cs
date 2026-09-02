using Aprendizaje.Api.Endpoints.Nucleo;
using Aprendizaje.Aplicacion.Evidence.Vistas.EvidenceListaV1;
using Aprendizaje.Aplicacion.Nucleo.Usuarios;
using Aprendizaje.Dominio.Evidence;

namespace Aprendizaje.Api.Endpoints.Evidence;

public static class EvidenceListaEndpoints
{
    public static IEndpointRouteBuilder MapEvidenceListaEndpoints(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/evidence");

        grupo.MapGet("/", ObtenerEvidenceAsync);

        return app;
    }

    private static async Task<IResult> ObtenerEvidenceAsync(
        Guid? usuarioId,
        TipoEvidenceV1? tipoEvidence,
        EstadoMadurez? estadoMadurez,
        Guid? temaId,
        IHostEnvironment environment,
        IUsuarioActual usuarioActual,
        ObtenerEvidenceListaV1CasoUso casoUso,
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
                new ObtenerEvidenceListaV1Solicitud(
                    usuario.UsuarioId,
                    tipoEvidence,
                    estadoMadurez,
                    temaId),
                cancellationToken);

            return resultado.Encontrado
                ? Results.Ok(resultado.Evidence)
                : Results.NotFound();
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }
}
