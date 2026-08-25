namespace Aprendizaje.Aplicacion.Roadmap.Temas.ActualizarPlanificacion;

public sealed record ActualizarPlanificacionTemaSolicitud(
    Guid TemaId,
    DateOnly? FechaInicio,
    DateOnly? FechaFin);
