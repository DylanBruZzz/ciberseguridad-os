namespace Aprendizaje.Aplicacion.Evidence.Laboratorios.CrearLaboratorio;

public sealed record CrearLaboratorioSolicitud(
    Guid UsuarioId,
    string Nombre,
    string? Objetivo,
    string? EntornoVms,
    string? Hallazgos,
    int? TiempoInvertidoMinutos,
    DateOnly? Fecha);
