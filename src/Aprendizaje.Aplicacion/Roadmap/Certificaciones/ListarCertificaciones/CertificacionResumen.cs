using Aprendizaje.Dominio.Roadmap;

namespace Aprendizaje.Aplicacion.Roadmap.Certificaciones.ListarCertificaciones;

public sealed record CertificacionResumen(
    Guid Id,
    string Nombre,
    string? Proveedor,
    TipoCosto TipoCosto,
    string? Url);
