using Aprendizaje.Dominio.Roadmap;

namespace Aprendizaje.Dominio.Roadmap.Repositorios;

public interface ICertificacionRepository
{
    Task<Certificacion?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Certificacion>> ListarAsync(CancellationToken cancellationToken = default);

    Task<bool> ExisteVinculoTemaAsync(
        Guid certificacionId,
        Guid temaId,
        CancellationToken cancellationToken = default);

    void Agregar(Certificacion certificacion);

    void VincularTema(Guid certificacionId, Guid temaId);
}
