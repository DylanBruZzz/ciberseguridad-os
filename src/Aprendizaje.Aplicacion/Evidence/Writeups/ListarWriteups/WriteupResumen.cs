using Aprendizaje.Dominio.Evidence;

namespace Aprendizaje.Aplicacion.Evidence.Writeups.ListarWriteups;

public sealed record WriteupResumen(
    Guid Id,
    Guid UsuarioId,
    string Titulo,
    string? PlataformaOrigen,
    EstadoMadurez EstadoMadurez,
    DateOnly? Fecha);
