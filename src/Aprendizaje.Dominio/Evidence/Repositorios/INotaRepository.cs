namespace Aprendizaje.Dominio.Evidence.Repositorios;

public interface INotaRepository
{
    Task<Nota?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Nota>> ListarPorUsuarioAsync(
        Guid usuarioId,
        CancellationToken cancellationToken = default);

    void Agregar(Nota nota);
}
