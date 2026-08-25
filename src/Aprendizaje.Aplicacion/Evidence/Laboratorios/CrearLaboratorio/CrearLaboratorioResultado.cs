using Aprendizaje.Dominio.Evidence;

namespace Aprendizaje.Aplicacion.Evidence.Laboratorios.CrearLaboratorio;

public sealed record CrearLaboratorioResultado(
    Guid Id,
    Guid UsuarioId,
    string Nombre,
    string? Objetivo,
    string? EntornoVms,
    string? Hallazgos,
    int? TiempoInvertidoMinutos,
    EstadoMadurez EstadoMadurez,
    DateOnly? Fecha);
