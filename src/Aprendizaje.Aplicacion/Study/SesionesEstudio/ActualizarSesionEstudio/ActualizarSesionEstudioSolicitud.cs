using Aprendizaje.Dominio.Study;

namespace Aprendizaje.Aplicacion.Study.SesionesEstudio.ActualizarSesionEstudio;

public sealed record ActualizarSesionEstudioSolicitud(
    Guid SesionId,
    Guid UsuarioId,
    Guid TemaId,
    DateOnly Fecha,
    int DuracionMinutos,
    TipoSesion Tipo,
    string? Notas);
