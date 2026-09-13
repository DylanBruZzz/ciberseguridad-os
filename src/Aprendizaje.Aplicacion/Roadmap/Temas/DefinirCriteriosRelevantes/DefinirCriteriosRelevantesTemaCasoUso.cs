using Aprendizaje.Aplicacion.Comun;
using Aprendizaje.Dominio.Roadmap;
using Aprendizaje.Dominio.Roadmap.Repositorios;

namespace Aprendizaje.Aplicacion.Roadmap.Temas.DefinirCriteriosRelevantes;

public sealed class DefinirCriteriosRelevantesTemaCasoUso
{
    private readonly ITemaRepository _temas;
    private readonly IUnitOfWork _unitOfWork;

    public DefinirCriteriosRelevantesTemaCasoUso(ITemaRepository temas, IUnitOfWork unitOfWork)
    {
        _temas = temas;
        _unitOfWork = unitOfWork;
    }

    public async Task<DefinirCriteriosRelevantesTemaResultado> EjecutarAsync(
        DefinirCriteriosRelevantesTemaSolicitud solicitud,
        CancellationToken cancellationToken = default)
    {
        if (solicitud.TemaId == Guid.Empty)
            throw new ArgumentException("El temaId debe ser un Guid válido.", nameof(solicitud));

        if (solicitud.Criterios is null)
            throw new ArgumentException("La lista de criterios no puede ser null.", nameof(solicitud));

        var tema = await _temas.ObtenerPorIdAsync(solicitud.TemaId, cancellationToken);

        if (tema is null)
            return DefinirCriteriosRelevantesTemaResultado.TemaNoEncontrado();

        try
        {
            tema.DefinirCriteriosRelevantes(
                solicitud.Criterios
                    .Select(c => new DefinicionCriterioTema(c.Tipo, c.Descripcion))
                    .ToArray());
        }
        catch (InvalidOperationException)
        {
            return DefinirCriteriosRelevantesTemaResultado.ProgresoRegistrado();
        }

        await _unitOfWork.GuardarCambiosAsync(cancellationToken);

        return DefinirCriteriosRelevantesTemaResultado.Actualizado();
    }
}
