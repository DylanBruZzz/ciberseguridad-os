using Aprendizaje.Aplicacion.Comun;
using Aprendizaje.Dominio.Evidence.Repositorios;
using Aprendizaje.Dominio.Roadmap.Repositorios;

namespace Aprendizaje.Aplicacion.Evidence.ArtefactosTecnicos.VincularArtefactoATema;

public sealed class VincularArtefactoATemaCasoUso
{
    private readonly IArtefactoTecnicoRepository _artefactosTecnicos;
    private readonly ITemaRepository _temas;
    private readonly IUnitOfWork _unitOfWork;

    public VincularArtefactoATemaCasoUso(
        IArtefactoTecnicoRepository artefactosTecnicos,
        ITemaRepository temas,
        IUnitOfWork unitOfWork)
    {
        _artefactosTecnicos = artefactosTecnicos;
        _temas = temas;
        _unitOfWork = unitOfWork;
    }

    public async Task<VincularArtefactoATemaResultado> EjecutarAsync(
        VincularArtefactoATemaSolicitud solicitud,
        CancellationToken cancellationToken = default)
    {
        if (solicitud.ArtefactoTecnicoId == Guid.Empty)
            throw new ArgumentException("El artefactoTecnicoId debe ser un Guid válido.", nameof(solicitud));

        if (solicitud.TemaId == Guid.Empty)
            throw new ArgumentException("El temaId debe ser un Guid válido.", nameof(solicitud));

        var artefactoTecnico = await _artefactosTecnicos.ObtenerPorIdAsync(
            solicitud.ArtefactoTecnicoId,
            cancellationToken);

        if (artefactoTecnico is null)
            return VincularArtefactoATemaResultado.ArtefactoNoEncontrado();

        var tema = await _temas.ObtenerPorIdAsync(solicitud.TemaId, cancellationToken);

        if (tema is null)
            return VincularArtefactoATemaResultado.TemaNoEncontrado();

        if (artefactoTecnico.UsuarioId != tema.UsuarioId)
            return VincularArtefactoATemaResultado.UsuarioNoCoincide();

        var yaExiste = await _artefactosTecnicos.ExisteVinculoTemaAsync(
            solicitud.ArtefactoTecnicoId,
            solicitud.TemaId,
            cancellationToken);

        if (!yaExiste)
        {
            _artefactosTecnicos.VincularTema(solicitud.ArtefactoTecnicoId, solicitud.TemaId);
            await _unitOfWork.GuardarCambiosAsync(cancellationToken);
        }

        return VincularArtefactoATemaResultado.Actualizado();
    }
}
