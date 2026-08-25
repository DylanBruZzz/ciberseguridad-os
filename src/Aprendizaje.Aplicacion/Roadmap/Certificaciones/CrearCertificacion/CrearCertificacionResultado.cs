using Aprendizaje.Dominio.Roadmap;

namespace Aprendizaje.Aplicacion.Roadmap.Certificaciones.CrearCertificacion;

public sealed record CrearCertificacionResultado(
    Guid Id,
    string Nombre,
    string? Proveedor,
    TipoCosto TipoCosto,
    string? Url);
