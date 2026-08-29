using Aprendizaje.Dominio.Evidence;

namespace Aprendizaje.Aplicacion.Evidence.Proyectos.ActualizarProyecto;

public sealed record ActualizarProyectoSolicitud(
    Guid ProyectoId,
    Guid UsuarioId,
    string Nombre,
    string? Descripcion,
    EstadoProyecto Estado,
    EstadoMadurez EstadoMadurez,
    string? RepositorioUrl,
    DateOnly? FechaInicio,
    DateOnly? FechaFin);
