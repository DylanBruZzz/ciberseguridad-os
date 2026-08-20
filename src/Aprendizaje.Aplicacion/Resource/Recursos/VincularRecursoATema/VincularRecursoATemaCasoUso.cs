using Aprendizaje.Aplicacion.Comun;
using Aprendizaje.Dominio.Resource.Repositorios;
using Aprendizaje.Dominio.Roadmap.Repositorios;

namespace Aprendizaje.Aplicacion.Resource.Recursos.VincularRecursoATema;

public sealed class VincularRecursoATemaCasoUso
{
    private readonly IRecursoRepository _recursos;
    private readonly ITemaRepository _temas;
    private readonly IUnitOfWork _unitOfWork;

    public VincularRecursoATemaCasoUso(
        IRecursoRepository recursos,
        ITemaRepository temas,
        IUnitOfWork unitOfWork)
    {
        _recursos = recursos;
        _temas = temas;
        _unitOfWork = unitOfWork;
    }

    public async Task<VincularRecursoATemaResultado> EjecutarAsync(
        VincularRecursoATemaSolicitud solicitud,
        CancellationToken cancellationToken = default)
    {
        if (solicitud.RecursoId == Guid.Empty)
            throw new ArgumentException("El recursoId debe ser un Guid válido.", nameof(solicitud));

        if (solicitud.TemaId == Guid.Empty)
            throw new ArgumentException("El temaId debe ser un Guid válido.", nameof(solicitud));

        var recurso = await _recursos.ObtenerPorIdAsync(solicitud.RecursoId, cancellationToken);

        if (recurso is null)
            return VincularRecursoATemaResultado.RecursoNoEncontrado();

        var tema = await _temas.ObtenerPorIdAsync(solicitud.TemaId, cancellationToken);

        if (tema is null)
            return VincularRecursoATemaResultado.TemaNoEncontrado();

        if (recurso.UsuarioId != tema.UsuarioId)
            return VincularRecursoATemaResultado.UsuarioNoCoincide();

        var yaExiste = await _recursos.ExisteVinculoTemaAsync(
            solicitud.RecursoId,
            solicitud.TemaId,
            cancellationToken);

        if (!yaExiste)
        {
            _recursos.VincularTema(solicitud.RecursoId, solicitud.TemaId);
            await _unitOfWork.GuardarCambiosAsync(cancellationToken);
        }

        return VincularRecursoATemaResultado.Actualizado();
    }
}
