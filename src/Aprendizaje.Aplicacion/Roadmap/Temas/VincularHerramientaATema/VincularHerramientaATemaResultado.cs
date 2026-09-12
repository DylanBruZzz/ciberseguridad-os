namespace Aprendizaje.Aplicacion.Roadmap.Temas.VincularHerramientaATema;

public enum VincularHerramientaATemaEstado
{
    Actualizado,
    TemaNoEncontrado,
    HerramientaNoEncontrada,
    UsuarioNoCoincide
}

public sealed record VincularHerramientaATemaResultado(VincularHerramientaATemaEstado Estado)
{
    public static VincularHerramientaATemaResultado Actualizado() =>
        new(VincularHerramientaATemaEstado.Actualizado);

    public static VincularHerramientaATemaResultado TemaNoEncontrado() =>
        new(VincularHerramientaATemaEstado.TemaNoEncontrado);

    public static VincularHerramientaATemaResultado HerramientaNoEncontrada() =>
        new(VincularHerramientaATemaEstado.HerramientaNoEncontrada);

    public static VincularHerramientaATemaResultado UsuarioNoCoincide() =>
        new(VincularHerramientaATemaEstado.UsuarioNoCoincide);
}
