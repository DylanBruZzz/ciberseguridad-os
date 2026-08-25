using Aprendizaje.Aplicacion.Comun;
using Aprendizaje.Aplicacion.Evidence.Notas.Comun;
using Aprendizaje.Dominio.Evidence;
using Aprendizaje.Dominio.Evidence.Repositorios;

namespace Aprendizaje.Aplicacion.Evidence.Notas.CrearNotaParaWriteup;

public sealed class CrearNotaParaWriteupCasoUso
{
    private readonly INotaRepository _notas;
    private readonly IWriteupRepository _writeups;
    private readonly IUnitOfWork _unitOfWork;

    public CrearNotaParaWriteupCasoUso(
        INotaRepository notas,
        IWriteupRepository writeups,
        IUnitOfWork unitOfWork)
    {
        _notas = notas;
        _writeups = writeups;
        _unitOfWork = unitOfWork;
    }

    public async Task<CrearNotaResultado> EjecutarAsync(
        CrearNotaParaWriteupSolicitud solicitud,
        CancellationToken cancellationToken = default)
    {
        if (solicitud.UsuarioId == Guid.Empty)
            throw new ArgumentException("El usuarioId debe ser un Guid válido.", nameof(solicitud));

        if (solicitud.WriteupId == Guid.Empty)
            throw new ArgumentException("El writeupId debe ser un Guid válido.", nameof(solicitud));

        var writeup = await _writeups.ObtenerPorIdAsync(solicitud.WriteupId, cancellationToken);

        if (writeup is null)
            return CrearNotaResultado.PadreNoEncontrado();

        if (writeup.UsuarioId != solicitud.UsuarioId)
            return CrearNotaResultado.UsuarioNoCoincide();

        var nota = Nota.SobreWriteup(solicitud.UsuarioId, solicitud.WriteupId, solicitud.Texto, solicitud.Tipo);

        _notas.Agregar(nota);
        await _unitOfWork.GuardarCambiosAsync(cancellationToken);

        return CrearNotaResultado.Creada(nota);
    }
}
