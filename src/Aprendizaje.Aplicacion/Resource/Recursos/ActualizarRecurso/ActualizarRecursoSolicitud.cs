using Aprendizaje.Dominio.Resource;

namespace Aprendizaje.Aplicacion.Resource.Recursos.ActualizarRecurso;

public sealed record ActualizarRecursoSolicitud(
    Guid RecursoId,
    Guid UsuarioId,
    string Titulo,
    string? Url,
    EstadoRecurso Estado,
    int? Rating,
    string? Notas,
    string? HerramientaIA,
    string? PromptsUtilizados);
