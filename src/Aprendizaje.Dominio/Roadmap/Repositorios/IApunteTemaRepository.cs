using Aprendizaje.Dominio.Roadmap;

namespace Aprendizaje.Dominio.Roadmap.Repositorios;

public interface IApunteTemaRepository
{
    Task<ApunteTema?> ObtenerPorTemaIdAsync(Guid temaId, CancellationToken cancellationToken = default);

    void Agregar(ApunteTema apunte);
}
