using Aprendizaje.Api.Endpoints.Nucleo;
using Aprendizaje.Aplicacion.Nucleo.Usuarios;
using Aprendizaje.Aplicacion.Roadmap.Vistas.TemaWorkspaceV1;

namespace Aprendizaje.Api.Endpoints.Roadmap;

public static class TemaWorkspaceEndpoints
{
    public static IEndpointRouteBuilder MapTemaWorkspaceEndpoints(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/temas");

        grupo.MapGet("/{temaId:guid}/workspace", ObtenerWorkspaceAsync);

        return app;
    }

    private static async Task<IResult> ObtenerWorkspaceAsync(
        Guid temaId,
        Guid? usuarioId,
        IHostEnvironment environment,
        IUsuarioActual usuarioActual,
        ObtenerTemaWorkspaceV1CasoUso casoUso,
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
                new ObtenerTemaWorkspaceV1Solicitud(usuario.UsuarioId, temaId),
                cancellationToken);

            return resultado.Encontrado
                ? Results.Ok(resultado.Workspace)
                : Results.NotFound();
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }
}
