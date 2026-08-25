using Aprendizaje.Aplicacion.Comun;
using Aprendizaje.Dominio.Roadmap.Repositorios;

namespace Aprendizaje.Aplicacion.Roadmap.Competencias.VincularCompetenciaATema;

public sealed class VincularCompetenciaATemaCasoUso
{
    private readonly ICompetenciaRepository _competencias;
    private readonly ITemaRepository _temas;
    private readonly IUnitOfWork _unitOfWork;

    public VincularCompetenciaATemaCasoUso(
        ICompetenciaRepository competencias,
        ITemaRepository temas,
        IUnitOfWork unitOfWork)
    {
        _competencias = competencias;
        _temas = temas;
        _unitOfWork = unitOfWork;
    }

    public async Task<VincularCompetenciaATemaResultado> EjecutarAsync(
        VincularCompetenciaATemaSolicitud solicitud,
        CancellationToken cancellationToken = default)
    {
        if (solicitud.CompetenciaId == Guid.Empty)
            throw new ArgumentException("El competenciaId debe ser un Guid válido.", nameof(solicitud));

        if (solicitud.TemaId == Guid.Empty)
            throw new ArgumentException("El temaId debe ser un Guid válido.", nameof(solicitud));

        var competencia = await _competencias.ObtenerPorIdAsync(solicitud.CompetenciaId, cancellationToken);

        if (competencia is null)
            return VincularCompetenciaATemaResultado.CompetenciaNoEncontrada();

        var tema = await _temas.ObtenerPorIdAsync(solicitud.TemaId, cancellationToken);

        if (tema is null)
            return VincularCompetenciaATemaResultado.TemaNoEncontrado();

        if (competencia.UsuarioId != tema.UsuarioId)
            return VincularCompetenciaATemaResultado.UsuarioNoCoincide();

        var yaExiste = await _competencias.ExisteVinculoTemaAsync(
            solicitud.CompetenciaId,
            solicitud.TemaId,
            cancellationToken);

        if (!yaExiste)
        {
            _competencias.VincularTema(solicitud.CompetenciaId, solicitud.TemaId);
            await _unitOfWork.GuardarCambiosAsync(cancellationToken);
        }

        return VincularCompetenciaATemaResultado.Actualizado();
    }
}
