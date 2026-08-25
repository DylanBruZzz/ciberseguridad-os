using Aprendizaje.Dominio.Evidence;

namespace Aprendizaje.Aplicacion.Evidence.Notas.CrearNotaParaArtefactoTecnico;

public sealed record CrearNotaParaArtefactoTecnicoSolicitud(
    Guid UsuarioId,
    Guid ArtefactoTecnicoId,
    string Texto,
    TipoNota Tipo = TipoNota.Nota);
