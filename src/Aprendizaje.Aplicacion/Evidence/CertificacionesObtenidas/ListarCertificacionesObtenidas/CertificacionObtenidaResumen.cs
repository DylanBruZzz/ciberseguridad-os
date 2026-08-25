using Aprendizaje.Dominio.Evidence;

namespace Aprendizaje.Aplicacion.Evidence.CertificacionesObtenidas.ListarCertificacionesObtenidas;

public sealed record CertificacionObtenidaResumen(
    Guid Id,
    Guid UsuarioId,
    Guid CertificacionId,
    DateOnly FechaObtencion,
    string? EvidenciaUrl,
    EstadoMadurez EstadoMadurez);
