namespace Aprendizaje.Aplicacion.Study.SesionesEstudio.VincularHerramientaASesionEstudio;

public enum VincularHerramientaASesionEstudioEstado
{
    Actualizado,
    SesionNoEncontrada,
    HerramientaNoEncontrada
}

public sealed record VincularHerramientaASesionEstudioResultado(VincularHerramientaASesionEstudioEstado Estado)
{
    public static VincularHerramientaASesionEstudioResultado Actualizado() =>
        new(VincularHerramientaASesionEstudioEstado.Actualizado);

    public static VincularHerramientaASesionEstudioResultado SesionNoEncontrada() =>
        new(VincularHerramientaASesionEstudioEstado.SesionNoEncontrada);

    public static VincularHerramientaASesionEstudioResultado HerramientaNoEncontrada() =>
        new(VincularHerramientaASesionEstudioEstado.HerramientaNoEncontrada);
}
