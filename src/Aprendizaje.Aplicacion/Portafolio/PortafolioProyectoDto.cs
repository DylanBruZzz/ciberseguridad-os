using Aprendizaje.Dominio.Evidence;

namespace Aprendizaje.Aplicacion.Portafolio;

public sealed record PortafolioProyectoDto(
    Guid ProyectoId,
    string Nombre,
    string? Descripcion,
    EstadoProyecto Estado,
    EstadoMadurez EstadoMadurez,
    string? RepositorioUrl,
    DateOnly? FechaInicio,
    DateOnly? FechaFin,
    IReadOnlyCollection<PortafolioTemaDto> Temas,
    IReadOnlyCollection<PortafolioHerramientaDto> Herramientas);
