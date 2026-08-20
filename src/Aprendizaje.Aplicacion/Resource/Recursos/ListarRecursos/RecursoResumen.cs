using Aprendizaje.Dominio.Resource;

namespace Aprendizaje.Aplicacion.Resource.Recursos.ListarRecursos;

public sealed record RecursoResumen(
    Guid Id,
    Guid UsuarioId,
    TipoRecurso Tipo,
    string Titulo,
    string? Url,
    EstadoRecurso Estado);
