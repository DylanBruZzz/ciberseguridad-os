using Aprendizaje.Aplicacion.Nucleo.Usuarios.CrearUsuario;

namespace Aprendizaje.Api.Endpoints.Nucleo;

public static class UsuarioEndpoints
{
    public static IEndpointRouteBuilder MapUsuarioEndpoints(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/usuarios");

        grupo.MapPost("/", CrearUsuarioAsync);

        return app;
    }

    private static async Task<IResult> CrearUsuarioAsync(
        CrearUsuarioHttpRequest request,
        CrearUsuarioCasoUso casoUso,
        CancellationToken cancellationToken)
    {
        try
        {
            var resultado = await casoUso.EjecutarAsync(
                new CrearUsuarioSolicitud(request.Nombre, request.Email),
                cancellationToken);

            return Results.Json(resultado, statusCode: StatusCodes.Status201Created);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private sealed record CrearUsuarioHttpRequest(string Nombre, string Email);
}
