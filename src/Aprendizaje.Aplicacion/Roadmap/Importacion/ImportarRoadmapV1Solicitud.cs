namespace Aprendizaje.Aplicacion.Roadmap.Importacion;

public sealed record ImportarRoadmapV1Solicitud(
    Guid UsuarioId,
    RoadmapV1Documento Documento);
