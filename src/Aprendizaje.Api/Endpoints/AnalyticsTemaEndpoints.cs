using Aprendizaje.Aplicacion.Analytics.Temas;
using Aprendizaje.Api.Endpoints.Nucleo;
using Aprendizaje.Aplicacion.Nucleo.Usuarios;

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
        Guid? usuarioId,
        IHostEnvironment environment,
        IUsuarioActual usuarioActual,
        ObtenerResumenTemaCasoUso casoUso,
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
                new ObtenerResumenTemaSolicitud(usuario.UsuarioId, temaId),
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
