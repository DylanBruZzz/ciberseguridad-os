using Aprendizaje.Aplicacion.Comun;
using Aprendizaje.Dominio.Nucleo.Repositorios;
using Aprendizaje.Dominio.Resource.Repositorios;

namespace Aprendizaje.Aplicacion.Resource.Recursos.EliminarRecurso;

public sealed class EliminarRecursoCasoUso
{
    private readonly IRecursoRepository _recursos;
    private readonly IUsuarioRepository _usuarios;
    private readonly IUnitOfWork _unitOfWork;

    public EliminarRecursoCasoUso(
        IRecursoRepository recursos,
        IUsuarioRepository usuarios,
        IUnitOfWork unitOfWork)
    {
        _recursos = recursos;
        _usuarios = usuarios;
        _unitOfWork = unitOfWork;
    }

    public async Task<EliminarRecursoResultado> EjecutarAsync(
        EliminarRecursoSolicitud solicitud,
        CancellationToken cancellationToken = default)
    {
        if (solicitud.RecursoId == Guid.Empty)
            throw new ArgumentException("El recursoId debe ser un Guid válido.", nameof(solicitud));

        if (solicitud.UsuarioId == Guid.Empty)
            throw new ArgumentException("El usuarioId debe ser un Guid válido.", nameof(solicitud));

        var usuario = await _usuarios.ObtenerPorIdAsync(solicitud.UsuarioId, cancellationToken);

        if (usuario is null)
            return EliminarRecursoResultado.UsuarioNoEncontrado();

        var recurso = await _recursos.ObtenerPorIdAsync(solicitud.RecursoId, cancellationToken);

        if (recurso is null)
            return EliminarRecursoResultado.RecursoNoEncontrado();

        if (recurso.UsuarioId != solicitud.UsuarioId)
            return EliminarRecursoResultado.UsuarioNoCoincide();

        recurso.MarcarComoEliminado();

        await _unitOfWork.GuardarCambiosAsync(cancellationToken);

        return EliminarRecursoResultado.Eliminado();
    }
}
