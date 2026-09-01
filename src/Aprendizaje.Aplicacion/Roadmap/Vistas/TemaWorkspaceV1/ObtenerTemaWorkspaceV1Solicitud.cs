namespace Aprendizaje.Aplicacion.Roadmap.Vistas.TemaWorkspaceV1;

public sealed record ObtenerTemaWorkspaceV1Solicitud(Guid UsuarioId, Guid TemaId, DateTime? AhoraUtc = null);
