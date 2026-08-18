using Aprendizaje.Dominio.Roadmap;

namespace Aprendizaje.Dominio.Roadmap.Repositorios;

public interface ICompetenciaRepository
{
    Task<Competencia?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default);

    void Agregar(Competencia competencia);
}
