namespace Aprendizaje.Dominio.Study.Repositorios;

public interface IHerramientaRepository
{
    Task<Herramienta?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Herramienta>> ListarAsync(CancellationToken cancellationToken = default);

    void Agregar(Herramienta herramienta);
}
