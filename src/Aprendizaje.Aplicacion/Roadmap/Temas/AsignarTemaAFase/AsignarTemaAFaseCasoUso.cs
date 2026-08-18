using Aprendizaje.Aplicacion.Comun;
using Aprendizaje.Dominio.Roadmap.Repositorios;

namespace Aprendizaje.Aplicacion.Roadmap.Temas.AsignarTemaAFase;

public sealed class AsignarTemaAFaseCasoUso
{
    private readonly ITemaRepository _temas;
    private readonly IFaseRepository _fases;
    private readonly IUnitOfWork _unitOfWork;

    public AsignarTemaAFaseCasoUso(
        ITemaRepository temas,
        IFaseRepository fases,
        IUnitOfWork unitOfWork)
    {
        _temas = temas;
        _fases = fases;
        _unitOfWork = unitOfWork;
    }

    public async Task<AsignarTemaAFaseResultado> EjecutarAsync(
        AsignarTemaAFaseSolicitud solicitud,
        CancellationToken cancellationToken = default)
    {
        if (solicitud.TemaId == Guid.Empty)
            throw new ArgumentException("El temaId debe ser un Guid válido.", nameof(solicitud));

        if (solicitud.FaseId == Guid.Empty)
            throw new ArgumentException("El faseId debe ser un Guid válido.", nameof(solicitud));

        var tema = await _temas.ObtenerPorIdAsync(solicitud.TemaId, cancellationToken);

        if (tema is null)
            return AsignarTemaAFaseResultado.TemaNoEncontrado();

        var fase = await _fases.ObtenerPorIdAsync(solicitud.FaseId, cancellationToken);

        if (fase is null)
            return AsignarTemaAFaseResultado.FaseNoEncontrada();

        if (tema.UsuarioId != fase.UsuarioId)
            return AsignarTemaAFaseResultado.UsuarioNoCoincide();

        tema.AsignarFase(fase.Id);

        await _unitOfWork.GuardarCambiosAsync(cancellationToken);

        return AsignarTemaAFaseResultado.Actualizado();
    }
}
