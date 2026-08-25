namespace Aprendizaje.Aplicacion.Evidence.Writeups.ObtenerWriteupPorId;

public sealed record ObtenerWriteupPorIdResultado(bool Encontrado, WriteupDetalle? Writeup)
{
    public static ObtenerWriteupPorIdResultado NoEncontrado() => new(false, null);

    public static ObtenerWriteupPorIdResultado EncontradoCon(WriteupDetalle writeup) => new(true, writeup);
}
