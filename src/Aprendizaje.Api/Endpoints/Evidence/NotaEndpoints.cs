using Aprendizaje.Aplicacion.Evidence.Notas.Comun;
using Aprendizaje.Aplicacion.Evidence.Notas.CrearNotaParaArtefactoTecnico;
using Aprendizaje.Aplicacion.Evidence.Notas.CrearNotaParaLaboratorio;
using Aprendizaje.Aplicacion.Evidence.Notas.CrearNotaParaProyecto;
using Aprendizaje.Aplicacion.Evidence.Notas.CrearNotaParaTema;
using Aprendizaje.Aplicacion.Evidence.Notas.CrearNotaParaWriteup;
using Aprendizaje.Aplicacion.Evidence.Notas.ListarNotas;
using Aprendizaje.Aplicacion.Evidence.Notas.ObtenerNotaPorId;
using Aprendizaje.Dominio.Evidence;

namespace Aprendizaje.Api.Endpoints.Evidence;

public static class NotaEndpoints
{
    public static IEndpointRouteBuilder MapNotaEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/temas/{temaId:guid}/notas", CrearNotaParaTemaAsync);
        app.MapPost("/api/proyectos/{proyectoId:guid}/notas", CrearNotaParaProyectoAsync);
        app.MapPost("/api/laboratorios/{laboratorioId:guid}/notas", CrearNotaParaLaboratorioAsync);
        app.MapPost("/api/writeups/{writeupId:guid}/notas", CrearNotaParaWriteupAsync);
        app.MapPost("/api/artefactos-tecnicos/{artefactoTecnicoId:guid}/notas", CrearNotaParaArtefactoTecnicoAsync);

        var grupo = app.MapGroup("/api/notas");

        grupo.MapGet("/", ListarNotasAsync);
        grupo.MapGet("/{id:guid}", ObtenerNotaPorIdAsync);

        return app;
    }

    private static async Task<IResult> CrearNotaParaTemaAsync(
        Guid temaId,
        CrearNotaHttpRequest request,
        CrearNotaParaTemaCasoUso casoUso,
        CancellationToken cancellationToken)
    {
        try
        {
            var resultado = await casoUso.EjecutarAsync(
                new CrearNotaParaTemaSolicitud(request.UsuarioId, temaId, request.Texto, request.Tipo),
                cancellationToken);

            return MapearCreacion(resultado);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> CrearNotaParaProyectoAsync(
        Guid proyectoId,
        CrearNotaHttpRequest request,
        CrearNotaParaProyectoCasoUso casoUso,
        CancellationToken cancellationToken)
    {
        try
        {
            var resultado = await casoUso.EjecutarAsync(
                new CrearNotaParaProyectoSolicitud(request.UsuarioId, proyectoId, request.Texto, request.Tipo),
                cancellationToken);

            return MapearCreacion(resultado);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> CrearNotaParaLaboratorioAsync(
        Guid laboratorioId,
        CrearNotaHttpRequest request,
        CrearNotaParaLaboratorioCasoUso casoUso,
        CancellationToken cancellationToken)
    {
        try
        {
            var resultado = await casoUso.EjecutarAsync(
                new CrearNotaParaLaboratorioSolicitud(request.UsuarioId, laboratorioId, request.Texto, request.Tipo),
                cancellationToken);

            return MapearCreacion(resultado);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> CrearNotaParaWriteupAsync(
        Guid writeupId,
        CrearNotaHttpRequest request,
        CrearNotaParaWriteupCasoUso casoUso,
        CancellationToken cancellationToken)
    {
        try
        {
            var resultado = await casoUso.EjecutarAsync(
                new CrearNotaParaWriteupSolicitud(request.UsuarioId, writeupId, request.Texto, request.Tipo),
                cancellationToken);

            return MapearCreacion(resultado);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> CrearNotaParaArtefactoTecnicoAsync(
        Guid artefactoTecnicoId,
        CrearNotaHttpRequest request,
        CrearNotaParaArtefactoTecnicoCasoUso casoUso,
        CancellationToken cancellationToken)
    {
        try
        {
            var resultado = await casoUso.EjecutarAsync(
                new CrearNotaParaArtefactoTecnicoSolicitud(
                    request.UsuarioId,
                    artefactoTecnicoId,
                    request.Texto,
                    request.Tipo),
                cancellationToken);

            return MapearCreacion(resultado);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> ObtenerNotaPorIdAsync(
        Guid id,
        ObtenerNotaPorIdCasoUso casoUso,
        CancellationToken cancellationToken)
    {
        try
        {
            var resultado = await casoUso.EjecutarAsync(id, cancellationToken);

            return resultado.Encontrada
                ? Results.Ok(resultado.Nota)
                : Results.NotFound();
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> ListarNotasAsync(
        Guid usuarioId,
        ListarNotasCasoUso casoUso,
        CancellationToken cancellationToken)
    {
        try
        {
            var resultado = await casoUso.EjecutarAsync(new ListarNotasSolicitud(usuarioId), cancellationToken);

            return Results.Ok(resultado.Notas);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static IResult MapearCreacion(CrearNotaResultado resultado) =>
        resultado.Estado switch
        {
            CrearNotaEstado.Creada => Results.Created($"/api/notas/{resultado.Id}", resultado),
            CrearNotaEstado.PadreNoEncontrado => Results.NotFound(),
            CrearNotaEstado.UsuarioNoCoincide => Results.Conflict(new
            {
                error = "La Nota y su padre deben pertenecer al mismo Usuario."
            }),
            _ => Results.Problem("Estado de creación de Nota no reconocido.")
        };

    private sealed record CrearNotaHttpRequest(Guid UsuarioId, string Texto, TipoNota Tipo = TipoNota.Nota);
}
