namespace Aprendizaje.Aplicacion.Roadmap.Temas.ActualizarPercepcion;

public sealed record ActualizarPercepcionTemaResultado(bool Encontrado)
{
    public static ActualizarPercepcionTemaResultado Actualizado() => new(true);

    public static ActualizarPercepcionTemaResultado NoEncontrado() => new(false);
}
