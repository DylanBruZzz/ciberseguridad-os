using Aprendizaje.Api.Endpoints.Nucleo;
using Aprendizaje.Aplicacion.Nucleo.Usuarios;
using Aprendizaje.Aplicacion.Roadmap.Vistas.RoadmapVistaV1;

namespace Aprendizaje.Api.Endpoints.Roadmap;

public static class RoadmapVistaEndpoints
{
    public static IEndpointRouteBuilder MapRoadmapVistaEndpoints(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/roadmap");

        grupo.MapGet("/vista", ObtenerVistaAsync);

        return app;
    }

    private static async Task<IResult> ObtenerVistaAsync(
        Guid? usuarioId,
        IHostEnvironment environment,
        IUsuarioActual usuarioActual,
        ObtenerRoadmapVistaV1CasoUso casoUso,
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

            var vista = await casoUso.EjecutarAsync(
                new ObtenerRoadmapVistaV1Solicitud(usuario.UsuarioId),
                cancellationToken);

            return Results.Ok(vista);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }
}
