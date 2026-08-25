using Aprendizaje.Dominio.Evidence;

namespace Aprendizaje.Aplicacion.Evidence.Laboratorios.ListarLaboratorios;

public sealed record LaboratorioResumen(
    Guid Id,
    Guid UsuarioId,
    string Nombre,
    string? Objetivo,
    int? TiempoInvertidoMinutos,
    EstadoMadurez EstadoMadurez,
    DateOnly? Fecha);
