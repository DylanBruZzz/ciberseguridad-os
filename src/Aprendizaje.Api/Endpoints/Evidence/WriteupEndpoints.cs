using Aprendizaje.Aplicacion.Evidence.Writeups.ActualizarWriteup;
using Aprendizaje.Aplicacion.Evidence.Writeups.CrearWriteup;
using Aprendizaje.Aplicacion.Evidence.Writeups.EliminarWriteup;
using Aprendizaje.Aplicacion.Evidence.Writeups.ListarWriteups;
using Aprendizaje.Aplicacion.Evidence.Writeups.ObtenerWriteupPorId;
using Aprendizaje.Aplicacion.Evidence.Writeups.VincularWriteupATema;
using Aprendizaje.Api.Endpoints.Nucleo;
using Aprendizaje.Aplicacion.Nucleo.Usuarios;
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
        IHostEnvironment environment,
        IUsuarioActual usuarioActual,
        CrearWriteupCasoUso casoUso,
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
                new CrearWriteupSolicitud(usuario.UsuarioId, request.Titulo),
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
        IHostEnvironment environment,
        IUsuarioActual usuarioActual,
        ObtenerWriteupPorIdCasoUso casoUso,
        CancellationToken cancellationToken)
    {
        try
        {
            var resultado = await casoUso.EjecutarAsync(id, cancellationToken);

            if (!resultado.Encontrado)
                return Results.NotFound();

            var validacion = await UsuarioHttpContexto.ValidarPertenenciaPersonalAsync(
                resultado.Writeup!.UsuarioId,
                environment,
                usuarioActual,
                cancellationToken);

            return validacion ?? Results.Ok(resultado.Writeup);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> ListarWriteupsAsync(
        Guid? usuarioId,
        IHostEnvironment environment,
        IUsuarioActual usuarioActual,
        ListarWriteupsCasoUso casoUso,
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
                new ListarWriteupsSolicitud(usuario.UsuarioId),
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
        IHostEnvironment environment,
        IUsuarioActual usuarioActual,
        ObtenerWriteupPorIdCasoUso obtenerWriteup,
        VincularWriteupATemaCasoUso casoUso,
        CancellationToken cancellationToken)
    {
        try
        {
            var validacion = await ValidarWriteupPersonalAsync(
                writeupId,
                environment,
                usuarioActual,
                obtenerWriteup,
                cancellationToken);

            if (validacion is not null)
                return validacion;

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
        IHostEnvironment environment,
        IUsuarioActual usuarioActual,
        ActualizarWriteupCasoUso casoUso,
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
                new ActualizarWriteupSolicitud(
                    id,
                    usuario.UsuarioId,
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
        Guid? usuarioId,
        IHostEnvironment environment,
        IUsuarioActual usuarioActual,
        EliminarWriteupCasoUso casoUso,
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
                new EliminarWriteupSolicitud(id, usuario.UsuarioId),
                cancellationToken);

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

    private static async Task<IResult?> ValidarWriteupPersonalAsync(
        Guid writeupId,
        IHostEnvironment environment,
        IUsuarioActual usuarioActual,
        ObtenerWriteupPorIdCasoUso obtenerWriteup,
        CancellationToken cancellationToken)
    {
        if (!environment.IsEnvironment("Personal"))
            return null;

        var writeup = await obtenerWriteup.EjecutarAsync(writeupId, cancellationToken);

        if (!writeup.Encontrado)
            return Results.NotFound();

        return await UsuarioHttpContexto.ValidarPertenenciaPersonalAsync(
            writeup.Writeup!.UsuarioId,
            environment,
            usuarioActual,
            cancellationToken);
    }

    private sealed record CrearWriteupHttpRequest(Guid? UsuarioId, string Titulo);

    private sealed record ActualizarWriteupHttpRequest(
        Guid? UsuarioId,
        string Titulo,
        string? PlataformaOrigen,
        string? Url,
        DateOnly? Fecha,
        EstadoMadurez EstadoMadurez);
}
