using Aprendizaje.Dominio.Evidence;

namespace Aprendizaje.Aplicacion.Evidence.Writeups.CrearWriteup;

public sealed record CrearWriteupResultado(
    Guid Id,
    Guid UsuarioId,
    string Titulo,
    string? PlataformaOrigen,
    string? Url,
    EstadoMadurez EstadoMadurez,
    DateOnly? Fecha);
