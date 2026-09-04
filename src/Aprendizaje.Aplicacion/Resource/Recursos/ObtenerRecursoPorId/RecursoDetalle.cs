using Aprendizaje.Dominio.Resource;
using Aprendizaje.Aplicacion.Resource.Recursos;

namespace Aprendizaje.Aplicacion.Resource.Recursos.ObtenerRecursoPorId;

public sealed record RecursoDetalle(
    Guid Id,
    Guid UsuarioId,
    TipoRecurso Tipo,
    string Titulo,
    string? Url,
    EstadoRecurso Estado,
    int? Rating,
    string? Notas,
    string? HerramientaIA,
    string? PromptsUtilizados,
    IReadOnlyCollection<RecursoTemaResumen> Temas);
