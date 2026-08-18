using Aprendizaje.Aplicacion.Roadmap.Temas.CrearTema;
using Aprendizaje.Aplicacion.Roadmap.Temas.EstablecerObjetivos;
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
        grupo.MapPut("/{id:guid}/objetivos", EstablecerObjetivosAsync);

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

    private static async Task<IResult> EstablecerObjetivosAsync(
        Guid id,
        EstablecerObjetivosTemaHttpRequest request,
        EstablecerObjetivosTemaCasoUso casoUso,
        CancellationToken cancellationToken)
    {
        if (request.Objetivos is null)
            return Results.BadRequest(new { error = "La lista de objetivos no puede ser null." });

        try
        {
            var resultado = await casoUso.EjecutarAsync(
                new EstablecerObjetivosTemaSolicitud(id, request.Objetivos),
                cancellationToken);

            return resultado.Encontrado
                ? Results.NoContent()
                : Results.NotFound();
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private sealed record CrearTemaHttpRequest(
        Guid UsuarioId,
        string Nombre,
        TipoConocimiento TipoConocimiento);

    private sealed record EstablecerObjetivosTemaHttpRequest(
        IReadOnlyCollection<string>? Objetivos);
}
