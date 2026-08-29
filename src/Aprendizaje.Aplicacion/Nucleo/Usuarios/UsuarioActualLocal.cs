using Aprendizaje.Dominio.Nucleo;
using Aprendizaje.Dominio.Nucleo.Repositorios;

namespace Aprendizaje.Aplicacion.Nucleo.Usuarios;

public interface IUsuarioActual
{
    Task<ObtenerUsuarioActualResultado> ObtenerAsync(CancellationToken cancellationToken = default);
}

public sealed class UsuarioActualLocal : IUsuarioActual
{
    private readonly IUsuarioRepository _usuarios;

    public UsuarioActualLocal(IUsuarioRepository usuarios)
    {
        _usuarios = usuarios;
    }

    public async Task<ObtenerUsuarioActualResultado> ObtenerAsync(
        CancellationToken cancellationToken = default)
    {
        var usuariosVisibles = await _usuarios.ListarVisiblesAsync(cancellationToken);

        return usuariosVisibles.Count switch
        {
            0 => ObtenerUsuarioActualResultado.SinUsuarioLocal(),
            1 => ObtenerUsuarioActualResultado.DesdeUsuario(usuariosVisibles.Single()),
            _ => ObtenerUsuarioActualResultado.MultiplesUsuariosLocales()
        };
    }
}

public sealed record ObtenerUsuarioActualResultado(
    ObtenerUsuarioActualEstado Estado,
    UsuarioActualDto? Usuario)
{
    public bool Encontrado => Estado == ObtenerUsuarioActualEstado.Encontrado;

    public static ObtenerUsuarioActualResultado DesdeUsuario(Usuario usuario) =>
        new(
            ObtenerUsuarioActualEstado.Encontrado,
            new UsuarioActualDto(usuario.Id, usuario.Nombre));

    public static ObtenerUsuarioActualResultado SinUsuarioLocal() =>
        new(ObtenerUsuarioActualEstado.SinUsuarioLocal, null);

    public static ObtenerUsuarioActualResultado MultiplesUsuariosLocales() =>
        new(ObtenerUsuarioActualEstado.MultiplesUsuariosLocales, null);
}

public sealed record UsuarioActualDto(Guid Id, string Nombre);

public enum ObtenerUsuarioActualEstado
{
    Encontrado,
    SinUsuarioLocal,
    MultiplesUsuariosLocales
}
