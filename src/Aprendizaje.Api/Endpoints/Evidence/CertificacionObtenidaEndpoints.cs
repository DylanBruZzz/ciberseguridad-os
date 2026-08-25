using Aprendizaje.Aplicacion.Evidence.CertificacionesObtenidas.CrearCertificacionObtenida;
using Aprendizaje.Aplicacion.Evidence.CertificacionesObtenidas.ListarCertificacionesObtenidas;
using Aprendizaje.Aplicacion.Evidence.CertificacionesObtenidas.ObtenerCertificacionObtenidaPorId;

namespace Aprendizaje.Api.Endpoints.Evidence;

public static class CertificacionObtenidaEndpoints
{
    public static IEndpointRouteBuilder MapCertificacionObtenidaEndpoints(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/certificaciones-obtenidas");

        grupo.MapPost("/", CrearCertificacionObtenidaAsync);
        grupo.MapGet("/", ListarCertificacionesObtenidasAsync);
        grupo.MapGet("/{id:guid}", ObtenerCertificacionObtenidaPorIdAsync);

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

    private sealed record CrearCertificacionObtenidaHttpRequest(
        Guid UsuarioId,
        Guid CertificacionId,
        DateOnly FechaObtencion);
}
