using Aprendizaje.Aplicacion.Comun;
using Aprendizaje.Dominio.Roadmap.Repositorios;

namespace Aprendizaje.Aplicacion.Roadmap.Temas.DesmarcarCriterio;

public sealed class DesmarcarCriterioTemaCasoUso
{
    private readonly ITemaRepository _temas;
    private readonly IUnitOfWork _unitOfWork;

    public DesmarcarCriterioTemaCasoUso(ITemaRepository temas, IUnitOfWork unitOfWork)
    {
        _temas = temas;
        _unitOfWork = unitOfWork;
    }

    public async Task<DesmarcarCriterioTemaResultado> EjecutarAsync(
        DesmarcarCriterioTemaSolicitud solicitud,
        CancellationToken cancellationToken = default)
    {
        if (solicitud.TemaId == Guid.Empty)
            throw new ArgumentException("El temaId debe ser un Guid válido.", nameof(solicitud));

        var tema = await _temas.ObtenerPorIdAsync(solicitud.TemaId, cancellationToken);

        if (tema is null)
            return DesmarcarCriterioTemaResultado.TemaNoEncontrado();

        try
        {
            tema.DesmarcarCriterio(solicitud.Tipo);
        }
        catch (InvalidOperationException)
        {
            return DesmarcarCriterioTemaResultado.CriterioNoDefinido();
        }

        await _unitOfWork.GuardarCambiosAsync(cancellationToken);

        return DesmarcarCriterioTemaResultado.Actualizado();
    }
}
