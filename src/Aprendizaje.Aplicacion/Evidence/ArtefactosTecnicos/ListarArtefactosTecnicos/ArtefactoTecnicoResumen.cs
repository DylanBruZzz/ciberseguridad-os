using Aprendizaje.Dominio.Evidence;

namespace Aprendizaje.Aplicacion.Evidence.ArtefactosTecnicos.ListarArtefactosTecnicos;

public sealed record ArtefactoTecnicoResumen(
    Guid Id,
    Guid UsuarioId,
    TipoArtefacto TipoArtefacto,
    string Nombre,
    string? LenguajeTecnologia,
    EstadoMadurez EstadoMadurez);
