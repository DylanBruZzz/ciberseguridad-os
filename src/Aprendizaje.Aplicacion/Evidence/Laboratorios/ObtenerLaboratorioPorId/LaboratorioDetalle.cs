using Aprendizaje.Dominio.Evidence;

namespace Aprendizaje.Aplicacion.Evidence.Laboratorios.ObtenerLaboratorioPorId;

public sealed record LaboratorioDetalle(
    Guid Id,
    Guid UsuarioId,
    string Nombre,
    string? Objetivo,
    string? EntornoVms,
    string? Hallazgos,
    int? TiempoInvertidoMinutos,
    EstadoMadurez EstadoMadurez,
    DateOnly? Fecha);
