using Aprendizaje.Dominio.Study;

namespace Aprendizaje.Aplicacion.Study.SesionesEstudio.RegistrarSesionEstudio;

public sealed record RegistrarSesionEstudioSolicitud(
    Guid UsuarioId,
    Guid TemaId,
    DateOnly Fecha,
    int DuracionMinutos,
    TipoSesion Tipo,
    string? Notas);
