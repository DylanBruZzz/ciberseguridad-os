using Aprendizaje.Dominio.Evidence;

namespace Aprendizaje.Aplicacion.Evidence.Notas.CrearNotaParaWriteup;

public sealed record CrearNotaParaWriteupSolicitud(
    Guid UsuarioId,
    Guid WriteupId,
    string Texto,
    TipoNota Tipo = TipoNota.Nota);
