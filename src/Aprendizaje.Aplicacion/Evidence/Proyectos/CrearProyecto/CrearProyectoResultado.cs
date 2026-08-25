using Aprendizaje.Dominio.Evidence;

namespace Aprendizaje.Aplicacion.Evidence.Proyectos.CrearProyecto;

public sealed record CrearProyectoResultado(
    Guid Id,
    Guid UsuarioId,
    string Nombre,
    string? Descripcion,
    EstadoProyecto Estado,
    EstadoMadurez EstadoMadurez,
    string? RepositorioUrl,
    DateOnly? FechaInicio,
    DateOnly? FechaFin);
