using Aprendizaje.Api.Endpoints.Nucleo;
using Aprendizaje.Aplicacion.Nucleo.Usuarios;
using Aprendizaje.Aplicacion.Roadmap.Temas.DesvincularHerramientaDeTema;
using Aprendizaje.Aplicacion.Roadmap.Temas.VincularHerramientaATema;

namespace Aprendizaje.Api.Endpoints.Roadmap;

public static class TemaHerramientaEndpoints
{
    public static IEndpointRouteBuilder MapTemaHerramientaEndpoints(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/temas");

        grupo.MapPut("/{temaId:guid}/herramientas/{herramientaId:guid}", VincularHerramientaAsync);
        grupo.MapDelete("/{temaId:guid}/herramientas/{herramientaId:guid}", DesvincularHerramientaAsync);

        return app;
    }

    private static async Task<IResult> VincularHerramientaAsync(
        Guid temaId,
        Guid herramientaId,
        Guid? usuarioId,
        IHostEnvironment environment,
        IUsuarioActual usuarioActual,
        VincularHerramientaATemaCasoUso casoUso,
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
                new VincularHerramientaATemaSolicitud(usuario.UsuarioId, temaId, herramientaId),
                cancellationToken);

            return resultado.Estado switch
            {
                VincularHerramientaATemaEstado.Actualizado => Results.NoContent(),
                VincularHerramientaATemaEstado.TemaNoEncontrado => Results.NotFound(),
                VincularHerramientaATemaEstado.HerramientaNoEncontrada => Results.NotFound(),
                VincularHerramientaATemaEstado.UsuarioNoCoincide => ResultadoOwnershipNoCoincide(environment),
                _ => Results.Problem("Estado de vínculo Tema-Herramienta no reconocido.")
            };
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> DesvincularHerramientaAsync(
        Guid temaId,
        Guid herramientaId,
        Guid? usuarioId,
        IHostEnvironment environment,
        IUsuarioActual usuarioActual,
        DesvincularHerramientaDeTemaCasoUso casoUso,
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
                new DesvincularHerramientaDeTemaSolicitud(usuario.UsuarioId, temaId, herramientaId),
                cancellationToken);

            return resultado.Estado switch
            {
                DesvincularHerramientaDeTemaEstado.Actualizado => Results.NoContent(),
                DesvincularHerramientaDeTemaEstado.TemaNoEncontrado => Results.NotFound(),
                DesvincularHerramientaDeTemaEstado.HerramientaNoEncontrada => Results.NotFound(),
                DesvincularHerramientaDeTemaEstado.UsuarioNoCoincide => ResultadoOwnershipNoCoincide(environment),
                _ => Results.Problem("Estado de desvinculación Tema-Herramienta no reconocido.")
            };
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static IResult ResultadoOwnershipNoCoincide(IHostEnvironment environment) =>
        environment.IsEnvironment("Personal")
            ? Results.NotFound()
            : Results.Conflict(new { error = "El Tema debe pertenecer al Usuario indicado." });
}
