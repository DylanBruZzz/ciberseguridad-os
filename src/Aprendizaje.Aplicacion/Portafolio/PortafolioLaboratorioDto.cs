using Aprendizaje.Dominio.Evidence;

namespace Aprendizaje.Aplicacion.Portafolio;

public sealed record PortafolioLaboratorioDto(
    Guid LaboratorioId,
    string Nombre,
    string? Objetivo,
    string? EntornoVms,
    string? Hallazgos,
    int? TiempoInvertidoMinutos,
    DateOnly? Fecha,
    EstadoMadurez EstadoMadurez,
    IReadOnlyCollection<PortafolioTemaDto> Temas,
    IReadOnlyCollection<PortafolioHerramientaDto> Herramientas);
