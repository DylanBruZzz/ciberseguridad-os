using Aprendizaje.Aplicacion.Analytics.Competencias;
using Aprendizaje.Api.Endpoints.Nucleo;
using Aprendizaje.Aplicacion.Nucleo.Usuarios;

namespace Aprendizaje.Api.Endpoints;

public static class AnalyticsCompetenciaEndpoints
{
    public static IEndpointRouteBuilder MapAnalyticsCompetenciaEndpoints(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/analytics/competencias");

        grupo.MapGet("/", ListarResumenCompetenciasAsync);

        return app;
    }

    private static async Task<IResult> ListarResumenCompetenciasAsync(
        Guid? usuarioId,
        IHostEnvironment environment,
        IUsuarioActual usuarioActual,
        ObtenerResumenCompetenciasCasoUso casoUso,
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
