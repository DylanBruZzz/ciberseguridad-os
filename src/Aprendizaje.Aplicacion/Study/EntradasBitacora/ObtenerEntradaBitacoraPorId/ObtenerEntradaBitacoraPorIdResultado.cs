namespace Aprendizaje.Aplicacion.Study.EntradasBitacora.ObtenerEntradaBitacoraPorId;

public sealed record ObtenerEntradaBitacoraPorIdResultado(bool Encontrada, EntradaBitacoraDetalle? Entrada)
{
    public static ObtenerEntradaBitacoraPorIdResultado EncontradaCon(EntradaBitacoraDetalle entrada) =>
        new(true, entrada);

    public static ObtenerEntradaBitacoraPorIdResultado NoEncontrada() => new(false, null);
}
