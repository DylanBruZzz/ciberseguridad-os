namespace Aprendizaje.Dominio.Resource.Repositorios;

public interface IRecursoRepository
{
    Task<Recurso?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Recurso>> ListarPorUsuarioAsync(
        Guid usuarioId,
        CancellationToken cancellationToken = default);

    Task<bool> ExisteVinculoTemaAsync(
        Guid recursoId,
        Guid temaId,
        CancellationToken cancellationToken = default);

    void Agregar(Recurso recurso);

    void VincularTema(Guid recursoId, Guid temaId);
}
