using Aprendizaje.Dominio.Roadmap;

namespace Aprendizaje.Dominio.Roadmap.Repositorios;

public interface ICompetenciaRepository
{
    Task<Competencia?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Competencia>> ListarPorUsuarioAsync(
        Guid usuarioId,
        CancellationToken cancellationToken = default);

    Task<bool> ExisteVinculoTemaAsync(
        Guid competenciaId,
        Guid temaId,
        CancellationToken cancellationToken = default);

    void Agregar(Competencia competencia);

    void VincularTema(Guid competenciaId, Guid temaId);
}
