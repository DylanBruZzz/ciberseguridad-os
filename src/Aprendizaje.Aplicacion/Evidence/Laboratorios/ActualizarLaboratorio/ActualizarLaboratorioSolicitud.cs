using Aprendizaje.Dominio.Evidence;

namespace Aprendizaje.Aplicacion.Evidence.Laboratorios.ActualizarLaboratorio;

public sealed record ActualizarLaboratorioSolicitud(
    Guid LaboratorioId,
    Guid UsuarioId,
    string Nombre,
    string? Objetivo,
    string? EntornoVms,
    string? Hallazgos,
    int? TiempoInvertidoMinutos,
    DateOnly? Fecha,
    EstadoMadurez EstadoMadurez);
