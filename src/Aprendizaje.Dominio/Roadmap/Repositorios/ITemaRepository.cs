using Aprendizaje.Dominio.Roadmap;

namespace Aprendizaje.Dominio.Roadmap.Repositorios;

public interface ITemaRepository
{
    Task<Tema?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default);

    void Agregar(Tema tema);
}
