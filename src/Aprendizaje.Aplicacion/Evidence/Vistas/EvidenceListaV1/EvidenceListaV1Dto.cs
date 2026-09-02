using Aprendizaje.Dominio.Evidence;

namespace Aprendizaje.Aplicacion.Evidence.Vistas.EvidenceListaV1;

public sealed record EvidenceListaV1Dto(
    int Total,
    IReadOnlyCollection<EvidenceItemV1Dto> Items);

public sealed record EvidenceItemV1Dto(
    Guid Id,
    TipoEvidenceV1 TipoEvidence,
    string Titulo,
    EstadoMadurez EstadoMadurez,
    DateTime FechaCreacionUtc,
    DateTime? FechaModificacionUtc,
    DateTime FechaActividadUtc,
    DateOnly? FechaReferencia,
    IReadOnlyCollection<EvidenceTemaV1Dto> Temas,
    IReadOnlyCollection<EvidenceHerramientaV1Dto> Herramientas);

public sealed record EvidenceTemaV1Dto(Guid Id, string Nombre);

public sealed record EvidenceHerramientaV1Dto(Guid Id, string Nombre);
