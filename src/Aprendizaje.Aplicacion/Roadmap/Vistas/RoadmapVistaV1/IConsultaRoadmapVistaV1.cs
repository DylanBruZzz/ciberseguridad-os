namespace Aprendizaje.Aplicacion.Roadmap.Vistas.RoadmapVistaV1;

public interface IConsultaRoadmapVistaV1
{
    Task<RoadmapVistaV1Dto> ObtenerAsync(
        Guid usuarioId,
        DateTime ahoraUtc,
        CancellationToken cancellationToken = default);
}
