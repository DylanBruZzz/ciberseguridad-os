using Aprendizaje.Dominio.Resource;

namespace Aprendizaje.Aplicacion.Resource.Recursos.CrearRecurso;

public sealed record CrearRecursoSolicitud(
    Guid UsuarioId,
    TipoRecurso Tipo,
    string Titulo,
    string? Url);
