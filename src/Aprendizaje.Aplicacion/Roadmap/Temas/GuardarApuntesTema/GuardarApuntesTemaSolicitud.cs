namespace Aprendizaje.Aplicacion.Roadmap.Temas.GuardarApuntesTema;

public sealed record GuardarApuntesTemaSolicitud(Guid UsuarioId, Guid TemaId, string Contenido);
