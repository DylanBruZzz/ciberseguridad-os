using Aprendizaje.Aplicacion.Comun;
using Aprendizaje.Dominio.Evidence.Repositorios;
using Aprendizaje.Dominio.Nucleo.Repositorios;

namespace Aprendizaje.Aplicacion.Evidence.Proyectos.EliminarProyecto;

public sealed class EliminarProyectoCasoUso
{
    private readonly IProyectoRepository _proyectos;
    private readonly IUsuarioRepository _usuarios;
    private readonly IUnitOfWork _unitOfWork;

    public EliminarProyectoCasoUso(
        IProyectoRepository proyectos,
        IUsuarioRepository usuarios,
        IUnitOfWork unitOfWork)
    {
        _proyectos = proyectos;
        _usuarios = usuarios;
        _unitOfWork = unitOfWork;
    }

    public async Task<EliminarProyectoResultado> EjecutarAsync(
        EliminarProyectoSolicitud solicitud,
        CancellationToken cancellationToken = default)
    {
        if (solicitud.ProyectoId == Guid.Empty)
            throw new ArgumentException("El proyectoId debe ser un Guid válido.", nameof(solicitud));

        if (solicitud.UsuarioId == Guid.Empty)
            throw new ArgumentException("El usuarioId debe ser un Guid válido.", nameof(solicitud));

        var usuario = await _usuarios.ObtenerPorIdAsync(solicitud.UsuarioId, cancellationToken);
        if (usuario is null)
            return EliminarProyectoResultado.UsuarioNoEncontrado();

        var proyecto = await _proyectos.ObtenerPorIdAsync(solicitud.ProyectoId, cancellationToken);
        if (proyecto is null)
            return EliminarProyectoResultado.ProyectoNoEncontrado();

        if (proyecto.UsuarioId != solicitud.UsuarioId)
            return EliminarProyectoResultado.UsuarioNoCoincide();

        proyecto.MarcarComoEliminado();

        await _unitOfWork.GuardarCambiosAsync(cancellationToken);

        return EliminarProyectoResultado.Eliminado();
    }
}
