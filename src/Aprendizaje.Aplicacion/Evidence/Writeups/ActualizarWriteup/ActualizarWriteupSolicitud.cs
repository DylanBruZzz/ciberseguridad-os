using Aprendizaje.Dominio.Evidence;

namespace Aprendizaje.Aplicacion.Evidence.Writeups.ActualizarWriteup;

public sealed record ActualizarWriteupSolicitud(
    Guid WriteupId,
    Guid UsuarioId,
    string Titulo,
    string? PlataformaOrigen,
    string? Url,
    DateOnly? Fecha,
    EstadoMadurez EstadoMadurez);
