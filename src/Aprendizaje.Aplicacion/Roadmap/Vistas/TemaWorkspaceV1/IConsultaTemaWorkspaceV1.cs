namespace Aprendizaje.Aplicacion.Roadmap.Vistas.TemaWorkspaceV1;

public interface IConsultaTemaWorkspaceV1
{
    Task<TemaWorkspaceV1Dto?> ObtenerAsync(
        Guid usuarioId,
        Guid temaId,
        DateTime ahoraUtc,
        CancellationToken cancellationToken = default);
}
