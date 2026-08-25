using Aprendizaje.Dominio.Evidence;

namespace Aprendizaje.Aplicacion.Evidence.ArtefactosTecnicos.CrearArtefactoTecnico;

public sealed record CrearArtefactoTecnicoResultado(
    Guid Id,
    Guid UsuarioId,
    TipoArtefacto TipoArtefacto,
    string Nombre,
    string? ContenidoOUrl,
    string? LenguajeTecnologia,
    EstadoMadurez EstadoMadurez);
