using Aprendizaje.Aplicacion.Roadmap.Fases.ActualizarMetadataPedagogica;
using Aprendizaje.Aplicacion.Roadmap.Fases.CrearFase;
using Aprendizaje.Aplicacion.Roadmap.Fases.ListarFases;
using Aprendizaje.Api.Endpoints.Nucleo;
using Aprendizaje.Aplicacion.Nucleo.Usuarios;

namespace Aprendizaje.Api.Endpoints.Roadmap;

public static class FaseEndpoints
{
    public static IEndpointRouteBuilder MapFaseEndpoints(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/fases");

        grupo.MapPost("/", CrearFaseAsync);
        grupo.MapGet("/", ListarFasesAsync);
        grupo.MapPut("/{id:guid}/metadata-pedagogica", ActualizarMetadataPedagogicaAsync);

        return app;
    }

    private static async Task<IResult> CrearFaseAsync(
        CrearFaseHttpRequest request,
        IHostEnvironment environment,
        IUsuarioActual usuarioActual,
        CrearFaseCasoUso casoUso,
        CancellationToken cancellationToken)
    {
        try
        {
            var usuario = await UsuarioHttpContexto.ResolverAsync(
                request.UsuarioId,
                environment,
                usuarioActual,
                cancellationToken);

            if (!usuario.Exitosa)
                return usuario.Error!;

            var resultado = await casoUso.EjecutarAsync(
                new CrearFaseSolicitud(
                    usuario.UsuarioId,
                    request.Nombre,
                    request.Orden,
                    request.Objetivos ?? [],
                    request.CriteriosAvance ?? [],
                    request.MesInicioRecomendado,
                    request.MesFinRecomendado,
                    request.CargaSemanalRecomendada),
                cancellationToken);

            return Results.Json(resultado, statusCode: StatusCodes.Status201Created);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> ListarFasesAsync(
        Guid? usuarioId,
        IHostEnvironment environment,
        IUsuarioActual usuarioActual,
        ListarFasesCasoUso casoUso,
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
                new ListarFasesSolicitud(usuario.UsuarioId),
                cancellationToken);

            return Results.Ok(resultado.Fases);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> ActualizarMetadataPedagogicaAsync(
        Guid id,
        ActualizarMetadataPedagogicaFaseHttpRequest request,
        IHostEnvironment environment,
        IUsuarioActual usuarioActual,
        ActualizarMetadataPedagogicaFaseCasoUso casoUso,
        CancellationToken cancellationToken)
    {
        try
        {
            var usuario = await UsuarioHttpContexto.ResolverAsync(
                request.UsuarioId,
                environment,
                usuarioActual,
                cancellationToken);

            if (!usuario.Exitosa)
                return usuario.Error!;

            var resultado = await casoUso.EjecutarAsync(
                new ActualizarMetadataPedagogicaFaseSolicitud(
                    id,
                    usuario.UsuarioId,
                    request.Objetivos ?? [],
                    request.CriteriosAvance ?? [],
                    request.MesInicioRecomendado,
                    request.MesFinRecomendado,
                    request.CargaSemanalRecomendada),
                cancellationToken);

            return resultado.Estado switch
            {
                ActualizarMetadataPedagogicaFaseEstado.Actualizada => Results.NoContent(),
                ActualizarMetadataPedagogicaFaseEstado.FaseNoEncontrada => Results.NotFound(),
                ActualizarMetadataPedagogicaFaseEstado.UsuarioNoCoincide => Results.Conflict(new
                {
                    error = "La Fase no pertenece al usuario indicado."
                }),
                _ => Results.Problem()
            };
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private sealed record CrearFaseHttpRequest(
        Guid? UsuarioId,
        string Nombre,
        int Orden,
        IReadOnlyCollection<string>? Objetivos,
        IReadOnlyCollection<string>? CriteriosAvance,
        int? MesInicioRecomendado,
        int? MesFinRecomendado,
        string? CargaSemanalRecomendada);

    private sealed record ActualizarMetadataPedagogicaFaseHttpRequest(
        Guid? UsuarioId,
        IReadOnlyCollection<string>? Objetivos,
        IReadOnlyCollection<string>? CriteriosAvance,
        int? MesInicioRecomendado,
        int? MesFinRecomendado,
        string? CargaSemanalRecomendada);
}
