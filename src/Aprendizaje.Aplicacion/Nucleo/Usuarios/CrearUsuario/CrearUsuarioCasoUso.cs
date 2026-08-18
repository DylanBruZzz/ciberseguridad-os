using Aprendizaje.Aplicacion.Comun;
using Aprendizaje.Dominio.Nucleo;
using Aprendizaje.Dominio.Nucleo.Repositorios;

namespace Aprendizaje.Aplicacion.Nucleo.Usuarios.CrearUsuario;

public sealed class CrearUsuarioCasoUso
{
    private readonly IUsuarioRepository _usuarios;
    private readonly IUnitOfWork _unitOfWork;

    public CrearUsuarioCasoUso(IUsuarioRepository usuarios, IUnitOfWork unitOfWork)
    {
        _usuarios = usuarios;
        _unitOfWork = unitOfWork;
    }

    public async Task<CrearUsuarioResultado> EjecutarAsync(
        CrearUsuarioSolicitud solicitud,
        CancellationToken cancellationToken = default)
    {
        var usuario = Usuario.Registrar(solicitud.Nombre, solicitud.Email);

        _usuarios.Agregar(usuario);
        await _unitOfWork.GuardarCambiosAsync(cancellationToken);

        return new CrearUsuarioResultado(usuario.Id, usuario.Nombre, usuario.Email);
    }
}
