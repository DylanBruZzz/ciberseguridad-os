namespace Aprendizaje.Aplicacion.Roadmap.Temas.ActualizarPercepcion;

public sealed record ActualizarPercepcionTemaSolicitud(
    Guid TemaId,
    int? DificultadPercibida,
    int? Confianza);
