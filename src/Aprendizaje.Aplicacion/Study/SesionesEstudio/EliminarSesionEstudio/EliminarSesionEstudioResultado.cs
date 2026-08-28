namespace Aprendizaje.Aplicacion.Study.SesionesEstudio.EliminarSesionEstudio;

public enum EliminarSesionEstudioEstado
{
    Eliminada,
    UsuarioNoEncontrado,
    SesionNoEncontrada,
    UsuarioNoCoincide
}

public sealed record EliminarSesionEstudioResultado(EliminarSesionEstudioEstado Estado)
{
    public static EliminarSesionEstudioResultado Eliminada() =>
        new(EliminarSesionEstudioEstado.Eliminada);

    public static EliminarSesionEstudioResultado UsuarioNoEncontrado() =>
        new(EliminarSesionEstudioEstado.UsuarioNoEncontrado);

    public static EliminarSesionEstudioResultado SesionNoEncontrada() =>
        new(EliminarSesionEstudioEstado.SesionNoEncontrada);

    public static EliminarSesionEstudioResultado UsuarioNoCoincide() =>
        new(EliminarSesionEstudioEstado.UsuarioNoCoincide);
}
