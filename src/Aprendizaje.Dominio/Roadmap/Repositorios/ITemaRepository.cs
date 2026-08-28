using Aprendizaje.Dominio.Roadmap;

namespace Aprendizaje.Dominio.Roadmap.Repositorios;

public interface ITemaRepository
{
    Task<Tema?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Tema>> ListarPorUsuarioAsync(Guid usuarioId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Tema>> ListarPorUsuarioParaImportacionAsync(
        Guid usuarioId,
        CancellationToken cancellationToken = default);

    void Agregar(Tema tema);
}
