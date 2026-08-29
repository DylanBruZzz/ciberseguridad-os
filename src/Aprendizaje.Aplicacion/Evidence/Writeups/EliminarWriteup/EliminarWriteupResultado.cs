namespace Aprendizaje.Aplicacion.Evidence.Writeups.EliminarWriteup;

public enum EliminarWriteupEstado
{
    Eliminado,
    UsuarioNoEncontrado,
    WriteupNoEncontrado,
    UsuarioNoCoincide
}

public sealed record EliminarWriteupResultado(EliminarWriteupEstado Estado)
{
    public static EliminarWriteupResultado Eliminado() => new(EliminarWriteupEstado.Eliminado);

    public static EliminarWriteupResultado UsuarioNoEncontrado() =>
        new(EliminarWriteupEstado.UsuarioNoEncontrado);

    public static EliminarWriteupResultado WriteupNoEncontrado() =>
        new(EliminarWriteupEstado.WriteupNoEncontrado);

    public static EliminarWriteupResultado UsuarioNoCoincide() =>
        new(EliminarWriteupEstado.UsuarioNoCoincide);
}
