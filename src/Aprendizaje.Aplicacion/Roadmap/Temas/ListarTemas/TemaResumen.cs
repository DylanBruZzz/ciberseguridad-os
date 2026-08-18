using Aprendizaje.Dominio.Roadmap;

namespace Aprendizaje.Aplicacion.Roadmap.Temas.ListarTemas;

public sealed record TemaResumen(
    Guid Id,
    Guid UsuarioId,
    Guid? FaseId,
    Guid? TemaPadreId,
    string Nombre,
    TipoConocimiento TipoConocimiento,
    string? Descripcion);
