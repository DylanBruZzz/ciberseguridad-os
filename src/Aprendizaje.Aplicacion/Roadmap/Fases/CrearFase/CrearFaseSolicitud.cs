namespace Aprendizaje.Aplicacion.Roadmap.Fases.CrearFase;

public sealed record CrearFaseSolicitud(
    Guid UsuarioId,
    string Nombre,
    int Orden,
    IReadOnlyCollection<string>? Objetivos = null,
    IReadOnlyCollection<string>? CriteriosAvance = null,
    int? MesInicioRecomendado = null,
    int? MesFinRecomendado = null,
    string? CargaSemanalRecomendada = null);
