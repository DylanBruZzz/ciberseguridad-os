using Aprendizaje.Aplicacion.Evidence.CertificacionesObtenidas.ActualizarCertificacionObtenida;
using Aprendizaje.Aplicacion.Evidence.CertificacionesObtenidas.CrearCertificacionObtenida;
using Aprendizaje.Aplicacion.Evidence.CertificacionesObtenidas.EliminarCertificacionObtenida;
using Aprendizaje.Aplicacion.Evidence.CertificacionesObtenidas.ListarCertificacionesObtenidas;
using Aprendizaje.Aplicacion.Evidence.CertificacionesObtenidas.ObtenerCertificacionObtenidaPorId;
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
        CrearCertificacionObtenidaCasoUso casoUso,
        CancellationToken cancellationToken)
    {
        try
        {
            var resultado = await casoUso.EjecutarAsync(
                new CrearCertificacionObtenidaSolicitud(
                    request.UsuarioId,
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
        ObtenerCertificacionObtenidaPorIdCasoUso casoUso,
        CancellationToken cancellationToken)
    {
        try
        {
            var resultado = await casoUso.EjecutarAsync(id, cancellationToken);

            return resultado.Encontrada
                ? Results.Ok(resultado.CertificacionObtenida)
                : Results.NotFound();
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> ListarCertificacionesObtenidasAsync(
        Guid usuarioId,
        ListarCertificacionesObtenidasCasoUso casoUso,
        CancellationToken cancellationToken)
    {
        try
        {
            var resultado = await casoUso.EjecutarAsync(
                new ListarCertificacionesObtenidasSolicitud(usuarioId),
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
        ActualizarCertificacionObtenidaCasoUso casoUso,
        CancellationToken cancellationToken)
    {
        try
        {
            var resultado = await casoUso.EjecutarAsync(
                new ActualizarCertificacionObtenidaSolicitud(
                    id,
                    request.UsuarioId,
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
        Guid usuarioId,
        EliminarCertificacionObtenidaCasoUso casoUso,
        CancellationToken cancellationToken)
    {
        try
        {
            var resultado = await casoUso.EjecutarAsync(
                new EliminarCertificacionObtenidaSolicitud(id, usuarioId),
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
        Guid UsuarioId,
        Guid CertificacionId,
        DateOnly FechaObtencion);

    private sealed record ActualizarCertificacionObtenidaHttpRequest(
        Guid UsuarioId,
        string? EvidenciaUrl,
        EstadoMadurez EstadoMadurez);
}
