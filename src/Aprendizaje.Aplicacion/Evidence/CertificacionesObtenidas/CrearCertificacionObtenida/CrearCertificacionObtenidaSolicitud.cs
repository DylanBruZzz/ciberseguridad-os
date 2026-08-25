namespace Aprendizaje.Aplicacion.Evidence.CertificacionesObtenidas.CrearCertificacionObtenida;

public sealed record CrearCertificacionObtenidaSolicitud(
    Guid UsuarioId,
    Guid CertificacionId,
    DateOnly FechaObtencion);
