namespace Aprendizaje.Aplicacion.Roadmap.Fases.CrearFase;

public sealed record CrearFaseResultado(
    Guid Id,
    Guid UsuarioId,
    string Nombre,
    int Orden,
    IReadOnlyCollection<string> Objetivos,
    IReadOnlyCollection<string> CriteriosAvance,
    int? MesInicioRecomendado,
    int? MesFinRecomendado,
    string? CargaSemanalRecomendada);
