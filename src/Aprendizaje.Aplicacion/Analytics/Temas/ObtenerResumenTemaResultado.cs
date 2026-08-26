namespace Aprendizaje.Aplicacion.Analytics.Temas;

public sealed record ObtenerResumenTemaResultado(bool Encontrado, ResumenTemaDto? Resumen)
{
    public static ObtenerResumenTemaResultado EncontradoCon(ResumenTemaDto resumen) => new(true, resumen);

    public static ObtenerResumenTemaResultado NoEncontrado() => new(false, null);
}
