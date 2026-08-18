namespace Aprendizaje.Aplicacion.Roadmap.Temas.EstablecerObjetivos;

public sealed record EstablecerObjetivosTemaResultado(bool Encontrado)
{
    public static EstablecerObjetivosTemaResultado Actualizado() => new(true);

    public static EstablecerObjetivosTemaResultado NoEncontrado() => new(false);
}
