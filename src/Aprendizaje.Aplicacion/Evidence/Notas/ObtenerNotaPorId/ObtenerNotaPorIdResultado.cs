namespace Aprendizaje.Aplicacion.Evidence.Notas.ObtenerNotaPorId;

public sealed record ObtenerNotaPorIdResultado(bool Encontrada, NotaDetalle? Nota)
{
    public static ObtenerNotaPorIdResultado NoEncontrada() => new(false, null);

    public static ObtenerNotaPorIdResultado EncontradaCon(NotaDetalle nota) => new(true, nota);
}
