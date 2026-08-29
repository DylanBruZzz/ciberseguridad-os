using Aprendizaje.Aplicacion.Nucleo.Usuarios;

namespace Aprendizaje.Api.Endpoints.Nucleo;

public static class UsuarioHttpContexto
{
    private const string EntornoPersonal = "Personal";

    public static async Task<ResolucionUsuarioHttp> ResolverAsync(
        Guid? usuarioIdExplicito,
        IHostEnvironment environment,
        IUsuarioActual usuarioActual,
        CancellationToken cancellationToken)
    {
        if (!environment.IsEnvironment(EntornoPersonal))
        {
            return usuarioIdExplicito.HasValue
                ? ResolucionUsuarioHttp.Resuelta(usuarioIdExplicito.Value)
                : ResolucionUsuarioHttp.Fallida(Results.BadRequest(new
                {
                    error = "usuarioId es requerido fuera de environment Personal."
                }));
        }

        var resultado = await usuarioActual.ObtenerAsync(cancellationToken);

        if (resultado.Estado == ObtenerUsuarioActualEstado.SinUsuarioLocal)
            return ResolucionUsuarioHttp.Fallida(Results.Conflict(new
            {
                codigo = "SIN_USUARIO_LOCAL",
                error = "No existe un Usuario visible para operar en modo Personal."
            }));

        if (resultado.Estado == ObtenerUsuarioActualEstado.MultiplesUsuariosLocales)
            return ResolucionUsuarioHttp.Fallida(Results.Conflict(new
            {
                codigo = "MULTIPLES_USUARIOS_LOCALES",
                error = "Existe mas de un Usuario visible para operar en modo Personal."
            }));

        var usuarioIdActual = resultado.Usuario!.Id;

        if (usuarioIdExplicito.HasValue && usuarioIdExplicito.Value != usuarioIdActual)
            return ResolucionUsuarioHttp.Fallida(Results.Conflict(new
            {
                codigo = "USUARIO_EXPLICITO_NO_COINCIDE",
                error = "El usuarioId indicado no coincide con el Usuario actual local."
            }));

        return ResolucionUsuarioHttp.Resuelta(usuarioIdActual);
    }

    public static async Task<IResult?> ValidarPertenenciaPersonalAsync(
        Guid usuarioIdEntidad,
        IHostEnvironment environment,
        IUsuarioActual usuarioActual,
        CancellationToken cancellationToken)
    {
        if (!environment.IsEnvironment(EntornoPersonal))
            return null;

        var resolucion = await ResolverAsync(null, environment, usuarioActual, cancellationToken);

        if (!resolucion.Exitosa)
            return resolucion.Error;

        return resolucion.UsuarioId == usuarioIdEntidad
            ? null
            : Results.NotFound();
    }
}

public sealed record ResolucionUsuarioHttp(Guid UsuarioId, IResult? Error)
{
    public bool Exitosa => Error is null;

    public static ResolucionUsuarioHttp Resuelta(Guid usuarioId) => new(usuarioId, null);

    public static ResolucionUsuarioHttp Fallida(IResult error) => new(Guid.Empty, error);
}
