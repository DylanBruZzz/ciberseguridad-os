using Aprendizaje.Dominio.Evidence;

namespace Aprendizaje.Aplicacion.Evidence.ArtefactosTecnicos.ActualizarArtefactoTecnico;

public sealed record ActualizarArtefactoTecnicoSolicitud(
    Guid ArtefactoTecnicoId,
    Guid UsuarioId,
    TipoArtefacto TipoArtefacto,
    string Nombre,
    string? ContenidoOUrl,
    string? LenguajeTecnologia,
    EstadoMadurez EstadoMadurez);
