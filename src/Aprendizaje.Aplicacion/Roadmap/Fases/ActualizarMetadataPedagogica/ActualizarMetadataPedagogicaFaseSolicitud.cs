namespace Aprendizaje.Aplicacion.Roadmap.Fases.ActualizarMetadataPedagogica;

public sealed record ActualizarMetadataPedagogicaFaseSolicitud(
    Guid FaseId,
    Guid UsuarioId,
    IReadOnlyCollection<string> Objetivos,
    IReadOnlyCollection<string> CriteriosAvance,
    int? MesInicioRecomendado,
    int? MesFinRecomendado,
    string? CargaSemanalRecomendada);
