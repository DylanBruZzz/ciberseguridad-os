using Aprendizaje.Aplicacion.Resource.Recursos.ActualizarRecurso;
using Aprendizaje.Aplicacion.Resource.Recursos.CrearRecurso;
using Aprendizaje.Aplicacion.Resource.Recursos.EliminarRecurso;
using Aprendizaje.Aplicacion.Resource.Recursos.ListarRecursos;
using Aprendizaje.Aplicacion.Resource.Recursos.ObtenerRecursoPorId;
using Aprendizaje.Aplicacion.Resource.Recursos.VincularRecursoATema;
using Aprendizaje.Api.Endpoints.Nucleo;
using Aprendizaje.Aplicacion.Nucleo.Usuarios;
using Aprendizaje.Dominio.Resource;

namespace Aprendizaje.Api.Endpoints.Resource;

public static class RecursoEndpoints
{
    public static IEndpointRouteBuilder MapRecursoEndpoints(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/recursos");

        grupo.MapPost("/", CrearRecursoAsync);
        grupo.MapGet("/", ListarRecursosAsync);
        grupo.MapGet("/{id:guid}", ObtenerRecursoPorIdAsync);
        grupo.MapPut("/{id:guid}", ActualizarRecursoAsync);
        grupo.MapDelete("/{id:guid}", EliminarRecursoAsync);
        grupo.MapPut("/{recursoId:guid}/temas/{temaId:guid}", VincularTemaAsync);

        return app;
    }

    private static async Task<IResult> CrearRecursoAsync(
        CrearRecursoHttpRequest request,
        IHostEnvironment environment,
        IUsuarioActual usuarioActual,
        CrearRecursoCasoUso casoUso,
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
                new CrearRecursoSolicitud(usuario.UsuarioId, request.Tipo, request.Titulo, request.Url),
                cancellationToken);

            return Results.Created($"/api/recursos/{resultado.Id}", resultado);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> ObtenerRecursoPorIdAsync(
        Guid id,
        IHostEnvironment environment,
        IUsuarioActual usuarioActual,
        ObtenerRecursoPorIdCasoUso casoUso,
        CancellationToken cancellationToken)
    {
        var resultado = await casoUso.EjecutarAsync(id, cancellationToken);

        if (!resultado.Encontrado)
            return Results.NotFound();

        var validacion = await UsuarioHttpContexto.ValidarPertenenciaPersonalAsync(
            resultado.Recurso!.UsuarioId,
            environment,
            usuarioActual,
            cancellationToken);

        return validacion ?? Results.Ok(resultado.Recurso);
    }

    private static async Task<IResult> ListarRecursosAsync(
        Guid? usuarioId,
        IHostEnvironment environment,
        IUsuarioActual usuarioActual,
        ListarRecursosCasoUso casoUso,
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
                new ListarRecursosSolicitud(usuario.UsuarioId),
                cancellationToken);

            return Results.Ok(resultado.Recursos);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> VincularTemaAsync(
        Guid recursoId,
        Guid temaId,
        IHostEnvironment environment,
        IUsuarioActual usuarioActual,
        ObtenerRecursoPorIdCasoUso obtenerRecurso,
        VincularRecursoATemaCasoUso casoUso,
        CancellationToken cancellationToken)
    {
        try
        {
            var validacion = await ValidarRecursoPersonalAsync(
                recursoId,
                environment,
                usuarioActual,
                obtenerRecurso,
                cancellationToken);

            if (validacion is not null)
                return validacion;

            var resultado = await casoUso.EjecutarAsync(
                new VincularRecursoATemaSolicitud(recursoId, temaId),
                cancellationToken);

            return resultado.Estado switch
            {
                VincularRecursoATemaEstado.Actualizado => Results.NoContent(),
                VincularRecursoATemaEstado.RecursoNoEncontrado => Results.NotFound(),
                VincularRecursoATemaEstado.TemaNoEncontrado => Results.NotFound(),
                VincularRecursoATemaEstado.UsuarioNoCoincide => Results.Conflict(new
                {
                    error = "El Recurso y el Tema deben pertenecer al mismo Usuario."
                }),
                _ => Results.Problem("Estado de vínculo Recurso-Tema no reconocido.")
            };
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> ActualizarRecursoAsync(
        Guid id,
        ActualizarRecursoHttpRequest request,
        IHostEnvironment environment,
        IUsuarioActual usuarioActual,
        ActualizarRecursoCasoUso casoUso,
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
                new ActualizarRecursoSolicitud(
                    id,
                    usuario.UsuarioId,
                    request.Titulo,
                    request.Url,
                    request.Estado,
                    request.Rating,
                    request.Notas,
                    request.HerramientaIA,
                    request.PromptsUtilizados),
                cancellationToken);

            return resultado.Estado switch
            {
                ActualizarRecursoEstado.Actualizado => Results.NoContent(),
                ActualizarRecursoEstado.UsuarioNoEncontrado => Results.NotFound(),
                ActualizarRecursoEstado.RecursoNoEncontrado => Results.NotFound(),
                ActualizarRecursoEstado.UsuarioNoCoincide => Results.Conflict(new
                {
                    error = "El Recurso no pertenece al usuario indicado."
                }),
                _ => Results.Problem("Estado de actualización de Recurso no reconocido.")
            };
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> EliminarRecursoAsync(
        Guid id,
        Guid? usuarioId,
        IHostEnvironment environment,
        IUsuarioActual usuarioActual,
        EliminarRecursoCasoUso casoUso,
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
                new EliminarRecursoSolicitud(id, usuario.UsuarioId),
                cancellationToken);

            return resultado.Estado switch
            {
                EliminarRecursoEstado.Eliminado => Results.NoContent(),
                EliminarRecursoEstado.UsuarioNoEncontrado => Results.NotFound(),
                EliminarRecursoEstado.RecursoNoEncontrado => Results.NotFound(),
                EliminarRecursoEstado.UsuarioNoCoincide => Results.Conflict(new
                {
                    error = "El Recurso no pertenece al usuario indicado."
                }),
                _ => Results.Problem("Estado de eliminación de Recurso no reconocido.")
            };
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult?> ValidarRecursoPersonalAsync(
        Guid recursoId,
        IHostEnvironment environment,
        IUsuarioActual usuarioActual,
        ObtenerRecursoPorIdCasoUso obtenerRecurso,
        CancellationToken cancellationToken)
    {
        if (!environment.IsEnvironment("Personal"))
            return null;

        var recurso = await obtenerRecurso.EjecutarAsync(recursoId, cancellationToken);

        if (!recurso.Encontrado)
            return Results.NotFound();

        return await UsuarioHttpContexto.ValidarPertenenciaPersonalAsync(
            recurso.Recurso!.UsuarioId,
            environment,
            usuarioActual,
            cancellationToken);
    }

    private sealed record CrearRecursoHttpRequest(
        Guid? UsuarioId,
        TipoRecurso Tipo,
        string Titulo,
        string? Url);

    private sealed record ActualizarRecursoHttpRequest(
        Guid? UsuarioId,
        string Titulo,
        string? Url,
        EstadoRecurso Estado,
        int? Rating,
        string? Notas,
        string? HerramientaIA,
        string? PromptsUtilizados);
}
