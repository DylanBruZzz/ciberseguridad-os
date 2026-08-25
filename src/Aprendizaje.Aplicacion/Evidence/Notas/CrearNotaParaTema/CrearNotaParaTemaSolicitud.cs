using Aprendizaje.Dominio.Evidence;

namespace Aprendizaje.Aplicacion.Evidence.Notas.CrearNotaParaTema;

public sealed record CrearNotaParaTemaSolicitud(
    Guid UsuarioId,
    Guid TemaId,
    string Texto,
    TipoNota Tipo = TipoNota.Nota);
