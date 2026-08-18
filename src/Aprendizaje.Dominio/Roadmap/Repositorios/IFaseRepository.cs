using Aprendizaje.Dominio.Roadmap;

namespace Aprendizaje.Dominio.Roadmap.Repositorios;

public interface IFaseRepository
{
    Task<Fase?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default);

    void Agregar(Fase fase);
}
