namespace Aprendizaje.Aplicacion.Roadmap.Temas.DesvincularHerramientaDeTema;

public enum DesvincularHerramientaDeTemaEstado
{
    Actualizado,
    TemaNoEncontrado,
    HerramientaNoEncontrada,
    UsuarioNoCoincide
}

public sealed record DesvincularHerramientaDeTemaResultado(DesvincularHerramientaDeTemaEstado Estado)
{
    public static DesvincularHerramientaDeTemaResultado Actualizado() =>
        new(DesvincularHerramientaDeTemaEstado.Actualizado);

    public static DesvincularHerramientaDeTemaResultado TemaNoEncontrado() =>
        new(DesvincularHerramientaDeTemaEstado.TemaNoEncontrado);

    public static DesvincularHerramientaDeTemaResultado HerramientaNoEncontrada() =>
        new(DesvincularHerramientaDeTemaEstado.HerramientaNoEncontrada);

    public static DesvincularHerramientaDeTemaResultado UsuarioNoCoincide() =>
        new(DesvincularHerramientaDeTemaEstado.UsuarioNoCoincide);
}
