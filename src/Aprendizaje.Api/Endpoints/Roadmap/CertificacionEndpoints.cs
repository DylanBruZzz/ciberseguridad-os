using Aprendizaje.Aplicacion.Roadmap.Certificaciones.CrearCertificacion;
using Aprendizaje.Aplicacion.Roadmap.Certificaciones.ListarCertificaciones;
using Aprendizaje.Aplicacion.Roadmap.Certificaciones.ObtenerCertificacionPorId;
using Aprendizaje.Aplicacion.Roadmap.Certificaciones.VincularCertificacionATema;
using Aprendizaje.Aplicacion.Roadmap.Temas.ObtenerTemaPorId;
using Aprendizaje.Api.Endpoints.Nucleo;
using Aprendizaje.Aplicacion.Nucleo.Usuarios;
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
        grupo.MapPut("/{certificacionId:guid}/temas/{temaId:guid}", VincularTemaAsync);

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

    private static async Task<IResult> VincularTemaAsync(
        Guid certificacionId,
        Guid temaId,
        IHostEnvironment environment,
        IUsuarioActual usuarioActual,
        ObtenerTemaPorIdCasoUso obtenerTema,
        VincularCertificacionATemaCasoUso casoUso,
        CancellationToken cancellationToken)
    {
        try
        {
            if (environment.IsEnvironment("Personal"))
            {
                var tema = await obtenerTema.EjecutarAsync(temaId, cancellationToken);

                if (!tema.Encontrado)
                    return Results.NotFound();

                var validacion = await UsuarioHttpContexto.ValidarPertenenciaPersonalAsync(
                    tema.Tema!.UsuarioId,
                    environment,
                    usuarioActual,
                    cancellationToken);

                if (validacion is not null)
                    return validacion;
            }

            var resultado = await casoUso.EjecutarAsync(
                new VincularCertificacionATemaSolicitud(certificacionId, temaId),
                cancellationToken);

            return resultado.Estado switch
            {
                VincularCertificacionATemaEstado.Actualizado => Results.NoContent(),
                VincularCertificacionATemaEstado.CertificacionNoEncontrada => Results.NotFound(),
                VincularCertificacionATemaEstado.TemaNoEncontrado => Results.NotFound(),
                _ => Results.Problem("Estado de vínculo Certificación-Tema no reconocido.")
            };
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private sealed record CrearCertificacionHttpRequest(string Nombre, TipoCosto TipoCosto);
}
