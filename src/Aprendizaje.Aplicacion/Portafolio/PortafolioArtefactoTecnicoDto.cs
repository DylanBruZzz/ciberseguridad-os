using Aprendizaje.Dominio.Evidence;

namespace Aprendizaje.Aplicacion.Portafolio;

public sealed record PortafolioArtefactoTecnicoDto(
    Guid ArtefactoTecnicoId,
    TipoArtefacto TipoArtefacto,
    string Nombre,
    string? ContenidoOUrl,
    string? LenguajeTecnologia,
    EstadoMadurez EstadoMadurez,
    IReadOnlyCollection<PortafolioTemaDto> Temas,
    IReadOnlyCollection<PortafolioHerramientaDto> Herramientas);
