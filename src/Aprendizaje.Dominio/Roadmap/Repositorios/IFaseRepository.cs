using Aprendizaje.Dominio.Roadmap;

namespace Aprendizaje.Dominio.Roadmap.Repositorios;

public interface IFaseRepository
{
    Task<Fase?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Fase>> ListarPorUsuarioAsync(Guid usuarioId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Fase>> ListarPorUsuarioParaImportacionAsync(
        Guid usuarioId,
        CancellationToken cancellationToken = default);

    void Agregar(Fase fase);
}
