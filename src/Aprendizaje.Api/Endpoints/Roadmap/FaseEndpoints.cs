using Aprendizaje.Aplicacion.Roadmap.Fases.CrearFase;
using Aprendizaje.Aplicacion.Roadmap.Fases.ListarFases;

namespace Aprendizaje.Api.Endpoints.Roadmap;

public static class FaseEndpoints
{
    public static IEndpointRouteBuilder MapFaseEndpoints(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/fases");

        grupo.MapPost("/", CrearFaseAsync);
        grupo.MapGet("/", ListarFasesAsync);

        return app;
    }

    private static async Task<IResult> CrearFaseAsync(
        CrearFaseHttpRequest request,
        CrearFaseCasoUso casoUso,
        CancellationToken cancellationToken)
    {
        try
        {
            var resultado = await casoUso.EjecutarAsync(
                new CrearFaseSolicitud(request.UsuarioId, request.Nombre, request.Orden),
                cancellationToken);

            return Results.Json(resultado, statusCode: StatusCodes.Status201Created);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> ListarFasesAsync(
        Guid usuarioId,
        ListarFasesCasoUso casoUso,
        CancellationToken cancellationToken)
    {
        try
        {
            var resultado = await casoUso.EjecutarAsync(
                new ListarFasesSolicitud(usuarioId),
                cancellationToken);

            return Results.Ok(resultado.Fases);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private sealed record CrearFaseHttpRequest(
        Guid UsuarioId,
        string Nombre,
        int Orden);
}
