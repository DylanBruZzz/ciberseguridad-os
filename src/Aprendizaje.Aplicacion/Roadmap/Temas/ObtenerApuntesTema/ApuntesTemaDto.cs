namespace Aprendizaje.Aplicacion.Roadmap.Temas.ObtenerApuntesTema;

public sealed record ApuntesTemaDto(
    Guid TemaId,
    string Contenido,
    DateTime? FechaModificacionUtc);
