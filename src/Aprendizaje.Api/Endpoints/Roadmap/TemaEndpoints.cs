using Aprendizaje.Aplicacion.Roadmap.Temas.CrearTema;
using Aprendizaje.Aplicacion.Roadmap.Temas.ObtenerTemaPorId;
using Aprendizaje.Dominio.Roadmap;

namespace Aprendizaje.Api.Endpoints.Roadmap;

public static class TemaEndpoints
{
    public static IEndpointRouteBuilder MapTemaEndpoints(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/temas");

        grupo.MapPost("/", CrearTemaAsync);
        grupo.MapGet("/{id:guid}", ObtenerTemaPorIdAsync);

        return app;
    }

    private static async Task<IResult> CrearTemaAsync(
        CrearTemaHttpRequest request,
        CrearTemaCasoUso casoUso,
        CancellationToken cancellationToken)
    {
        try
        {
            var resultado = await casoUso.EjecutarAsync(
                new CrearTemaSolicitud(request.UsuarioId, request.Nombre, request.TipoConocimiento),
                cancellationToken);

            return Results.Created($"/api/temas/{resultado.Id}", resultado);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> ObtenerTemaPorIdAsync(
        Guid id,
        ObtenerTemaPorIdCasoUso casoUso,
        CancellationToken cancellationToken)
    {
        var resultado = await casoUso.EjecutarAsync(id, cancellationToken);

        return resultado.Encontrado
            ? Results.Ok(resultado.Tema)
            : Results.NotFound();
    }

    private sealed record CrearTemaHttpRequest(
        Guid UsuarioId,
        string Nombre,
        TipoConocimiento TipoConocimiento);
}
