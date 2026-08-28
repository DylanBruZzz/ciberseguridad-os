using Aprendizaje.Aplicacion.Comun;
using Aprendizaje.Dominio.Nucleo.Repositorios;
using Aprendizaje.Dominio.Resource.Repositorios;
using Aprendizaje.Dominio.Resource.ValueObjects;

namespace Aprendizaje.Aplicacion.Resource.Recursos.ActualizarRecurso;

public sealed class ActualizarRecursoCasoUso
{
    private readonly IRecursoRepository _recursos;
    private readonly IUsuarioRepository _usuarios;
    private readonly IUnitOfWork _unitOfWork;

    public ActualizarRecursoCasoUso(
        IRecursoRepository recursos,
        IUsuarioRepository usuarios,
        IUnitOfWork unitOfWork)
    {
        _recursos = recursos;
        _usuarios = usuarios;
        _unitOfWork = unitOfWork;
    }

    public async Task<ActualizarRecursoResultado> EjecutarAsync(
        ActualizarRecursoSolicitud solicitud,
        CancellationToken cancellationToken = default)
    {
        if (solicitud.RecursoId == Guid.Empty)
            throw new ArgumentException("El recursoId debe ser un Guid válido.", nameof(solicitud));

        if (solicitud.UsuarioId == Guid.Empty)
            throw new ArgumentException("El usuarioId debe ser un Guid válido.", nameof(solicitud));

        var usuario = await _usuarios.ObtenerPorIdAsync(solicitud.UsuarioId, cancellationToken);

        if (usuario is null)
            return ActualizarRecursoResultado.UsuarioNoEncontrado();

        var recurso = await _recursos.ObtenerPorIdAsync(solicitud.RecursoId, cancellationToken);

        if (recurso is null)
            return ActualizarRecursoResultado.RecursoNoEncontrado();

        if (recurso.UsuarioId != solicitud.UsuarioId)
            return ActualizarRecursoResultado.UsuarioNoCoincide();

        recurso.CambiarTitulo(solicitud.Titulo);
        recurso.ActualizarUrl(solicitud.Url);
        recurso.CambiarEstado(solicitud.Estado);
        recurso.CalificarCon(solicitud.Rating.HasValue ? RatingRecurso.Crear(solicitud.Rating.Value) : null);
        recurso.ActualizarNotas(solicitud.Notas);
        recurso.RegistrarUsoDeIA(solicitud.HerramientaIA, solicitud.PromptsUtilizados);

        await _unitOfWork.GuardarCambiosAsync(cancellationToken);

        return ActualizarRecursoResultado.Actualizado();
    }
}
