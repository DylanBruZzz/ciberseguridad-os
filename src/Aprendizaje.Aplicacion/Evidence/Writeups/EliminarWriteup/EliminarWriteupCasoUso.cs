using Aprendizaje.Aplicacion.Comun;
using Aprendizaje.Dominio.Evidence.Repositorios;
using Aprendizaje.Dominio.Nucleo.Repositorios;

namespace Aprendizaje.Aplicacion.Evidence.Writeups.EliminarWriteup;

public sealed class EliminarWriteupCasoUso
{
    private readonly IWriteupRepository _writeups;
    private readonly IUsuarioRepository _usuarios;
    private readonly IUnitOfWork _unitOfWork;

    public EliminarWriteupCasoUso(
        IWriteupRepository writeups,
        IUsuarioRepository usuarios,
        IUnitOfWork unitOfWork)
    {
        _writeups = writeups;
        _usuarios = usuarios;
        _unitOfWork = unitOfWork;
    }

    public async Task<EliminarWriteupResultado> EjecutarAsync(
        EliminarWriteupSolicitud solicitud,
        CancellationToken cancellationToken = default)
    {
        if (solicitud.WriteupId == Guid.Empty)
            throw new ArgumentException("El writeupId debe ser un Guid válido.", nameof(solicitud));

        if (solicitud.UsuarioId == Guid.Empty)
            throw new ArgumentException("El usuarioId debe ser un Guid válido.", nameof(solicitud));

        var usuario = await _usuarios.ObtenerPorIdAsync(solicitud.UsuarioId, cancellationToken);
        if (usuario is null)
            return EliminarWriteupResultado.UsuarioNoEncontrado();

        var writeup = await _writeups.ObtenerPorIdAsync(solicitud.WriteupId, cancellationToken);
        if (writeup is null)
            return EliminarWriteupResultado.WriteupNoEncontrado();

        if (writeup.UsuarioId != solicitud.UsuarioId)
            return EliminarWriteupResultado.UsuarioNoCoincide();

        writeup.MarcarComoEliminado();

        await _unitOfWork.GuardarCambiosAsync(cancellationToken);

        return EliminarWriteupResultado.Eliminado();
    }
}
