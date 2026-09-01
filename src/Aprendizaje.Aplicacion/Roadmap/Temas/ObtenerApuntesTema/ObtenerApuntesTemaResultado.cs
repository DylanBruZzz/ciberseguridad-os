namespace Aprendizaje.Aplicacion.Roadmap.Temas.ObtenerApuntesTema;

public sealed record ObtenerApuntesTemaResultado(
    ObtenerApuntesTemaEstado Estado,
    ApuntesTemaDto? Apuntes)
{
    public static ObtenerApuntesTemaResultado Encontrado(ApuntesTemaDto apuntes) =>
        new(ObtenerApuntesTemaEstado.Encontrado, apuntes);

    public static ObtenerApuntesTemaResultado TemaNoEncontrado() =>
        new(ObtenerApuntesTemaEstado.TemaNoEncontrado, null);

    public static ObtenerApuntesTemaResultado UsuarioNoCoincide() =>
        new(ObtenerApuntesTemaEstado.UsuarioNoCoincide, null);
}

public enum ObtenerApuntesTemaEstado
{
    Encontrado,
    TemaNoEncontrado,
    UsuarioNoCoincide
}
