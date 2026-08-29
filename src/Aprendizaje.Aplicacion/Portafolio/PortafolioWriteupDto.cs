using Aprendizaje.Dominio.Evidence;

namespace Aprendizaje.Aplicacion.Portafolio;

public sealed record PortafolioWriteupDto(
    Guid WriteupId,
    string Titulo,
    string? PlataformaOrigen,
    string? Url,
    DateOnly? Fecha,
    EstadoMadurez EstadoMadurez,
    IReadOnlyCollection<PortafolioTemaDto> Temas);
