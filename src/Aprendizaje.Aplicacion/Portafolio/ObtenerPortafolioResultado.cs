namespace Aprendizaje.Aplicacion.Portafolio;

public sealed record ObtenerPortafolioResultado(
    bool Encontrado,
    PortafolioDto? Portafolio)
{
    public static ObtenerPortafolioResultado NoEncontrado() => new(false, null);

    public static ObtenerPortafolioResultado EncontradoCon(PortafolioDto portafolio) => new(true, portafolio);
}
