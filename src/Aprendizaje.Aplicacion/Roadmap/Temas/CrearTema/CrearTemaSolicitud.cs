using Aprendizaje.Dominio.Roadmap;

namespace Aprendizaje.Aplicacion.Roadmap.Temas.CrearTema;

public sealed record CrearTemaSolicitud(
    Guid UsuarioId,
    string Nombre,
    TipoConocimiento TipoConocimiento);
