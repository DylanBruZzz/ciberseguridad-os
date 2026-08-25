using Aprendizaje.Dominio.Evidence;

namespace Aprendizaje.Aplicacion.Evidence.Proyectos.ObtenerProyectoPorId;

public sealed record ProyectoDetalle(
    Guid Id,
    Guid UsuarioId,
    string Nombre,
    string? Descripcion,
    EstadoProyecto Estado,
    EstadoMadurez EstadoMadurez,
    string? RepositorioUrl,
    DateOnly? FechaInicio,
    DateOnly? FechaFin);
