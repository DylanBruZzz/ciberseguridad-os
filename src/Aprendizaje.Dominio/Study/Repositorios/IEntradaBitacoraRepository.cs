namespace Aprendizaje.Dominio.Study.Repositorios;

public interface IEntradaBitacoraRepository
{
    Task<EntradaBitacora?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<EntradaBitacora>> ListarPorUsuarioAsync(
        Guid usuarioId,
        CancellationToken cancellationToken = default);

    void Agregar(EntradaBitacora entrada);
}
