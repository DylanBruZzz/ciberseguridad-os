namespace Aprendizaje.Aplicacion.Roadmap.Temas.EstablecerObjetivos;

public sealed record EstablecerObjetivosTemaSolicitud(
    Guid TemaId,
    IReadOnlyCollection<string> Objetivos);
