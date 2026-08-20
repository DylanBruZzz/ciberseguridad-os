namespace Aprendizaje.Aplicacion.Study.SesionesEstudio.CorregirDuracionSesionEstudio;

public enum CorregirDuracionSesionEstudioEstado
{
    Actualizada,
    SesionNoEncontrada
}

public sealed record CorregirDuracionSesionEstudioResultado(CorregirDuracionSesionEstudioEstado Estado)
{
    public static CorregirDuracionSesionEstudioResultado Actualizada() =>
        new(CorregirDuracionSesionEstudioEstado.Actualizada);

    public static CorregirDuracionSesionEstudioResultado SesionNoEncontrada() =>
        new(CorregirDuracionSesionEstudioEstado.SesionNoEncontrada);
}
