namespace Aprendizaje.Dominio.Evidence.Repositorios;

public interface ILaboratorioRepository
{
    Task<Laboratorio?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Laboratorio>> ListarPorUsuarioAsync(
        Guid usuarioId,
        CancellationToken cancellationToken = default);

    Task<bool> ExisteVinculoTemaAsync(
        Guid laboratorioId,
        Guid temaId,
        CancellationToken cancellationToken = default);

    Task<bool> ExisteVinculoHerramientaAsync(
        Guid laboratorioId,
        Guid herramientaId,
        CancellationToken cancellationToken = default);

    void Agregar(Laboratorio laboratorio);

    void VincularTema(Guid laboratorioId, Guid temaId);

    void VincularHerramienta(Guid laboratorioId, Guid herramientaId);
}
