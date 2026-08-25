namespace Aprendizaje.Aplicacion.Roadmap.Competencias.ObtenerCompetenciaPorId;

public sealed record CompetenciaDetalle(
    Guid Id,
    Guid UsuarioId,
    string Nombre,
    string? Descripcion);
