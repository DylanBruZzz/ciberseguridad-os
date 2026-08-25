using Aprendizaje.Dominio.Evidence;

namespace Aprendizaje.Aplicacion.Evidence.Proyectos.ListarProyectos;

public sealed record ProyectoResumen(
    Guid Id,
    Guid UsuarioId,
    string Nombre,
    EstadoProyecto Estado,
    EstadoMadurez EstadoMadurez,
    DateOnly? FechaInicio,
    DateOnly? FechaFin);
