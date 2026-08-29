using Aprendizaje.Aplicacion.Nucleo.Usuarios;

namespace Aprendizaje.Api.Endpoints.Nucleo;

public static class UsuarioActualEndpoints
{
    private const string EntornoPersonal = "Personal";

    public static IEndpointRouteBuilder MapUsuarioActualEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/usuario-actual", ObtenerUsuarioActualAsync);

        return app;
    }

    private static async Task<IResult> ObtenerUsuarioActualAsync(
        IHostEnvironment environment,
        IUsuarioActual usuarioActual,
        CancellationToken cancellationToken)
    {
        if (!environment.IsEnvironment(EntornoPersonal))
            return Results.Conflict(new
            {
                codigo = "MODO_PERSONAL_REQUERIDO",
                error = "Usuario actual local solo esta disponible en environment Personal."
            });

        var resultado = await usuarioActual.ObtenerAsync(cancellationToken);

        return resultado.Estado switch
        {
            ObtenerUsuarioActualEstado.Encontrado => Results.Ok(resultado.Usuario),
            ObtenerUsuarioActualEstado.SinUsuarioLocal => Results.Conflict(new
            {
                codigo = "SIN_USUARIO_LOCAL",
                error = "No existe un Usuario visible para operar en modo Personal."
            }),
            ObtenerUsuarioActualEstado.MultiplesUsuariosLocales => Results.Conflict(new
            {
                codigo = "MULTIPLES_USUARIOS_LOCALES",
                error = "Existe mas de un Usuario visible para operar en modo Personal."
            }),
            _ => Results.Problem("Estado de Usuario actual no reconocido.")
        };
    }
}
