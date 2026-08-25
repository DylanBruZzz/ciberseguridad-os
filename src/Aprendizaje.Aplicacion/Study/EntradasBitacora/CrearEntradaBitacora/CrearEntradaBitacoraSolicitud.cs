namespace Aprendizaje.Aplicacion.Study.EntradasBitacora.CrearEntradaBitacora;

public sealed record CrearEntradaBitacoraSolicitud(Guid UsuarioId, string Texto, Guid? TemaId);
