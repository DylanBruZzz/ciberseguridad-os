namespace Aprendizaje.Aplicacion.Roadmap.Competencias.CrearCompetencia;

public sealed record CrearCompetenciaResultado(
    Guid Id,
    Guid UsuarioId,
    string Nombre,
    string? Descripcion);
