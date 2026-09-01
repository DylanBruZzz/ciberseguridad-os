using Aprendizaje.Dominio.Roadmap;

namespace Aprendizaje.Aplicacion.Roadmap.Vistas.RoadmapVistaV1;

public sealed record RoadmapVistaV1Dto(
    Guid? FaseActualId,
    int ProgresoGlobalPorcentaje,
    int TotalTemas,
    int TemasDominados,
    IReadOnlyCollection<FaseRoadmapVistaV1Dto> Fases);

public sealed record FaseRoadmapVistaV1Dto(
    Guid Id,
    int Orden,
    string Nombre,
    string? Color,
    string? Descripcion,
    IReadOnlyCollection<string> Objetivos,
    IReadOnlyCollection<string> CriteriosAvance,
    int? MesInicioRecomendado,
    int? MesFinRecomendado,
    string? CargaSemanalRecomendada,
    int TotalTemas,
    int TemasDominados,
    int ProgresoPorcentaje,
    bool EstaCompletada,
    bool EsFaseActual,
    IReadOnlyCollection<TemaRoadmapVistaV1Dto> Temas);

public sealed record TemaRoadmapVistaV1Dto(
    Guid Id,
    Guid? FaseId,
    Guid? TemaPadreId,
    string Nombre,
    string? Descripcion,
    TipoConocimiento TipoConocimiento,
    int? DificultadPercibida,
    int? Confianza,
    int IntervaloRepasoDias,
    EstadoTema Estado,
    int CriteriosTotal,
    int CriteriosCumplidos,
    int ProgresoPorcentaje,
    DateOnly? UltimaSesion,
    DateOnly? ProximaFechaRepaso,
    bool RepasoRecomendado);
