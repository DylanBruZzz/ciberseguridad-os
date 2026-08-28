namespace Aprendizaje.Aplicacion.Study.SesionesEstudio.ActualizarSesionEstudio;

public enum ActualizarSesionEstudioEstado
{
    Actualizada,
    UsuarioNoEncontrado,
    SesionNoEncontrada,
    TemaNoEncontrado,
    UsuarioNoCoincide
}

public sealed record ActualizarSesionEstudioResultado(ActualizarSesionEstudioEstado Estado)
{
    public static ActualizarSesionEstudioResultado Actualizada() =>
        new(ActualizarSesionEstudioEstado.Actualizada);

    public static ActualizarSesionEstudioResultado UsuarioNoEncontrado() =>
        new(ActualizarSesionEstudioEstado.UsuarioNoEncontrado);

    public static ActualizarSesionEstudioResultado SesionNoEncontrada() =>
        new(ActualizarSesionEstudioEstado.SesionNoEncontrada);

    public static ActualizarSesionEstudioResultado TemaNoEncontrado() =>
        new(ActualizarSesionEstudioEstado.TemaNoEncontrado);

    public static ActualizarSesionEstudioResultado UsuarioNoCoincide() =>
        new(ActualizarSesionEstudioEstado.UsuarioNoCoincide);
}
