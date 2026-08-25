using Aprendizaje.Aplicacion.Comun;
using Aprendizaje.Dominio.Evidence.Repositorios;
using Aprendizaje.Dominio.Study.Repositorios;

namespace Aprendizaje.Aplicacion.Evidence.Proyectos.VincularProyectoAHerramienta;

public sealed class VincularProyectoAHerramientaCasoUso
{
    private readonly IProyectoRepository _proyectos;
    private readonly IHerramientaRepository _herramientas;
    private readonly IUnitOfWork _unitOfWork;

    public VincularProyectoAHerramientaCasoUso(
        IProyectoRepository proyectos,
        IHerramientaRepository herramientas,
        IUnitOfWork unitOfWork)
    {
        _proyectos = proyectos;
        _herramientas = herramientas;
        _unitOfWork = unitOfWork;
    }

    public async Task<VincularProyectoAHerramientaResultado> EjecutarAsync(
        VincularProyectoAHerramientaSolicitud solicitud,
        CancellationToken cancellationToken = default)
    {
        if (solicitud.ProyectoId == Guid.Empty)
            throw new ArgumentException("El proyectoId debe ser un Guid válido.", nameof(solicitud));

        if (solicitud.HerramientaId == Guid.Empty)
            throw new ArgumentException("El herramientaId debe ser un Guid válido.", nameof(solicitud));

        var proyecto = await _proyectos.ObtenerPorIdAsync(solicitud.ProyectoId, cancellationToken);

        if (proyecto is null)
            return VincularProyectoAHerramientaResultado.ProyectoNoEncontrado();

        var herramienta = await _herramientas.ObtenerPorIdAsync(solicitud.HerramientaId, cancellationToken);

        if (herramienta is null)
            return VincularProyectoAHerramientaResultado.HerramientaNoEncontrada();

        var yaExiste = await _proyectos.ExisteVinculoHerramientaAsync(
            solicitud.ProyectoId,
            solicitud.HerramientaId,
            cancellationToken);

        if (!yaExiste)
        {
            _proyectos.VincularHerramienta(solicitud.ProyectoId, solicitud.HerramientaId);
            await _unitOfWork.GuardarCambiosAsync(cancellationToken);
        }

        return VincularProyectoAHerramientaResultado.Actualizado();
    }
}
