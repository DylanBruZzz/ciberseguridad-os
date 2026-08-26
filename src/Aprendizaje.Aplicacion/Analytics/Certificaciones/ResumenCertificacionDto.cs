using Aprendizaje.Dominio.Roadmap;

namespace Aprendizaje.Aplicacion.Analytics.Certificaciones;

public sealed record ResumenCertificacionDto(
    Guid CertificacionId,
    string Nombre,
    string? Proveedor,
    TipoCosto TipoCosto,
    int TemasVinculados,
    int CertificacionesObtenidas);
