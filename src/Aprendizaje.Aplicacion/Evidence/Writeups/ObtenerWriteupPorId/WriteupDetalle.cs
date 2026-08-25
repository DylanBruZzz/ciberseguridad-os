using Aprendizaje.Dominio.Evidence;

namespace Aprendizaje.Aplicacion.Evidence.Writeups.ObtenerWriteupPorId;

public sealed record WriteupDetalle(
    Guid Id,
    Guid UsuarioId,
    string Titulo,
    string? PlataformaOrigen,
    string? Url,
    EstadoMadurez EstadoMadurez,
    DateOnly? Fecha);
