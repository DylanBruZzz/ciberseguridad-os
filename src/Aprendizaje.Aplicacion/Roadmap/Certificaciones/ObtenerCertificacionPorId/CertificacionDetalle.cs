using Aprendizaje.Dominio.Roadmap;

namespace Aprendizaje.Aplicacion.Roadmap.Certificaciones.ObtenerCertificacionPorId;

public sealed record CertificacionDetalle(
    Guid Id,
    string Nombre,
    string? Proveedor,
    TipoCosto TipoCosto,
    string? Url);
