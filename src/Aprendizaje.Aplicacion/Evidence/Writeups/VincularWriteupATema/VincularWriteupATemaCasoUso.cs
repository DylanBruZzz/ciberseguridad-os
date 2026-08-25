using Aprendizaje.Aplicacion.Comun;
using Aprendizaje.Dominio.Evidence.Repositorios;
using Aprendizaje.Dominio.Roadmap.Repositorios;

namespace Aprendizaje.Aplicacion.Evidence.Writeups.VincularWriteupATema;

public sealed class VincularWriteupATemaCasoUso
{
    private readonly IWriteupRepository _writeups;
    private readonly ITemaRepository _temas;
    private readonly IUnitOfWork _unitOfWork;

    public VincularWriteupATemaCasoUso(
        IWriteupRepository writeups,
        ITemaRepository temas,
        IUnitOfWork unitOfWork)
    {
        _writeups = writeups;
        _temas = temas;
        _unitOfWork = unitOfWork;
    }

    public async Task<VincularWriteupATemaResultado> EjecutarAsync(
        VincularWriteupATemaSolicitud solicitud,
        CancellationToken cancellationToken = default)
    {
        if (solicitud.WriteupId == Guid.Empty)
            throw new ArgumentException("El writeupId debe ser un Guid válido.", nameof(solicitud));

        if (solicitud.TemaId == Guid.Empty)
            throw new ArgumentException("El temaId debe ser un Guid válido.", nameof(solicitud));

        var writeup = await _writeups.ObtenerPorIdAsync(solicitud.WriteupId, cancellationToken);

        if (writeup is null)
            return VincularWriteupATemaResultado.WriteupNoEncontrado();

        var tema = await _temas.ObtenerPorIdAsync(solicitud.TemaId, cancellationToken);

        if (tema is null)
            return VincularWriteupATemaResultado.TemaNoEncontrado();

        if (writeup.UsuarioId != tema.UsuarioId)
            return VincularWriteupATemaResultado.UsuarioNoCoincide();

        var yaExiste = await _writeups.ExisteVinculoTemaAsync(
            solicitud.WriteupId,
            solicitud.TemaId,
            cancellationToken);

        if (!yaExiste)
        {
            _writeups.VincularTema(solicitud.WriteupId, solicitud.TemaId);
            await _unitOfWork.GuardarCambiosAsync(cancellationToken);
        }

        return VincularWriteupATemaResultado.Actualizado();
    }
}
