namespace Aprendizaje.Dominio.Evidence.Repositorios;

public interface IArtefactoTecnicoRepository
{
    Task<ArtefactoTecnico?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<ArtefactoTecnico>> ListarPorUsuarioAsync(
        Guid usuarioId,
        CancellationToken cancellationToken = default);

    Task<bool> ExisteVinculoTemaAsync(
        Guid artefactoTecnicoId,
        Guid temaId,
        CancellationToken cancellationToken = default);

    Task<bool> ExisteVinculoHerramientaAsync(
        Guid artefactoTecnicoId,
        Guid herramientaId,
        CancellationToken cancellationToken = default);

    void Agregar(ArtefactoTecnico artefactoTecnico);

    void VincularTema(Guid artefactoTecnicoId, Guid temaId);

    void VincularHerramienta(Guid artefactoTecnicoId, Guid herramientaId);
}
