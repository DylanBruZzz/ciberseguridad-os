namespace Aprendizaje.Aplicacion.Evidence.Writeups.VincularWriteupATema;

public sealed record VincularWriteupATemaResultado(VincularWriteupATemaEstado Estado)
{
    public static VincularWriteupATemaResultado Actualizado() => new(VincularWriteupATemaEstado.Actualizado);

    public static VincularWriteupATemaResultado WriteupNoEncontrado() =>
        new(VincularWriteupATemaEstado.WriteupNoEncontrado);

    public static VincularWriteupATemaResultado TemaNoEncontrado() =>
        new(VincularWriteupATemaEstado.TemaNoEncontrado);

    public static VincularWriteupATemaResultado UsuarioNoCoincide() =>
        new(VincularWriteupATemaEstado.UsuarioNoCoincide);
}

public enum VincularWriteupATemaEstado
{
    Actualizado,
    WriteupNoEncontrado,
    TemaNoEncontrado,
    UsuarioNoCoincide
}
