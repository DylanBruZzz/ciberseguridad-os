namespace Aprendizaje.Aplicacion.Evidence.CertificacionesObtenidas.ObtenerCertificacionObtenidaPorId;

public sealed record ObtenerCertificacionObtenidaPorIdResultado(
    bool Encontrada,
    CertificacionObtenidaDetalle? CertificacionObtenida)
{
    public static ObtenerCertificacionObtenidaPorIdResultado NoEncontrada() => new(false, null);

    public static ObtenerCertificacionObtenidaPorIdResultado EncontradaCon(
        CertificacionObtenidaDetalle certificacionObtenida) =>
        new(true, certificacionObtenida);
}
