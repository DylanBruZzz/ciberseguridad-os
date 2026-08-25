namespace Aprendizaje.Dominio.Evidence.Repositorios;

public interface ICertificacionObtenidaRepository
{
    Task<CertificacionObtenida?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<CertificacionObtenida>> ListarPorUsuarioAsync(
        Guid usuarioId,
        CancellationToken cancellationToken = default);

    void Agregar(CertificacionObtenida certificacionObtenida);
}
