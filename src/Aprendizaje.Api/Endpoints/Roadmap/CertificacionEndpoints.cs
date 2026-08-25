using Aprendizaje.Aplicacion.Roadmap.Certificaciones.CrearCertificacion;
using Aprendizaje.Aplicacion.Roadmap.Certificaciones.ListarCertificaciones;
using Aprendizaje.Aplicacion.Roadmap.Certificaciones.ObtenerCertificacionPorId;
using Aprendizaje.Dominio.Roadmap;

namespace Aprendizaje.Api.Endpoints.Roadmap;

public static class CertificacionEndpoints
{
    public static IEndpointRouteBuilder MapCertificacionEndpoints(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/certificaciones");

        grupo.MapPost("/", CrearCertificacionAsync);
        grupo.MapGet("/", ListarCertificacionesAsync);
        grupo.MapGet("/{id:guid}", ObtenerCertificacionPorIdAsync);

        return app;
    }

    private static async Task<IResult> CrearCertificacionAsync(
        CrearCertificacionHttpRequest request,
        CrearCertificacionCasoUso casoUso,
        CancellationToken cancellationToken)
    {
        try
        {
            var resultado = await casoUso.EjecutarAsync(
                new CrearCertificacionSolicitud(request.Nombre, request.TipoCosto),
                cancellationToken);

            return Results.Created($"/api/certificaciones/{resultado.Id}", resultado);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> ObtenerCertificacionPorIdAsync(
        Guid id,
        ObtenerCertificacionPorIdCasoUso casoUso,
        CancellationToken cancellationToken)
    {
        try
        {
            var resultado = await casoUso.EjecutarAsync(id, cancellationToken);

            return resultado.Encontrado
                ? Results.Ok(resultado.Certificacion)
                : Results.NotFound();
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> ListarCertificacionesAsync(
        ListarCertificacionesCasoUso casoUso,
        CancellationToken cancellationToken)
    {
        var resultado = await casoUso.EjecutarAsync(cancellationToken);

        return Results.Ok(resultado.Certificaciones);
    }

    private sealed record CrearCertificacionHttpRequest(string Nombre, TipoCosto TipoCosto);
}
