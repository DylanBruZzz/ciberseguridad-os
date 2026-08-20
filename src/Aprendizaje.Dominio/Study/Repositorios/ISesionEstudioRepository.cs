namespace Aprendizaje.Dominio.Study.Repositorios;

public interface ISesionEstudioRepository
{
    Task<SesionEstudio?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<SesionEstudio>> ListarPorUsuarioAsync(
        Guid usuarioId,
        CancellationToken cancellationToken = default);

    void Agregar(SesionEstudio sesion);
}
