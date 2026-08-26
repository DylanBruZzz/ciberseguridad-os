namespace Aprendizaje.Aplicacion.Analytics.Competencias;

public sealed record ResumenCompetenciaDto(
    Guid CompetenciaId,
    string Nombre,
    int TemasVinculados);
