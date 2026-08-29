using Aprendizaje.Dominio.Evidence;

namespace Aprendizaje.Aplicacion.Evidence.CertificacionesObtenidas.ActualizarCertificacionObtenida;

public sealed record ActualizarCertificacionObtenidaSolicitud(
    Guid CertificacionObtenidaId,
    Guid UsuarioId,
    string? EvidenciaUrl,
    EstadoMadurez EstadoMadurez);
