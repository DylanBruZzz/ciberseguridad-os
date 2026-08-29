namespace Aprendizaje.Aplicacion.Evidence.Writeups.ActualizarWriteup;

public enum ActualizarWriteupEstado
{
    Actualizado,
    UsuarioNoEncontrado,
    WriteupNoEncontrado,
    UsuarioNoCoincide
}

public sealed record ActualizarWriteupResultado(ActualizarWriteupEstado Estado)
{
    public static ActualizarWriteupResultado Actualizado() => new(ActualizarWriteupEstado.Actualizado);

    public static ActualizarWriteupResultado UsuarioNoEncontrado() =>
        new(ActualizarWriteupEstado.UsuarioNoEncontrado);

    public static ActualizarWriteupResultado WriteupNoEncontrado() =>
        new(ActualizarWriteupEstado.WriteupNoEncontrado);

    public static ActualizarWriteupResultado UsuarioNoCoincide() =>
        new(ActualizarWriteupEstado.UsuarioNoCoincide);
}
