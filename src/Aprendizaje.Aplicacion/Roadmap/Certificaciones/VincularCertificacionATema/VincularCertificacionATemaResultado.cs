namespace Aprendizaje.Aplicacion.Roadmap.Certificaciones.VincularCertificacionATema;

public enum VincularCertificacionATemaEstado
{
    Actualizado,
    CertificacionNoEncontrada,
    TemaNoEncontrado
}

public sealed record VincularCertificacionATemaResultado(VincularCertificacionATemaEstado Estado)
{
    public static VincularCertificacionATemaResultado Actualizado() =>
        new(VincularCertificacionATemaEstado.Actualizado);

    public static VincularCertificacionATemaResultado CertificacionNoEncontrada() =>
        new(VincularCertificacionATemaEstado.CertificacionNoEncontrada);

    public static VincularCertificacionATemaResultado TemaNoEncontrado() =>
        new(VincularCertificacionATemaEstado.TemaNoEncontrado);
}
