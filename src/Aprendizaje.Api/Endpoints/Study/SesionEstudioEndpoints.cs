using Aprendizaje.Aplicacion.Study.SesionesEstudio.CorregirDuracionSesionEstudio;
using Aprendizaje.Aplicacion.Study.SesionesEstudio.ActualizarSesionEstudio;
using Aprendizaje.Aplicacion.Study.SesionesEstudio.EliminarSesionEstudio;
using Aprendizaje.Aplicacion.Study.SesionesEstudio.RegistrarSesionEstudio;
using Aprendizaje.Aplicacion.Study.SesionesEstudio.ListarSesionesEstudio;
using Aprendizaje.Aplicacion.Study.SesionesEstudio.ObtenerSesionEstudioPorId;
using Aprendizaje.Aplicacion.Study.SesionesEstudio.VincularHerramientaASesionEstudio;
using Aprendizaje.Api.Endpoints.Nucleo;
using Aprendizaje.Aplicacion.Nucleo.Usuarios;
using Aprendizaje.Dominio.Study;

namespace Aprendizaje.Api.Endpoints.Study;

public static class SesionEstudioEndpoints
{
    public static IEndpointRouteBuilder MapSesionEstudioEndpoints(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/sesiones-estudio");

        grupo.MapPost("/", RegistrarSesionEstudioAsync);
        grupo.MapGet("/", ListarSesionesEstudioAsync);
        grupo.MapGet("/{id:guid}", ObtenerSesionEstudioPorIdAsync);
        grupo.MapPut("/{id:guid}", ActualizarSesionEstudioAsync);
        grupo.MapDelete("/{id:guid}", EliminarSesionEstudioAsync);
        grupo.MapPut("/{id:guid}/duracion", CorregirDuracionAsync);
        grupo.MapPut("/{sesionId:guid}/herramientas/{herramientaId:guid}", VincularHerramientaAsync);

        return app;
    }

    private static async Task<IResult> VincularHerramientaAsync(
        Guid sesionId,
        Guid herramientaId,
        IHostEnvironment environment,
        IUsuarioActual usuarioActual,
        ObtenerSesionEstudioPorIdCasoUso obtenerSesion,
        VincularHerramientaASesionEstudioCasoUso casoUso,
        CancellationToken cancellationToken)
    {
        try
        {
            var validacion = await ValidarSesionPersonalAsync(
                sesionId,
                environment,
                usuarioActual,
                obtenerSesion,
                cancellationToken);

            if (validacion is not null)
                return validacion;

            var resultado = await casoUso.EjecutarAsync(
                new VincularHerramientaASesionEstudioSolicitud(sesionId, herramientaId),
                cancellationToken);

            return resultado.Estado switch
            {
                VincularHerramientaASesionEstudioEstado.Actualizado => Results.NoContent(),
                VincularHerramientaASesionEstudioEstado.SesionNoEncontrada => Results.NotFound(),
                VincularHerramientaASesionEstudioEstado.HerramientaNoEncontrada => Results.NotFound(),
                _ => Results.Problem("Estado de vínculo Sesión-Herramienta no reconocido.")
            };
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> CorregirDuracionAsync(
        Guid id,
        CorregirDuracionSesionEstudioHttpRequest request,
        IHostEnvironment environment,
        IUsuarioActual usuarioActual,
        ObtenerSesionEstudioPorIdCasoUso obtenerSesion,
        CorregirDuracionSesionEstudioCasoUso casoUso,
        CancellationToken cancellationToken)
    {
        try
        {
            var validacion = await ValidarSesionPersonalAsync(
                id,
                environment,
                usuarioActual,
                obtenerSesion,
                cancellationToken);

            if (validacion is not null)
                return validacion;

            var resultado = await casoUso.EjecutarAsync(
                new CorregirDuracionSesionEstudioSolicitud(id, request.DuracionMinutos),
                cancellationToken);

            return resultado.Estado switch
            {
                CorregirDuracionSesionEstudioEstado.Actualizada => Results.NoContent(),
                CorregirDuracionSesionEstudioEstado.SesionNoEncontrada => Results.NotFound(),
                _ => Results.Problem("Estado de corrección de duración no reconocido.")
            };
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> ActualizarSesionEstudioAsync(
        Guid id,
        ActualizarSesionEstudioHttpRequest request,
        IHostEnvironment environment,
        IUsuarioActual usuarioActual,
        ActualizarSesionEstudioCasoUso casoUso,
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
                new ActualizarSesionEstudioSolicitud(
                    id,
                    usuario.UsuarioId,
                    request.TemaId,
                    request.Fecha,
                    request.DuracionMinutos,
                    request.Tipo,
                    request.Notas),
                cancellationToken);

            return resultado.Estado switch
            {
                ActualizarSesionEstudioEstado.Actualizada => Results.NoContent(),
                ActualizarSesionEstudioEstado.UsuarioNoEncontrado => Results.NotFound(),
                ActualizarSesionEstudioEstado.SesionNoEncontrada => Results.NotFound(),
                ActualizarSesionEstudioEstado.TemaNoEncontrado => Results.NotFound(),
                ActualizarSesionEstudioEstado.UsuarioNoCoincide => Results.Conflict(new
                {
                    error = "La Sesión de estudio y el Tema deben pertenecer al usuario indicado."
                }),
                _ => Results.Problem("Estado de actualización de Sesión de estudio no reconocido.")
            };
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> EliminarSesionEstudioAsync(
        Guid id,
        Guid? usuarioId,
        IHostEnvironment environment,
        IUsuarioActual usuarioActual,
        EliminarSesionEstudioCasoUso casoUso,
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
                new EliminarSesionEstudioSolicitud(id, usuario.UsuarioId),
                cancellationToken);

            return resultado.Estado switch
            {
                EliminarSesionEstudioEstado.Eliminada => Results.NoContent(),
                EliminarSesionEstudioEstado.UsuarioNoEncontrado => Results.NotFound(),
                EliminarSesionEstudioEstado.SesionNoEncontrada => Results.NotFound(),
                EliminarSesionEstudioEstado.UsuarioNoCoincide => Results.Conflict(new
                {
                    error = "La Sesión de estudio no pertenece al usuario indicado."
                }),
                _ => Results.Problem("Estado de eliminación de Sesión de estudio no reconocido.")
            };
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> ObtenerSesionEstudioPorIdAsync(
        Guid id,
        IHostEnvironment environment,
        IUsuarioActual usuarioActual,
        ObtenerSesionEstudioPorIdCasoUso casoUso,
        CancellationToken cancellationToken)
    {
        var resultado = await casoUso.EjecutarAsync(id, cancellationToken);

        if (!resultado.Encontrado)
            return Results.NotFound();

        var validacion = await UsuarioHttpContexto.ValidarPertenenciaPersonalAsync(
            resultado.Sesion!.UsuarioId,
            environment,
            usuarioActual,
            cancellationToken);

        return validacion ?? Results.Ok(resultado.Sesion);
    }

    private static async Task<IResult> ListarSesionesEstudioAsync(
        Guid? usuarioId,
        IHostEnvironment environment,
        IUsuarioActual usuarioActual,
        ListarSesionesEstudioCasoUso casoUso,
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
                new ListarSesionesEstudioSolicitud(usuario.UsuarioId),
                cancellationToken);

            return Results.Ok(resultado.Sesiones);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> RegistrarSesionEstudioAsync(
        RegistrarSesionEstudioHttpRequest request,
        IHostEnvironment environment,
        IUsuarioActual usuarioActual,
        RegistrarSesionEstudioCasoUso casoUso,
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
                new RegistrarSesionEstudioSolicitud(
                    usuario.UsuarioId,
                    request.TemaId,
                    request.Fecha,
                    request.DuracionMinutos,
                    request.Tipo,
                    request.Notas),
                cancellationToken);

            return resultado.Estado switch
            {
                RegistrarSesionEstudioEstado.Creada => Results.Created(
                    $"/api/sesiones-estudio/{resultado.Id}",
                    resultado),
                RegistrarSesionEstudioEstado.TemaNoEncontrado => Results.NotFound(),
                RegistrarSesionEstudioEstado.UsuarioNoCoincide => Results.Conflict(new
                {
                    error = "La Sesión de estudio y el Tema deben pertenecer al mismo Usuario."
                }),
                _ => Results.Problem("Estado de registro de Sesión de estudio no reconocido.")
            };
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult?> ValidarSesionPersonalAsync(
        Guid sesionId,
        IHostEnvironment environment,
        IUsuarioActual usuarioActual,
        ObtenerSesionEstudioPorIdCasoUso obtenerSesion,
        CancellationToken cancellationToken)
    {
        if (!environment.IsEnvironment("Personal"))
            return null;

        var sesion = await obtenerSesion.EjecutarAsync(sesionId, cancellationToken);

        if (!sesion.Encontrado)
            return Results.NotFound();

        return await UsuarioHttpContexto.ValidarPertenenciaPersonalAsync(
            sesion.Sesion!.UsuarioId,
            environment,
            usuarioActual,
            cancellationToken);
    }

    private sealed record RegistrarSesionEstudioHttpRequest(
        Guid? UsuarioId,
        Guid TemaId,
        DateOnly Fecha,
        int DuracionMinutos,
        TipoSesion Tipo,
        string? Notas);

    private sealed record ActualizarSesionEstudioHttpRequest(
        Guid? UsuarioId,
        Guid TemaId,
        DateOnly Fecha,
        int DuracionMinutos,
        TipoSesion Tipo,
        string? Notas);

    private sealed record CorregirDuracionSesionEstudioHttpRequest(int DuracionMinutos);
}
