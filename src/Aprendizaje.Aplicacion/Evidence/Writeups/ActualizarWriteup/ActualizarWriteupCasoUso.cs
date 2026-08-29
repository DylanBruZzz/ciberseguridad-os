using Aprendizaje.Aplicacion.Comun;
using Aprendizaje.Dominio.Evidence.Repositorios;
using Aprendizaje.Dominio.Nucleo.Repositorios;

namespace Aprendizaje.Aplicacion.Evidence.Writeups.ActualizarWriteup;

public sealed class ActualizarWriteupCasoUso
{
    private readonly IWriteupRepository _writeups;
    private readonly IUsuarioRepository _usuarios;
    private readonly IUnitOfWork _unitOfWork;

    public ActualizarWriteupCasoUso(
        IWriteupRepository writeups,
        IUsuarioRepository usuarios,
        IUnitOfWork unitOfWork)
    {
        _writeups = writeups;
        _usuarios = usuarios;
        _unitOfWork = unitOfWork;
    }

    public async Task<ActualizarWriteupResultado> EjecutarAsync(
        ActualizarWriteupSolicitud solicitud,
        CancellationToken cancellationToken = default)
    {
        if (solicitud.WriteupId == Guid.Empty)
            throw new ArgumentException("El writeupId debe ser un Guid válido.", nameof(solicitud));

        if (solicitud.UsuarioId == Guid.Empty)
            throw new ArgumentException("El usuarioId debe ser un Guid válido.", nameof(solicitud));

        var usuario = await _usuarios.ObtenerPorIdAsync(solicitud.UsuarioId, cancellationToken);
        if (usuario is null)
            return ActualizarWriteupResultado.UsuarioNoEncontrado();

        var writeup = await _writeups.ObtenerPorIdAsync(solicitud.WriteupId, cancellationToken);
        if (writeup is null)
            return ActualizarWriteupResultado.WriteupNoEncontrado();

        if (writeup.UsuarioId != solicitud.UsuarioId)
            return ActualizarWriteupResultado.UsuarioNoCoincide();

        writeup.CambiarTitulo(solicitud.Titulo);
        writeup.ActualizarPlataformaOrigen(solicitud.PlataformaOrigen);
        writeup.ActualizarUrl(solicitud.Url);
        writeup.ActualizarFecha(solicitud.Fecha);
        writeup.AvanzarMadurez(solicitud.EstadoMadurez);

        await _unitOfWork.GuardarCambiosAsync(cancellationToken);

        return ActualizarWriteupResultado.Actualizado();
    }
}
