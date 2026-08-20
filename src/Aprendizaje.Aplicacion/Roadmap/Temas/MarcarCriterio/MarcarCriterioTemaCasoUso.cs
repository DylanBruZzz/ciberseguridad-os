using Aprendizaje.Aplicacion.Comun;
using Aprendizaje.Dominio.Roadmap.Repositorios;

namespace Aprendizaje.Aplicacion.Roadmap.Temas.MarcarCriterio;

public sealed class MarcarCriterioTemaCasoUso
{
    private readonly ITemaRepository _temas;
    private readonly IUnitOfWork _unitOfWork;

    public MarcarCriterioTemaCasoUso(ITemaRepository temas, IUnitOfWork unitOfWork)
    {
        _temas = temas;
        _unitOfWork = unitOfWork;
    }

    public async Task<MarcarCriterioTemaResultado> EjecutarAsync(
        MarcarCriterioTemaSolicitud solicitud,
        CancellationToken cancellationToken = default)
    {
        if (solicitud.TemaId == Guid.Empty)
            throw new ArgumentException("El temaId debe ser un Guid válido.", nameof(solicitud));

        var tema = await _temas.ObtenerPorIdAsync(solicitud.TemaId, cancellationToken);

        if (tema is null)
            return MarcarCriterioTemaResultado.TemaNoEncontrado();

        try
        {
            tema.MarcarCriterio(solicitud.Tipo);
        }
        catch (InvalidOperationException)
        {
            return MarcarCriterioTemaResultado.CriterioNoDefinido();
        }

        await _unitOfWork.GuardarCambiosAsync(cancellationToken);

        return MarcarCriterioTemaResultado.Actualizado();
    }
}
