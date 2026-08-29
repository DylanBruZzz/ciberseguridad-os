namespace Aprendizaje.Aplicacion.Evidence.CertificacionesObtenidas.EliminarCertificacionObtenida;

public enum EliminarCertificacionObtenidaEstado
{
    Eliminada,
    UsuarioNoEncontrado,
    CertificacionObtenidaNoEncontrada,
    UsuarioNoCoincide
}

public sealed record EliminarCertificacionObtenidaResultado(EliminarCertificacionObtenidaEstado Estado)
{
    public static EliminarCertificacionObtenidaResultado Eliminada() =>
        new(EliminarCertificacionObtenidaEstado.Eliminada);

    public static EliminarCertificacionObtenidaResultado UsuarioNoEncontrado() =>
        new(EliminarCertificacionObtenidaEstado.UsuarioNoEncontrado);

    public static EliminarCertificacionObtenidaResultado CertificacionObtenidaNoEncontrada() =>
        new(EliminarCertificacionObtenidaEstado.CertificacionObtenidaNoEncontrada);

    public static EliminarCertificacionObtenidaResultado UsuarioNoCoincide() =>
        new(EliminarCertificacionObtenidaEstado.UsuarioNoCoincide);
}
