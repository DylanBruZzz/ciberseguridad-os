namespace Aprendizaje.Aplicacion.Analytics.Competencias;

public interface IConsultaResumenCompetencias
{
    Task<IReadOnlyCollection<ResumenCompetenciaDto>> ListarAsync(
        Guid usuarioId,
        CancellationToken cancellationToken = default);
}
