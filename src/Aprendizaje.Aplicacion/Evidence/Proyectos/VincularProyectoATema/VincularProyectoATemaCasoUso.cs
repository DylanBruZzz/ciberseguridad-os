using Aprendizaje.Aplicacion.Comun;
using Aprendizaje.Dominio.Evidence.Repositorios;
using Aprendizaje.Dominio.Roadmap.Repositorios;

namespace Aprendizaje.Aplicacion.Evidence.Proyectos.VincularProyectoATema;

public sealed class VincularProyectoATemaCasoUso
{
    private readonly IProyectoRepository _proyectos;
    private readonly ITemaRepository _temas;
    private readonly IUnitOfWork _unitOfWork;

    public VincularProyectoATemaCasoUso(
        IProyectoRepository proyectos,
        ITemaRepository temas,
        IUnitOfWork unitOfWork)
    {
        _proyectos = proyectos;
        _temas = temas;
        _unitOfWork = unitOfWork;
    }

    public async Task<VincularProyectoATemaResultado> EjecutarAsync(
        VincularProyectoATemaSolicitud solicitud,
        CancellationToken cancellationToken = default)
    {
        if (solicitud.ProyectoId == Guid.Empty)
            throw new ArgumentException("El proyectoId debe ser un Guid válido.", nameof(solicitud));

        if (solicitud.TemaId == Guid.Empty)
            throw new ArgumentException("El temaId debe ser un Guid válido.", nameof(solicitud));

        var proyecto = await _proyectos.ObtenerPorIdAsync(solicitud.ProyectoId, cancellationToken);

        if (proyecto is null)
            return VincularProyectoATemaResultado.ProyectoNoEncontrado();

        var tema = await _temas.ObtenerPorIdAsync(solicitud.TemaId, cancellationToken);

        if (tema is null)
            return VincularProyectoATemaResultado.TemaNoEncontrado();

        if (proyecto.UsuarioId != tema.UsuarioId)
            return VincularProyectoATemaResultado.UsuarioNoCoincide();

        var yaExiste = await _proyectos.ExisteVinculoTemaAsync(
            solicitud.ProyectoId,
            solicitud.TemaId,
            cancellationToken);

        if (!yaExiste)
        {
            _proyectos.VincularTema(solicitud.ProyectoId, solicitud.TemaId);
            await _unitOfWork.GuardarCambiosAsync(cancellationToken);
        }

        return VincularProyectoATemaResultado.Actualizado();
    }
}
