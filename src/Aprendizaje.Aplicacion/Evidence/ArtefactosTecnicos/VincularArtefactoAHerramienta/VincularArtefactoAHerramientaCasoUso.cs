using Aprendizaje.Aplicacion.Comun;
using Aprendizaje.Dominio.Evidence.Repositorios;
using Aprendizaje.Dominio.Study.Repositorios;

namespace Aprendizaje.Aplicacion.Evidence.ArtefactosTecnicos.VincularArtefactoAHerramienta;

public sealed class VincularArtefactoAHerramientaCasoUso
{
    private readonly IArtefactoTecnicoRepository _artefactosTecnicos;
    private readonly IHerramientaRepository _herramientas;
    private readonly IUnitOfWork _unitOfWork;

    public VincularArtefactoAHerramientaCasoUso(
        IArtefactoTecnicoRepository artefactosTecnicos,
        IHerramientaRepository herramientas,
        IUnitOfWork unitOfWork)
    {
        _artefactosTecnicos = artefactosTecnicos;
        _herramientas = herramientas;
        _unitOfWork = unitOfWork;
    }

    public async Task<VincularArtefactoAHerramientaResultado> EjecutarAsync(
        VincularArtefactoAHerramientaSolicitud solicitud,
        CancellationToken cancellationToken = default)
    {
        if (solicitud.ArtefactoTecnicoId == Guid.Empty)
            throw new ArgumentException("El artefactoTecnicoId debe ser un Guid válido.", nameof(solicitud));

        if (solicitud.HerramientaId == Guid.Empty)
            throw new ArgumentException("El herramientaId debe ser un Guid válido.", nameof(solicitud));

        var artefactoTecnico = await _artefactosTecnicos.ObtenerPorIdAsync(
            solicitud.ArtefactoTecnicoId,
            cancellationToken);

        if (artefactoTecnico is null)
            return VincularArtefactoAHerramientaResultado.ArtefactoNoEncontrado();

        var herramienta = await _herramientas.ObtenerPorIdAsync(solicitud.HerramientaId, cancellationToken);

        if (herramienta is null)
            return VincularArtefactoAHerramientaResultado.HerramientaNoEncontrada();

        var yaExiste = await _artefactosTecnicos.ExisteVinculoHerramientaAsync(
            solicitud.ArtefactoTecnicoId,
            solicitud.HerramientaId,
            cancellationToken);

        if (!yaExiste)
        {
            _artefactosTecnicos.VincularHerramienta(solicitud.ArtefactoTecnicoId, solicitud.HerramientaId);
            await _unitOfWork.GuardarCambiosAsync(cancellationToken);
        }

        return VincularArtefactoAHerramientaResultado.Actualizado();
    }
}
