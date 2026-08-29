using Aprendizaje.Aplicacion.Analytics.Estudio;
using Aprendizaje.Api.Endpoints.Nucleo;
using Aprendizaje.Aplicacion.Nucleo.Usuarios;

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
        Guid? usuarioId,
        IHostEnvironment environment,
        IUsuarioActual usuarioActual,
        ObtenerResumenEstudioCasoUso casoUso,
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

            var resumen = await casoUso.EjecutarAsync(usuario.UsuarioId, cancellationToken);

            return Results.Ok(resumen);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }
}
