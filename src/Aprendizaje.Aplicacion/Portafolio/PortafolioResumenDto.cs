namespace Aprendizaje.Aplicacion.Portafolio;

public sealed record PortafolioResumenDto(
    int Total,
    int Proyectos,
    int Laboratorios,
    int Writeups,
    int ArtefactosTecnicos,
    int CertificacionesObtenidas,
    int ListosPortafolio,
    int Publicados);
