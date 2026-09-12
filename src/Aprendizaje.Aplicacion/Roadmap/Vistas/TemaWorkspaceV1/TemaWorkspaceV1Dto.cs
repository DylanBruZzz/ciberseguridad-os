using Aprendizaje.Dominio.Roadmap;
using Aprendizaje.Dominio.Study;

namespace Aprendizaje.Aplicacion.Roadmap.Vistas.TemaWorkspaceV1;

public sealed record TemaWorkspaceV1Dto(
    TemaWorkspaceTemaDto Tema,
    TemaWorkspaceFaseDto? Fase,
    TemaWorkspaceApuntesDto Apuntes,
    IReadOnlyCollection<TemaWorkspaceHerramientaDto> Herramientas,
    TemaWorkspaceSesionDto? UltimaSesion,
    TemaWorkspaceRepasoDto Repaso,
    TemaWorkspaceResourcesResumenDto ResourcesResumen,
    TemaWorkspaceSesionesResumenDto SesionesResumen,
    TemaWorkspaceEvidenceResumenDto EvidenceResumen);

public sealed record TemaWorkspaceTemaDto(
    Guid Id,
    Guid? TemaPadreId,
    string Nombre,
    string? Descripcion,
    TipoConocimiento TipoConocimiento,
    EstadoTema Estado,
    int? DificultadPercibida,
    int? Confianza,
    int IntervaloRepasoDias,
    int CriteriosTotal,
    int CriteriosCumplidos,
    int ProgresoPorcentaje,
    IReadOnlyCollection<string> Objetivos,
    IReadOnlyCollection<TemaWorkspaceCriterioDto> Criterios);

public sealed record TemaWorkspaceCriterioDto(
    Guid Id,
    TipoCriterio Tipo,
    bool Cumplido,
    DateTime? FechaCumplidoUtc);

public sealed record TemaWorkspaceFaseDto(Guid Id, int Orden, string Nombre);

public sealed record TemaWorkspaceApuntesDto(string Contenido, DateTime? FechaModificacionUtc);

public sealed record TemaWorkspaceHerramientaDto(Guid Id, string Nombre);

public sealed record TemaWorkspaceSesionDto(Guid Id, DateOnly Fecha, int DuracionMinutos, TipoSesion Tipo);

public sealed record TemaWorkspaceRepasoDto(DateOnly? ProximaFechaRepaso, bool RepasoRecomendado);

public sealed record TemaWorkspaceResourcesResumenDto(int Total);

public sealed record TemaWorkspaceSesionesResumenDto(int Total, int TotalMinutos);

public sealed record TemaWorkspaceEvidenceResumenDto(
    int Total,
    int Proyectos,
    int Laboratorios,
    int Writeups,
    int ArtefactosTecnicos,
    int CertificacionesObtenidas);
