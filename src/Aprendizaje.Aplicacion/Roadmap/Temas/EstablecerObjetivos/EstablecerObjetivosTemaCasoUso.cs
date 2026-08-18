using Aprendizaje.Aplicacion.Comun;
using Aprendizaje.Dominio.Roadmap.Repositorios;

namespace Aprendizaje.Aplicacion.Roadmap.Temas.EstablecerObjetivos;

public sealed class EstablecerObjetivosTemaCasoUso
{
    private readonly ITemaRepository _temas;
    private readonly IUnitOfWork _unitOfWork;

    public EstablecerObjetivosTemaCasoUso(ITemaRepository temas, IUnitOfWork unitOfWork)
    {
        _temas = temas;
        _unitOfWork = unitOfWork;
    }

    public async Task<EstablecerObjetivosTemaResultado> EjecutarAsync(
        EstablecerObjetivosTemaSolicitud solicitud,
        CancellationToken cancellationToken = default)
    {
        if (solicitud.Objetivos is null)
            throw new ArgumentException("La lista de objetivos no puede ser null.", nameof(solicitud));

        var tema = await _temas.ObtenerPorIdAsync(solicitud.TemaId, cancellationToken);

        if (tema is null)
            return EstablecerObjetivosTemaResultado.NoEncontrado();

        tema.EstablecerObjetivos(solicitud.Objetivos);

        await _unitOfWork.GuardarCambiosAsync(cancellationToken);

        return EstablecerObjetivosTemaResultado.Actualizado();
    }
}
