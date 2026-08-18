using Aprendizaje.Dominio.Roadmap;

namespace Aprendizaje.Aplicacion.Roadmap.Temas.ObtenerTemaPorId;

public sealed record TemaDetalle(
    Guid Id,
    Guid UsuarioId,
    Guid? FaseId,
    Guid? TemaPadreId,
    string Nombre,
    string? Descripcion,
    IReadOnlyCollection<string> Objetivos,
    TipoConocimiento TipoConocimiento,
    DateOnly? FechaInicio,
    DateOnly? FechaFin,
    int? DificultadPercibida,
    int? Confianza,
    int? IntervaloRepasoDias,
    IReadOnlyCollection<CriterioTemaDetalle> Criterios);
