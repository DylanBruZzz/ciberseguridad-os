using Aprendizaje.Aplicacion.Study.Herramientas.CrearHerramienta;
using Aprendizaje.Aplicacion.Study.Herramientas.ListarHerramientas;
using Aprendizaje.Aplicacion.Study.Herramientas.ObtenerHerramientaPorId;

namespace Aprendizaje.Api.Endpoints.Study;

public static class HerramientaEndpoints
{
    public static IEndpointRouteBuilder MapHerramientaEndpoints(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/herramientas");

        grupo.MapPost("/", CrearHerramientaAsync);
        grupo.MapGet("/", ListarHerramientasAsync);
        grupo.MapGet("/{id:guid}", ObtenerHerramientaPorIdAsync);

        return app;
    }

    private static async Task<IResult> CrearHerramientaAsync(
        CrearHerramientaHttpRequest request,
        CrearHerramientaCasoUso casoUso,
        CancellationToken cancellationToken)
    {
        try
        {
            var resultado = await casoUso.EjecutarAsync(
                new CrearHerramientaSolicitud(request.Nombre, request.Categoria),
                cancellationToken);

            return Results.Created($"/api/herramientas/{resultado.Id}", resultado);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> ObtenerHerramientaPorIdAsync(
        Guid id,
        ObtenerHerramientaPorIdCasoUso casoUso,
        CancellationToken cancellationToken)
    {
        try
        {
            var resultado = await casoUso.EjecutarAsync(id, cancellationToken);

            return resultado.Encontrada
                ? Results.Ok(resultado.Herramienta)
                : Results.NotFound();
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> ListarHerramientasAsync(
        ListarHerramientasCasoUso casoUso,
        CancellationToken cancellationToken)
    {
        var resultado = await casoUso.EjecutarAsync(cancellationToken);

        return Results.Ok(resultado.Herramientas);
    }

    private sealed record CrearHerramientaHttpRequest(string Nombre, string? Categoria);
}
