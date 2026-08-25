namespace Aprendizaje.Dominio.Evidence.Repositorios;

public interface IWriteupRepository
{
    Task<Writeup?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Writeup>> ListarPorUsuarioAsync(
        Guid usuarioId,
        CancellationToken cancellationToken = default);

    Task<bool> ExisteVinculoTemaAsync(
        Guid writeupId,
        Guid temaId,
        CancellationToken cancellationToken = default);

    void Agregar(Writeup writeup);

    void VincularTema(Guid writeupId, Guid temaId);
}
