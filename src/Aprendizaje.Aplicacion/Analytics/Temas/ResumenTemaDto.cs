namespace Aprendizaje.Aplicacion.Analytics.Temas;

public sealed record ResumenTemaDto(
    Guid TemaId,
    string Nombre,
    int? DificultadPercibida,
    int? Confianza,
    DateOnly? FechaInicio,
    DateOnly? FechaFin,
    int? IntervaloRepasoDias,
    int SesionesTotales,
    int MinutosTotales,
    DateOnly? UltimaSesion,
    int CriteriosTotales,
    int CriteriosCumplidos,
    int RecursosVinculados,
    int Laboratorios,
    int Proyectos,
    int ArtefactosTecnicos,
    int Writeups,
    int Notas);
