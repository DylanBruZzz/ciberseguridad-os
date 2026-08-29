using Aprendizaje.Aplicacion.Evidence.CertificacionesObtenidas.ActualizarCertificacionObtenida;
using Aprendizaje.Aplicacion.Evidence.CertificacionesObtenidas.CrearCertificacionObtenida;
using Aprendizaje.Aplicacion.Evidence.CertificacionesObtenidas.EliminarCertificacionObtenida;
using Aprendizaje.Aplicacion.Evidence.CertificacionesObtenidas.ListarCertificacionesObtenidas;
using Aprendizaje.Aplicacion.Evidence.CertificacionesObtenidas.ObtenerCertificacionObtenidaPorId;
using Aprendizaje.Api.Endpoints.Nucleo;
using Aprendizaje.Aplicacion.Nucleo.Usuarios;
using Aprendizaje.Dominio.Evidence;

namespace Aprendizaje.Api.Endpoints.Evidence;

public static class CertificacionObtenidaEndpoints
{
    public static IEndpointRouteBuilder MapCertificacionObtenidaEndpoints(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/certificaciones-obtenidas");

        grupo.MapPost("/", CrearCertificacionObtenidaAsync);
        grupo.MapGet("/", ListarCertificacionesObtenidasAsync);
        grupo.MapGet("/{id:guid}", ObtenerCertificacionObtenidaPorIdAsync);
        grupo.MapPut("/{id:guid}", ActualizarCertificacionObtenidaAsync);
        grupo.MapDelete("/{id:guid}", EliminarCertificacionObtenidaAsync);

        return app;
    }

    private static async Task<IResult> CrearCertificacionObtenidaAsync(
        CrearCertificacionObtenidaHttpRequest request,
        IHostEnvironment environment,
        IUsuarioActual usuarioActual,
        CrearCertificacionObtenidaCasoUso casoUso,
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
                new CrearCertificacionObtenidaSolicitud(
                    usuario.UsuarioId,
                    request.CertificacionId,
                    request.FechaObtencion),
                cancellationToken);

            return resultado.Estado switch
            {
                CrearCertificacionObtenidaEstado.Creada =>
                    Results.Created($"/api/certificaciones-obtenidas/{resultado.Id}", resultado),
                CrearCertificacionObtenidaEstado.CertificacionNoEncontrada => Results.NotFound(),
                _ => Results.Problem("Estado de creación de Certificación obtenida no reconocido.")
            };
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> ObtenerCertificacionObtenidaPorIdAsync(
        Guid id,
        IHostEnvironment environment,
        IUsuarioActual usuarioActual,
        ObtenerCertificacionObtenidaPorIdCasoUso casoUso,
        CancellationToken cancellationToken)
    {
        try
        {
            var resultado = await casoUso.EjecutarAsync(id, cancellationToken);

            if (!resultado.Encontrada)
                return Results.NotFound();

            var validacion = await UsuarioHttpContexto.ValidarPertenenciaPersonalAsync(
                resultado.CertificacionObtenida!.UsuarioId,
                environment,
                usuarioActual,
                cancellationToken);

            return validacion ?? Results.Ok(resultado.CertificacionObtenida);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> ListarCertificacionesObtenidasAsync(
        Guid? usuarioId,
        IHostEnvironment environment,
        IUsuarioActual usuarioActual,
        ListarCertificacionesObtenidasCasoUso casoUso,
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
                new ListarCertificacionesObtenidasSolicitud(usuario.UsuarioId),
                cancellationToken);

            return Results.Ok(resultado.CertificacionesObtenidas);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> ActualizarCertificacionObtenidaAsync(
        Guid id,
        ActualizarCertificacionObtenidaHttpRequest request,
        IHostEnvironment environment,
        IUsuarioActual usuarioActual,
        ActualizarCertificacionObtenidaCasoUso casoUso,
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
                new ActualizarCertificacionObtenidaSolicitud(
                    id,
                    usuario.UsuarioId,
                    request.EvidenciaUrl,
                    request.EstadoMadurez),
                cancellationToken);

            return resultado.Estado switch
            {
                ActualizarCertificacionObtenidaEstado.Actualizada => Results.NoContent(),
                ActualizarCertificacionObtenidaEstado.UsuarioNoEncontrado => Results.NotFound(),
                ActualizarCertificacionObtenidaEstado.CertificacionObtenidaNoEncontrada => Results.NotFound(),
                ActualizarCertificacionObtenidaEstado.UsuarioNoCoincide => Results.Conflict(new
                {
                    error = "La Certificación obtenida no pertenece al usuario indicado."
                }),
                _ => Results.Problem("Estado de actualización de Certificación obtenida no reconocido.")
            };
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> EliminarCertificacionObtenidaAsync(
        Guid id,
        Guid? usuarioId,
        IHostEnvironment environment,
        IUsuarioActual usuarioActual,
        EliminarCertificacionObtenidaCasoUso casoUso,
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
                new EliminarCertificacionObtenidaSolicitud(id, usuario.UsuarioId),
                cancellationToken);

            return resultado.Estado switch
            {
                EliminarCertificacionObtenidaEstado.Eliminada => Results.NoContent(),
                EliminarCertificacionObtenidaEstado.UsuarioNoEncontrado => Results.NotFound(),
                EliminarCertificacionObtenidaEstado.CertificacionObtenidaNoEncontrada => Results.NotFound(),
                EliminarCertificacionObtenidaEstado.UsuarioNoCoincide => Results.Conflict(new
                {
                    error = "La Certificación obtenida no pertenece al usuario indicado."
                }),
                _ => Results.Problem("Estado de eliminación de Certificación obtenida no reconocido.")
            };
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private sealed record CrearCertificacionObtenidaHttpRequest(
        Guid? UsuarioId,
        Guid CertificacionId,
        DateOnly FechaObtencion);

    private sealed record ActualizarCertificacionObtenidaHttpRequest(
        Guid? UsuarioId,
        string? EvidenciaUrl,
        EstadoMadurez EstadoMadurez);
}
