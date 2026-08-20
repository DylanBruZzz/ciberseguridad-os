using Aprendizaje.Dominio.Study;

namespace Aprendizaje.Aplicacion.Study.SesionesEstudio.ListarSesionesEstudio;

public sealed record SesionEstudioResumen(
    Guid Id,
    Guid UsuarioId,
    Guid TemaId,
    DateOnly Fecha,
    int DuracionMinutos,
    TipoSesion Tipo,
    string? Notas)
{
    public static SesionEstudioResumen Desde(SesionEstudio sesion) =>
        new(
            sesion.Id,
            sesion.UsuarioId,
            sesion.TemaId,
            sesion.Fecha,
            sesion.DuracionMinutos,
            sesion.Tipo,
            sesion.Notas);
}
