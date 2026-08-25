using Aprendizaje.Dominio.Evidence;

namespace Aprendizaje.Aplicacion.Evidence.Notas.CrearNotaParaProyecto;

public sealed record CrearNotaParaProyectoSolicitud(
    Guid UsuarioId,
    Guid ProyectoId,
    string Texto,
    TipoNota Tipo = TipoNota.Nota);
