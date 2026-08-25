using Aprendizaje.Aplicacion.Comun;
using Aprendizaje.Dominio.Roadmap.Repositorios;

namespace Aprendizaje.Aplicacion.Roadmap.Temas.ActualizarPlanificacion;

public sealed class ActualizarPlanificacionTemaCasoUso
{
    private readonly ITemaRepository _temas;
    private readonly IUnitOfWork _unitOfWork;

    public ActualizarPlanificacionTemaCasoUso(ITemaRepository temas, IUnitOfWork unitOfWork)
    {
        _temas = temas;
        _unitOfWork = unitOfWork;
    }

    public async Task<ActualizarPlanificacionTemaResultado> EjecutarAsync(
        ActualizarPlanificacionTemaSolicitud solicitud,
        CancellationToken cancellationToken = default)
    {
        if (solicitud.TemaId == Guid.Empty)
            throw new ArgumentException("El temaId debe ser un Guid válido.", nameof(solicitud));

        if (!solicitud.FechaInicio.HasValue && !solicitud.FechaFin.HasValue)
            throw new ArgumentException("Debe informar al menos una fecha de planificación.", nameof(solicitud));

        var tema = await _temas.ObtenerPorIdAsync(solicitud.TemaId, cancellationToken);

        if (tema is null)
            return ActualizarPlanificacionTemaResultado.NoEncontrado();

        if (solicitud.FechaInicio.HasValue)
            tema.IniciarEstudio(solicitud.FechaInicio.Value);

        if (solicitud.FechaFin.HasValue)
            tema.FinalizarEstudio(solicitud.FechaFin.Value);

        await _unitOfWork.GuardarCambiosAsync(cancellationToken);

        return ActualizarPlanificacionTemaResultado.Actualizado();
    }
}
