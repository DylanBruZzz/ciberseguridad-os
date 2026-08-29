using Aprendizaje.Aplicacion.Evidence.Proyectos.ActualizarProyecto;
using Aprendizaje.Aplicacion.Evidence.Proyectos.CrearProyecto;
using Aprendizaje.Aplicacion.Evidence.Proyectos.EliminarProyecto;
using Aprendizaje.Aplicacion.Evidence.Proyectos.ListarProyectos;
using Aprendizaje.Aplicacion.Evidence.Proyectos.ObtenerProyectoPorId;
using Aprendizaje.Aplicacion.Evidence.Proyectos.VincularProyectoAHerramienta;
using Aprendizaje.Aplicacion.Evidence.Proyectos.VincularProyectoATema;
using Aprendizaje.Api.Endpoints.Nucleo;
using Aprendizaje.Aplicacion.Nucleo.Usuarios;
using Aprendizaje.Dominio.Evidence;

namespace Aprendizaje.Api.Endpoints.Evidence;

public static class ProyectoEndpoints
{
    public static IEndpointRouteBuilder MapProyectoEndpoints(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/proyectos");

        grupo.MapPost("/", CrearProyectoAsync);
        grupo.MapGet("/", ListarProyectosAsync);
        grupo.MapGet("/{id:guid}", ObtenerProyectoPorIdAsync);
        grupo.MapPut("/{id:guid}", ActualizarProyectoAsync);
        grupo.MapDelete("/{id:guid}", EliminarProyectoAsync);
        grupo.MapPut("/{proyectoId:guid}/temas/{temaId:guid}", VincularTemaAsync);
        grupo.MapPut("/{proyectoId:guid}/herramientas/{herramientaId:guid}", VincularHerramientaAsync);

        return app;
    }

    private static async Task<IResult> CrearProyectoAsync(
        CrearProyectoHttpRequest request,
        IHostEnvironment environment,
        IUsuarioActual usuarioActual,
        CrearProyectoCasoUso casoUso,
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
                new CrearProyectoSolicitud(usuario.UsuarioId, request.Nombre),
                cancellationToken);

            return Results.Created($"/api/proyectos/{resultado.Id}", resultado);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> ObtenerProyectoPorIdAsync(
        Guid id,
        IHostEnvironment environment,
        IUsuarioActual usuarioActual,
        ObtenerProyectoPorIdCasoUso casoUso,
        CancellationToken cancellationToken)
    {
        try
        {
            var resultado = await casoUso.EjecutarAsync(id, cancellationToken);

            if (!resultado.Encontrado)
                return Results.NotFound();

            var validacion = await UsuarioHttpContexto.ValidarPertenenciaPersonalAsync(
                resultado.Proyecto!.UsuarioId,
                environment,
                usuarioActual,
                cancellationToken);

            return validacion ?? Results.Ok(resultado.Proyecto);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> ListarProyectosAsync(
        Guid? usuarioId,
        IHostEnvironment environment,
        IUsuarioActual usuarioActual,
        ListarProyectosCasoUso casoUso,
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
                new ListarProyectosSolicitud(usuario.UsuarioId),
                cancellationToken);

            return Results.Ok(resultado.Proyectos);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> VincularTemaAsync(
        Guid proyectoId,
        Guid temaId,
        IHostEnvironment environment,
        IUsuarioActual usuarioActual,
        ObtenerProyectoPorIdCasoUso obtenerProyecto,
        VincularProyectoATemaCasoUso casoUso,
        CancellationToken cancellationToken)
    {
        try
        {
            var validacion = await ValidarProyectoPersonalAsync(
                proyectoId,
                environment,
                usuarioActual,
                obtenerProyecto,
                cancellationToken);

            if (validacion is not null)
                return validacion;

            var resultado = await casoUso.EjecutarAsync(
                new VincularProyectoATemaSolicitud(proyectoId, temaId),
                cancellationToken);

            return resultado.Estado switch
            {
                VincularProyectoATemaEstado.Actualizado => Results.NoContent(),
                VincularProyectoATemaEstado.ProyectoNoEncontrado => Results.NotFound(),
                VincularProyectoATemaEstado.TemaNoEncontrado => Results.NotFound(),
                VincularProyectoATemaEstado.UsuarioNoCoincide => Results.Conflict(new
                {
                    error = "El Proyecto y el Tema deben pertenecer al mismo Usuario."
                }),
                _ => Results.Problem("Estado de vínculo Proyecto-Tema no reconocido.")
            };
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> VincularHerramientaAsync(
        Guid proyectoId,
        Guid herramientaId,
        IHostEnvironment environment,
        IUsuarioActual usuarioActual,
        ObtenerProyectoPorIdCasoUso obtenerProyecto,
        VincularProyectoAHerramientaCasoUso casoUso,
        CancellationToken cancellationToken)
    {
        try
        {
            var validacion = await ValidarProyectoPersonalAsync(
                proyectoId,
                environment,
                usuarioActual,
                obtenerProyecto,
                cancellationToken);

            if (validacion is not null)
                return validacion;

            var resultado = await casoUso.EjecutarAsync(
                new VincularProyectoAHerramientaSolicitud(proyectoId, herramientaId),
                cancellationToken);

            return resultado.Estado switch
            {
                VincularProyectoAHerramientaEstado.Actualizado => Results.NoContent(),
                VincularProyectoAHerramientaEstado.ProyectoNoEncontrado => Results.NotFound(),
                VincularProyectoAHerramientaEstado.HerramientaNoEncontrada => Results.NotFound(),
                _ => Results.Problem("Estado de vínculo Proyecto-Herramienta no reconocido.")
            };
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> ActualizarProyectoAsync(
        Guid id,
        ActualizarProyectoHttpRequest request,
        IHostEnvironment environment,
        IUsuarioActual usuarioActual,
        ActualizarProyectoCasoUso casoUso,
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
                new ActualizarProyectoSolicitud(
                    id,
                    usuario.UsuarioId,
                    request.Nombre,
                    request.Descripcion,
                    request.Estado,
                    request.EstadoMadurez,
                    request.RepositorioUrl,
                    request.FechaInicio,
                    request.FechaFin),
                cancellationToken);

            return resultado.Estado switch
            {
                ActualizarProyectoEstado.Actualizado => Results.NoContent(),
                ActualizarProyectoEstado.UsuarioNoEncontrado => Results.NotFound(),
                ActualizarProyectoEstado.ProyectoNoEncontrado => Results.NotFound(),
                ActualizarProyectoEstado.UsuarioNoCoincide => Results.Conflict(new
                {
                    error = "El Proyecto no pertenece al usuario indicado."
                }),
                _ => Results.Problem("Estado de actualización de Proyecto no reconocido.")
            };
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> EliminarProyectoAsync(
        Guid id,
        Guid? usuarioId,
        IHostEnvironment environment,
        IUsuarioActual usuarioActual,
        EliminarProyectoCasoUso casoUso,
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
                new EliminarProyectoSolicitud(id, usuario.UsuarioId),
                cancellationToken);

            return resultado.Estado switch
            {
                EliminarProyectoEstado.Eliminado => Results.NoContent(),
                EliminarProyectoEstado.UsuarioNoEncontrado => Results.NotFound(),
                EliminarProyectoEstado.ProyectoNoEncontrado => Results.NotFound(),
                EliminarProyectoEstado.UsuarioNoCoincide => Results.Conflict(new
                {
                    error = "El Proyecto no pertenece al usuario indicado."
                }),
                _ => Results.Problem("Estado de eliminación de Proyecto no reconocido.")
            };
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult?> ValidarProyectoPersonalAsync(
        Guid proyectoId,
        IHostEnvironment environment,
        IUsuarioActual usuarioActual,
        ObtenerProyectoPorIdCasoUso obtenerProyecto,
        CancellationToken cancellationToken)
    {
        if (!environment.IsEnvironment("Personal"))
            return null;

        var proyecto = await obtenerProyecto.EjecutarAsync(proyectoId, cancellationToken);

        if (!proyecto.Encontrado)
            return Results.NotFound();

        return await UsuarioHttpContexto.ValidarPertenenciaPersonalAsync(
            proyecto.Proyecto!.UsuarioId,
            environment,
            usuarioActual,
            cancellationToken);
    }

    private sealed record CrearProyectoHttpRequest(Guid? UsuarioId, string Nombre);

    private sealed record ActualizarProyectoHttpRequest(
        Guid? UsuarioId,
        string Nombre,
        string? Descripcion,
        EstadoProyecto Estado,
        EstadoMadurez EstadoMadurez,
        string? RepositorioUrl,
        DateOnly? FechaInicio,
        DateOnly? FechaFin);
}
