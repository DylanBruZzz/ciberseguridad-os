namespace Aprendizaje.Aplicacion.Roadmap.Temas.GuardarApuntesTema;

public sealed record GuardarApuntesTemaResultado(GuardarApuntesTemaEstado Estado)
{
    public static GuardarApuntesTemaResultado Guardado() =>
        new(GuardarApuntesTemaEstado.Guardado);

    public static GuardarApuntesTemaResultado TemaNoEncontrado() =>
        new(GuardarApuntesTemaEstado.TemaNoEncontrado);

    public static GuardarApuntesTemaResultado UsuarioNoCoincide() =>
        new(GuardarApuntesTemaEstado.UsuarioNoCoincide);
}

public enum GuardarApuntesTemaEstado
{
    Guardado,
    TemaNoEncontrado,
    UsuarioNoCoincide
}
