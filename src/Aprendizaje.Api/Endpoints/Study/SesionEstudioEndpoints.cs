using Aprendizaje.Aplicacion.Study.SesionesEstudio.CorregirDuracionSesionEstudio;
using Aprendizaje.Aplicacion.Study.SesionesEstudio.ActualizarSesionEstudio;
using Aprendizaje.Aplicacion.Study.SesionesEstudio.EliminarSesionEstudio;
using Aprendizaje.Aplicacion.Study.SesionesEstudio.RegistrarSesionEstudio;
using Aprendizaje.Aplicacion.Study.SesionesEstudio.ListarSesionesEstudio;
using Aprendizaje.Aplicacion.Study.SesionesEstudio.ObtenerSesionEstudioPorId;
using Aprendizaje.Aplicacion.Study.SesionesEstudio.VincularHerramientaASesionEstudio;
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
        VincularHerramientaASesionEstudioCasoUso casoUso,
        CancellationToken cancellationToken)
    {
        try
        {
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
        CorregirDuracionSesionEstudioCasoUso casoUso,
        CancellationToken cancellationToken)
    {
        try
        {
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
        ActualizarSesionEstudioCasoUso casoUso,
        CancellationToken cancellationToken)
    {
        try
        {
            var resultado = await casoUso.EjecutarAsync(
                new ActualizarSesionEstudioSolicitud(
                    id,
                    request.UsuarioId,
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
        Guid usuarioId,
        EliminarSesionEstudioCasoUso casoUso,
        CancellationToken cancellationToken)
    {
        try
        {
            var resultado = await casoUso.EjecutarAsync(
                new EliminarSesionEstudioSolicitud(id, usuarioId),
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
        ObtenerSesionEstudioPorIdCasoUso casoUso,
        CancellationToken cancellationToken)
    {
        var resultado = await casoUso.EjecutarAsync(id, cancellationToken);

        return resultado.Encontrado
            ? Results.Ok(resultado.Sesion)
            : Results.NotFound();
    }

    private static async Task<IResult> ListarSesionesEstudioAsync(
        Guid usuarioId,
        ListarSesionesEstudioCasoUso casoUso,
        CancellationToken cancellationToken)
    {
        try
        {
            var resultado = await casoUso.EjecutarAsync(
                new ListarSesionesEstudioSolicitud(usuarioId),
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
        RegistrarSesionEstudioCasoUso casoUso,
        CancellationToken cancellationToken)
    {
        try
        {
            var resultado = await casoUso.EjecutarAsync(
                new RegistrarSesionEstudioSolicitud(
                    request.UsuarioId,
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

    private sealed record RegistrarSesionEstudioHttpRequest(
        Guid UsuarioId,
        Guid TemaId,
        DateOnly Fecha,
        int DuracionMinutos,
        TipoSesion Tipo,
        string? Notas);

    private sealed record ActualizarSesionEstudioHttpRequest(
        Guid UsuarioId,
        Guid TemaId,
        DateOnly Fecha,
        int DuracionMinutos,
        TipoSesion Tipo,
        string? Notas);

    private sealed record CorregirDuracionSesionEstudioHttpRequest(int DuracionMinutos);
}
