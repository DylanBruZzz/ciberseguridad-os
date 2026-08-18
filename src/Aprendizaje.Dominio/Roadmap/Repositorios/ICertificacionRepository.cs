using Aprendizaje.Dominio.Roadmap;

namespace Aprendizaje.Dominio.Roadmap.Repositorios;

public interface ICertificacionRepository
{
    Task<Certificacion?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default);

    void Agregar(Certificacion certificacion);
}
