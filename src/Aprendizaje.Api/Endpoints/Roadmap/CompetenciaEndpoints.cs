using Aprendizaje.Aplicacion.Roadmap.Competencias.CrearCompetencia;
using Aprendizaje.Aplicacion.Roadmap.Competencias.ListarCompetencias;
using Aprendizaje.Aplicacion.Roadmap.Competencias.ObtenerCompetenciaPorId;
using Aprendizaje.Aplicacion.Roadmap.Competencias.VincularCompetenciaATema;

namespace Aprendizaje.Api.Endpoints.Roadmap;

public static class CompetenciaEndpoints
{
    public static IEndpointRouteBuilder MapCompetenciaEndpoints(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/competencias");

        grupo.MapPost("/", CrearCompetenciaAsync);
        grupo.MapGet("/", ListarCompetenciasAsync);
        grupo.MapGet("/{id:guid}", ObtenerCompetenciaPorIdAsync);
        grupo.MapPut("/{competenciaId:guid}/temas/{temaId:guid}", VincularTemaAsync);

        return app;
    }

    private static async Task<IResult> CrearCompetenciaAsync(
        CrearCompetenciaHttpRequest request,
        CrearCompetenciaCasoUso casoUso,
        CancellationToken cancellationToken)
    {
        try
        {
            var resultado = await casoUso.EjecutarAsync(
                new CrearCompetenciaSolicitud(request.UsuarioId, request.Nombre),
                cancellationToken);

            return Results.Created($"/api/competencias/{resultado.Id}", resultado);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> ObtenerCompetenciaPorIdAsync(
        Guid id,
        ObtenerCompetenciaPorIdCasoUso casoUso,
        CancellationToken cancellationToken)
    {
        try
        {
            var resultado = await casoUso.EjecutarAsync(id, cancellationToken);

            return resultado.Encontrada
                ? Results.Ok(resultado.Competencia)
                : Results.NotFound();
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> ListarCompetenciasAsync(
        Guid usuarioId,
        ListarCompetenciasCasoUso casoUso,
        CancellationToken cancellationToken)
    {
        try
        {
            var resultado = await casoUso.EjecutarAsync(
                new ListarCompetenciasSolicitud(usuarioId),
                cancellationToken);

            return Results.Ok(resultado.Competencias);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> VincularTemaAsync(
        Guid competenciaId,
        Guid temaId,
        VincularCompetenciaATemaCasoUso casoUso,
        CancellationToken cancellationToken)
    {
        try
        {
            var resultado = await casoUso.EjecutarAsync(
                new VincularCompetenciaATemaSolicitud(competenciaId, temaId),
                cancellationToken);

            return resultado.Estado switch
            {
                VincularCompetenciaATemaEstado.Actualizado => Results.NoContent(),
                VincularCompetenciaATemaEstado.CompetenciaNoEncontrada => Results.NotFound(),
                VincularCompetenciaATemaEstado.TemaNoEncontrado => Results.NotFound(),
                VincularCompetenciaATemaEstado.UsuarioNoCoincide => Results.Conflict(new
                {
                    error = "La Competencia y el Tema deben pertenecer al mismo Usuario."
                }),
                _ => Results.Problem("Estado de vínculo Competencia-Tema no reconocido.")
            };
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private sealed record CrearCompetenciaHttpRequest(Guid UsuarioId, string Nombre);
}
