namespace Aprendizaje.Aplicacion.Roadmap.Certificaciones.ObtenerCertificacionPorId;

public sealed record ObtenerCertificacionPorIdResultado(bool Encontrado, CertificacionDetalle? Certificacion)
{
    public static ObtenerCertificacionPorIdResultado NoEncontrada() => new(false, null);

    public static ObtenerCertificacionPorIdResultado EncontradaCon(CertificacionDetalle certificacion) =>
        new(true, certificacion);
}
