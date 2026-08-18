namespace Aprendizaje.Aplicacion.Roadmap.Temas.ObtenerTemaPorId;

public sealed record ObtenerTemaPorIdResultado(bool Encontrado, TemaDetalle? Tema)
{
    public static ObtenerTemaPorIdResultado NoEncontrado() => new(false, null);

    public static ObtenerTemaPorIdResultado EncontradoCon(TemaDetalle tema) => new(true, tema);
}
