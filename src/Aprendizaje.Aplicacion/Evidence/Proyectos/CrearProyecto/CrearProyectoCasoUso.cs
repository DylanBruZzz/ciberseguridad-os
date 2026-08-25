using Aprendizaje.Aplicacion.Comun;
using Aprendizaje.Dominio.Evidence;
using Aprendizaje.Dominio.Evidence.Repositorios;

namespace Aprendizaje.Aplicacion.Evidence.Proyectos.CrearProyecto;

public sealed class CrearProyectoCasoUso
{
    private readonly IProyectoRepository _proyectos;
    private readonly IUnitOfWork _unitOfWork;

    public CrearProyectoCasoUso(IProyectoRepository proyectos, IUnitOfWork unitOfWork)
    {
        _proyectos = proyectos;
        _unitOfWork = unitOfWork;
    }

    public async Task<CrearProyectoResultado> EjecutarAsync(
        CrearProyectoSolicitud solicitud,
        CancellationToken cancellationToken = default)
    {
        var proyecto = Proyecto.Crear(solicitud.UsuarioId, solicitud.Nombre);

        _proyectos.Agregar(proyecto);

        await _unitOfWork.GuardarCambiosAsync(cancellationToken);

        return new CrearProyectoResultado(
            proyecto.Id,
            proyecto.UsuarioId,
            proyecto.Nombre,
            proyecto.Descripcion,
            proyecto.Estado,
            proyecto.EstadoMadurez,
            proyecto.RepositorioUrl,
            proyecto.FechaInicio,
            proyecto.FechaFin);
    }
}
