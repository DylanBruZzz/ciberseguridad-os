using Aprendizaje.Aplicacion.Comun;
using Aprendizaje.Dominio.Evidence.Repositorios;
using Aprendizaje.Dominio.Nucleo.Repositorios;

namespace Aprendizaje.Aplicacion.Evidence.Proyectos.ActualizarProyecto;

public sealed class ActualizarProyectoCasoUso
{
    private readonly IProyectoRepository _proyectos;
    private readonly IUsuarioRepository _usuarios;
    private readonly IUnitOfWork _unitOfWork;

    public ActualizarProyectoCasoUso(
        IProyectoRepository proyectos,
        IUsuarioRepository usuarios,
        IUnitOfWork unitOfWork)
    {
        _proyectos = proyectos;
        _usuarios = usuarios;
        _unitOfWork = unitOfWork;
    }

    public async Task<ActualizarProyectoResultado> EjecutarAsync(
        ActualizarProyectoSolicitud solicitud,
        CancellationToken cancellationToken = default)
    {
        if (solicitud.ProyectoId == Guid.Empty)
            throw new ArgumentException("El proyectoId debe ser un Guid válido.", nameof(solicitud));

        if (solicitud.UsuarioId == Guid.Empty)
            throw new ArgumentException("El usuarioId debe ser un Guid válido.", nameof(solicitud));

        var usuario = await _usuarios.ObtenerPorIdAsync(solicitud.UsuarioId, cancellationToken);
        if (usuario is null)
            return ActualizarProyectoResultado.UsuarioNoEncontrado();

        var proyecto = await _proyectos.ObtenerPorIdAsync(solicitud.ProyectoId, cancellationToken);
        if (proyecto is null)
            return ActualizarProyectoResultado.ProyectoNoEncontrado();

        if (proyecto.UsuarioId != solicitud.UsuarioId)
            return ActualizarProyectoResultado.UsuarioNoCoincide();

        proyecto.CambiarNombre(solicitud.Nombre);
        proyecto.ActualizarDescripcion(solicitud.Descripcion);
        proyecto.AvanzarEstado(solicitud.Estado);
        proyecto.AvanzarMadurez(solicitud.EstadoMadurez);
        proyecto.ActualizarRepositorioUrl(solicitud.RepositorioUrl);
        proyecto.ActualizarFechas(solicitud.FechaInicio, solicitud.FechaFin);

        await _unitOfWork.GuardarCambiosAsync(cancellationToken);

        return ActualizarProyectoResultado.Actualizado();
    }
}
