using Aprendizaje.Dominio.Evidence;

namespace Aprendizaje.Aplicacion.Evidence.CertificacionesObtenidas.ObtenerCertificacionObtenidaPorId;

public sealed record CertificacionObtenidaDetalle(
    Guid Id,
    Guid UsuarioId,
    Guid CertificacionId,
    DateOnly FechaObtencion,
    string? EvidenciaUrl,
    EstadoMadurez EstadoMadurez);
