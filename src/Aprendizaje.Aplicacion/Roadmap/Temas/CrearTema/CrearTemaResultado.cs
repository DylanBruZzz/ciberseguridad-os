using Aprendizaje.Dominio.Roadmap;

namespace Aprendizaje.Aplicacion.Roadmap.Temas.CrearTema;

public sealed record CrearTemaResultado(
    Guid Id,
    Guid UsuarioId,
    string Nombre,
    TipoConocimiento TipoConocimiento);
