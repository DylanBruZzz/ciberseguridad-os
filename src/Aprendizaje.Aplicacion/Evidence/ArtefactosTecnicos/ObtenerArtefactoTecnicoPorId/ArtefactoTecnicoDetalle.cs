using Aprendizaje.Dominio.Evidence;

namespace Aprendizaje.Aplicacion.Evidence.ArtefactosTecnicos.ObtenerArtefactoTecnicoPorId;

public sealed record ArtefactoTecnicoDetalle(
    Guid Id,
    Guid UsuarioId,
    TipoArtefacto TipoArtefacto,
    string Nombre,
    string? ContenidoOUrl,
    string? LenguajeTecnologia,
    EstadoMadurez EstadoMadurez);
