namespace Aprendizaje.Aplicacion.Roadmap.Competencias.ListarCompetencias;

public sealed record CompetenciaResumen(
    Guid Id,
    Guid UsuarioId,
    string Nombre,
    string? Descripcion);
