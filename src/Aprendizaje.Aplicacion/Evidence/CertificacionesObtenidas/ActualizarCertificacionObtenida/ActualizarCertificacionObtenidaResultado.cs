namespace Aprendizaje.Aplicacion.Evidence.CertificacionesObtenidas.ActualizarCertificacionObtenida;

public enum ActualizarCertificacionObtenidaEstado
{
    Actualizada,
    UsuarioNoEncontrado,
    CertificacionObtenidaNoEncontrada,
    UsuarioNoCoincide
}

public sealed record ActualizarCertificacionObtenidaResultado(ActualizarCertificacionObtenidaEstado Estado)
{
    public static ActualizarCertificacionObtenidaResultado Actualizada() =>
        new(ActualizarCertificacionObtenidaEstado.Actualizada);

    public static ActualizarCertificacionObtenidaResultado UsuarioNoEncontrado() =>
        new(ActualizarCertificacionObtenidaEstado.UsuarioNoEncontrado);

    public static ActualizarCertificacionObtenidaResultado CertificacionObtenidaNoEncontrada() =>
        new(ActualizarCertificacionObtenidaEstado.CertificacionObtenidaNoEncontrada);

    public static ActualizarCertificacionObtenidaResultado UsuarioNoCoincide() =>
        new(ActualizarCertificacionObtenidaEstado.UsuarioNoCoincide);
}
