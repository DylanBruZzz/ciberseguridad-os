using Aprendizaje.Dominio.Evidence;

namespace Aprendizaje.Aplicacion.Portafolio;

public sealed record ObtenerPortafolioSolicitud(
    Guid UsuarioId,
    TipoEvidencePortafolio? TipoEvidence = null,
    EstadoMadurez? EstadoMadurez = null);
