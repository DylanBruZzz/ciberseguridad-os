using Aprendizaje.Dominio.Resource;

namespace Aprendizaje.Aplicacion.Resource.Recursos.CrearRecurso;

public sealed record CrearRecursoResultado(
    Guid Id,
    Guid UsuarioId,
    TipoRecurso Tipo,
    string Titulo,
    string? Url,
    EstadoRecurso Estado);
