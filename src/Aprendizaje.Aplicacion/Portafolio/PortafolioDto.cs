namespace Aprendizaje.Aplicacion.Portafolio;

public sealed record PortafolioDto(
    Guid UsuarioId,
    PortafolioResumenDto Resumen,
    IReadOnlyCollection<PortafolioProyectoDto> Proyectos,
    IReadOnlyCollection<PortafolioLaboratorioDto> Laboratorios,
    IReadOnlyCollection<PortafolioWriteupDto> Writeups,
    IReadOnlyCollection<PortafolioArtefactoTecnicoDto> ArtefactosTecnicos,
    IReadOnlyCollection<PortafolioCertificacionObtenidaDto> CertificacionesObtenidas);
