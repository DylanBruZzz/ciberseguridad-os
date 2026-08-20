using Aprendizaje.Dominio.Study;

namespace Aprendizaje.Aplicacion.Study.SesionesEstudio.RegistrarSesionEstudio;

public enum RegistrarSesionEstudioEstado
{
    Creada,
    TemaNoEncontrado,
    UsuarioNoCoincide
}

public sealed record RegistrarSesionEstudioResultado(
    RegistrarSesionEstudioEstado Estado,
    Guid? Id,
    Guid? UsuarioId,
    Guid? TemaId,
    DateOnly? Fecha,
    int? DuracionMinutos,
    TipoSesion? Tipo,
    string? Notas)
{
    public static RegistrarSesionEstudioResultado Creada(SesionEstudio sesion) =>
        new(
            RegistrarSesionEstudioEstado.Creada,
            sesion.Id,
            sesion.UsuarioId,
            sesion.TemaId,
            sesion.Fecha,
            sesion.DuracionMinutos,
            sesion.Tipo,
            sesion.Notas);

    public static RegistrarSesionEstudioResultado TemaNoEncontrado() =>
        new(RegistrarSesionEstudioEstado.TemaNoEncontrado, null, null, null, null, null, null, null);

    public static RegistrarSesionEstudioResultado UsuarioNoCoincide() =>
        new(RegistrarSesionEstudioEstado.UsuarioNoCoincide, null, null, null, null, null, null, null);
}
