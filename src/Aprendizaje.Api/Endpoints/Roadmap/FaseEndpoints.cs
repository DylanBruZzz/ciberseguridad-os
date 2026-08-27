using Aprendizaje.Aplicacion.Roadmap.Fases.ActualizarMetadataPedagogica;
using Aprendizaje.Aplicacion.Roadmap.Fases.CrearFase;
using Aprendizaje.Aplicacion.Roadmap.Fases.ListarFases;

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
        CrearFaseCasoUso casoUso,
        CancellationToken cancellationToken)
    {
        try
        {
            var resultado = await casoUso.EjecutarAsync(
                new CrearFaseSolicitud(
                    request.UsuarioId,
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
        Guid usuarioId,
        ListarFasesCasoUso casoUso,
        CancellationToken cancellationToken)
    {
        try
        {
            var resultado = await casoUso.EjecutarAsync(
                new ListarFasesSolicitud(usuarioId),
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
        ActualizarMetadataPedagogicaFaseCasoUso casoUso,
        CancellationToken cancellationToken)
    {
        try
        {
            var resultado = await casoUso.EjecutarAsync(
                new ActualizarMetadataPedagogicaFaseSolicitud(
                    id,
                    request.UsuarioId,
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
        Guid UsuarioId,
        string Nombre,
        int Orden,
        IReadOnlyCollection<string>? Objetivos,
        IReadOnlyCollection<string>? CriteriosAvance,
        int? MesInicioRecomendado,
        int? MesFinRecomendado,
        string? CargaSemanalRecomendada);

    private sealed record ActualizarMetadataPedagogicaFaseHttpRequest(
        Guid UsuarioId,
        IReadOnlyCollection<string>? Objetivos,
        IReadOnlyCollection<string>? CriteriosAvance,
        int? MesInicioRecomendado,
        int? MesFinRecomendado,
        string? CargaSemanalRecomendada);
}
