namespace Aprendizaje.Aplicacion.Study.SesionesEstudio.ObtenerSesionEstudioPorId;

public sealed record ObtenerSesionEstudioPorIdResultado(bool Encontrado, SesionEstudioDetalle? Sesion)
{
    public static ObtenerSesionEstudioPorIdResultado Encontrada(SesionEstudioDetalle sesion) =>
        new(true, sesion);

    public static ObtenerSesionEstudioPorIdResultado NoEncontrada() =>
        new(false, null);
}
