using Aprendizaje.Aplicacion.Evidence.Notas.Comun;
using Aprendizaje.Aplicacion.Evidence.Notas.CrearNotaParaArtefactoTecnico;
using Aprendizaje.Aplicacion.Evidence.Notas.CrearNotaParaLaboratorio;
using Aprendizaje.Aplicacion.Evidence.Notas.CrearNotaParaProyecto;
using Aprendizaje.Aplicacion.Evidence.Notas.CrearNotaParaTema;
using Aprendizaje.Aplicacion.Evidence.Notas.CrearNotaParaWriteup;
using Aprendizaje.Aplicacion.Evidence.Notas.ListarNotas;
using Aprendizaje.Aplicacion.Evidence.Notas.ObtenerNotaPorId;
using Aprendizaje.Api.Endpoints.Nucleo;
using Aprendizaje.Aplicacion.Nucleo.Usuarios;
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
        IHostEnvironment environment,
        IUsuarioActual usuarioActual,
        CrearNotaParaTemaCasoUso casoUso,
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
                new CrearNotaParaTemaSolicitud(usuario.UsuarioId, temaId, request.Texto, request.Tipo),
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
        IHostEnvironment environment,
        IUsuarioActual usuarioActual,
        CrearNotaParaProyectoCasoUso casoUso,
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
                new CrearNotaParaProyectoSolicitud(usuario.UsuarioId, proyectoId, request.Texto, request.Tipo),
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
        IHostEnvironment environment,
        IUsuarioActual usuarioActual,
        CrearNotaParaLaboratorioCasoUso casoUso,
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
                new CrearNotaParaLaboratorioSolicitud(usuario.UsuarioId, laboratorioId, request.Texto, request.Tipo),
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
        IHostEnvironment environment,
        IUsuarioActual usuarioActual,
        CrearNotaParaWriteupCasoUso casoUso,
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
                new CrearNotaParaWriteupSolicitud(usuario.UsuarioId, writeupId, request.Texto, request.Tipo),
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
        IHostEnvironment environment,
        IUsuarioActual usuarioActual,
        CrearNotaParaArtefactoTecnicoCasoUso casoUso,
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
                new CrearNotaParaArtefactoTecnicoSolicitud(
                    usuario.UsuarioId,
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
        IHostEnvironment environment,
        IUsuarioActual usuarioActual,
        ObtenerNotaPorIdCasoUso casoUso,
        CancellationToken cancellationToken)
    {
        try
        {
            var resultado = await casoUso.EjecutarAsync(id, cancellationToken);

            if (!resultado.Encontrada)
                return Results.NotFound();

            var validacion = await UsuarioHttpContexto.ValidarPertenenciaPersonalAsync(
                resultado.Nota!.UsuarioId,
                environment,
                usuarioActual,
                cancellationToken);

            return validacion ?? Results.Ok(resultado.Nota);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> ListarNotasAsync(
        Guid? usuarioId,
        IHostEnvironment environment,
        IUsuarioActual usuarioActual,
        ListarNotasCasoUso casoUso,
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

            var resultado = await casoUso.EjecutarAsync(new ListarNotasSolicitud(usuario.UsuarioId), cancellationToken);

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

    private sealed record CrearNotaHttpRequest(Guid? UsuarioId, string Texto, TipoNota Tipo = TipoNota.Nota);
}
