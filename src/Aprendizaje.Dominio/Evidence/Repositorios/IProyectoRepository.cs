namespace Aprendizaje.Dominio.Evidence.Repositorios;

public interface IProyectoRepository
{
    Task<Proyecto?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Proyecto>> ListarPorUsuarioAsync(
        Guid usuarioId,
        CancellationToken cancellationToken = default);

    Task<bool> ExisteVinculoTemaAsync(
        Guid proyectoId,
        Guid temaId,
        CancellationToken cancellationToken = default);

    Task<bool> ExisteVinculoHerramientaAsync(
        Guid proyectoId,
        Guid herramientaId,
        CancellationToken cancellationToken = default);

    void Agregar(Proyecto proyecto);

    void VincularTema(Guid proyectoId, Guid temaId);

    void VincularHerramienta(Guid proyectoId, Guid herramientaId);
}
