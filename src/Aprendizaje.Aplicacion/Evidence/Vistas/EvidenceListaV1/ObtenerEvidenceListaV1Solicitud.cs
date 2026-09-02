using Aprendizaje.Dominio.Evidence;

namespace Aprendizaje.Aplicacion.Evidence.Vistas.EvidenceListaV1;

public sealed record ObtenerEvidenceListaV1Solicitud(
    Guid UsuarioId,
    TipoEvidenceV1? TipoEvidence = null,
    EstadoMadurez? EstadoMadurez = null,
    Guid? TemaId = null);
