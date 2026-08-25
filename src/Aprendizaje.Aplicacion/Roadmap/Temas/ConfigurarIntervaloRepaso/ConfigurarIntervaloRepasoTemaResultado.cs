namespace Aprendizaje.Aplicacion.Roadmap.Temas.ConfigurarIntervaloRepaso;

public sealed record ConfigurarIntervaloRepasoTemaResultado(bool Encontrado)
{
    public static ConfigurarIntervaloRepasoTemaResultado Actualizado() => new(true);

    public static ConfigurarIntervaloRepasoTemaResultado NoEncontrado() => new(false);
}
