using Aprendizaje.Aplicacion.Evidence.Writeups.ActualizarWriteup;
using Aprendizaje.Aplicacion.Evidence.Writeups.CrearWriteup;
using Aprendizaje.Aplicacion.Evidence.Writeups.EliminarWriteup;
using Aprendizaje.Aplicacion.Evidence.Writeups.ListarWriteups;
using Aprendizaje.Aplicacion.Evidence.Writeups.ObtenerWriteupPorId;
using Aprendizaje.Aplicacion.Evidence.Writeups.VincularWriteupATema;
using Aprendizaje.Dominio.Evidence;

namespace Aprendizaje.Api.Endpoints.Evidence;

public static class WriteupEndpoints
{
    public static IEndpointRouteBuilder MapWriteupEndpoints(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/writeups");

        grupo.MapPost("/", CrearWriteupAsync);
        grupo.MapGet("/", ListarWriteupsAsync);
        grupo.MapGet("/{id:guid}", ObtenerWriteupPorIdAsync);
        grupo.MapPut("/{id:guid}", ActualizarWriteupAsync);
        grupo.MapDelete("/{id:guid}", EliminarWriteupAsync);
        grupo.MapPut("/{writeupId:guid}/temas/{temaId:guid}", VincularTemaAsync);

        return app;
    }

    private static async Task<IResult> CrearWriteupAsync(
        CrearWriteupHttpRequest request,
        CrearWriteupCasoUso casoUso,
        CancellationToken cancellationToken)
    {
        try
        {
            var resultado = await casoUso.EjecutarAsync(
                new CrearWriteupSolicitud(request.UsuarioId, request.Titulo),
                cancellationToken);

            return Results.Created($"/api/writeups/{resultado.Id}", resultado);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> ObtenerWriteupPorIdAsync(
        Guid id,
        ObtenerWriteupPorIdCasoUso casoUso,
        CancellationToken cancellationToken)
    {
        try
        {
            var resultado = await casoUso.EjecutarAsync(id, cancellationToken);

            return resultado.Encontrado
                ? Results.Ok(resultado.Writeup)
                : Results.NotFound();
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> ListarWriteupsAsync(
        Guid usuarioId,
        ListarWriteupsCasoUso casoUso,
        CancellationToken cancellationToken)
    {
        try
        {
            var resultado = await casoUso.EjecutarAsync(
                new ListarWriteupsSolicitud(usuarioId),
                cancellationToken);

            return Results.Ok(resultado.Writeups);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> VincularTemaAsync(
        Guid writeupId,
        Guid temaId,
        VincularWriteupATemaCasoUso casoUso,
        CancellationToken cancellationToken)
    {
        try
        {
            var resultado = await casoUso.EjecutarAsync(
                new VincularWriteupATemaSolicitud(writeupId, temaId),
                cancellationToken);

            return resultado.Estado switch
            {
                VincularWriteupATemaEstado.Actualizado => Results.NoContent(),
                VincularWriteupATemaEstado.WriteupNoEncontrado => Results.NotFound(),
                VincularWriteupATemaEstado.TemaNoEncontrado => Results.NotFound(),
                VincularWriteupATemaEstado.UsuarioNoCoincide => Results.Conflict(new
                {
                    error = "El Writeup y el Tema deben pertenecer al mismo Usuario."
                }),
                _ => Results.Problem("Estado de vínculo Writeup-Tema no reconocido.")
            };
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> ActualizarWriteupAsync(
        Guid id,
        ActualizarWriteupHttpRequest request,
        ActualizarWriteupCasoUso casoUso,
        CancellationToken cancellationToken)
    {
        try
        {
            var resultado = await casoUso.EjecutarAsync(
                new ActualizarWriteupSolicitud(
                    id,
                    request.UsuarioId,
                    request.Titulo,
                    request.PlataformaOrigen,
                    request.Url,
                    request.Fecha,
                    request.EstadoMadurez),
                cancellationToken);

            return resultado.Estado switch
            {
                ActualizarWriteupEstado.Actualizado => Results.NoContent(),
                ActualizarWriteupEstado.UsuarioNoEncontrado => Results.NotFound(),
                ActualizarWriteupEstado.WriteupNoEncontrado => Results.NotFound(),
                ActualizarWriteupEstado.UsuarioNoCoincide => Results.Conflict(new
                {
                    error = "El Writeup no pertenece al usuario indicado."
                }),
                _ => Results.Problem("Estado de actualización de Writeup no reconocido.")
            };
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> EliminarWriteupAsync(
        Guid id,
        Guid usuarioId,
        EliminarWriteupCasoUso casoUso,
        CancellationToken cancellationToken)
    {
        try
        {
            var resultado = await casoUso.EjecutarAsync(new EliminarWriteupSolicitud(id, usuarioId), cancellationToken);

            return resultado.Estado switch
            {
                EliminarWriteupEstado.Eliminado => Results.NoContent(),
                EliminarWriteupEstado.UsuarioNoEncontrado => Results.NotFound(),
                EliminarWriteupEstado.WriteupNoEncontrado => Results.NotFound(),
                EliminarWriteupEstado.UsuarioNoCoincide => Results.Conflict(new
                {
                    error = "El Writeup no pertenece al usuario indicado."
                }),
                _ => Results.Problem("Estado de eliminación de Writeup no reconocido.")
            };
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private sealed record CrearWriteupHttpRequest(Guid UsuarioId, string Titulo);

    private sealed record ActualizarWriteupHttpRequest(
        Guid UsuarioId,
        string Titulo,
        string? PlataformaOrigen,
        string? Url,
        DateOnly? Fecha,
        EstadoMadurez EstadoMadurez);
}
