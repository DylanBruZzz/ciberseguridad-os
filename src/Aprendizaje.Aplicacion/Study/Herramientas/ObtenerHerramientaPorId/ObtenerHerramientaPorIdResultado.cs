namespace Aprendizaje.Aplicacion.Study.Herramientas.ObtenerHerramientaPorId;

public sealed record ObtenerHerramientaPorIdResultado(bool Encontrada, HerramientaDetalle? Herramienta)
{
    public static ObtenerHerramientaPorIdResultado EncontradaCon(HerramientaDetalle herramienta) =>
        new(true, herramienta);

    public static ObtenerHerramientaPorIdResultado NoEncontrada() => new(false, null);
}
