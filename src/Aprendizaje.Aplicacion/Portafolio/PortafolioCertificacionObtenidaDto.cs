using Aprendizaje.Dominio.Evidence;
using Aprendizaje.Dominio.Roadmap;

namespace Aprendizaje.Aplicacion.Portafolio;

public sealed record PortafolioCertificacionObtenidaDto(
    Guid CertificacionObtenidaId,
    Guid CertificacionId,
    string Nombre,
    string? Proveedor,
    TipoCosto TipoCosto,
    DateOnly FechaObtencion,
    string? EvidenciaUrl,
    EstadoMadurez EstadoMadurez);
