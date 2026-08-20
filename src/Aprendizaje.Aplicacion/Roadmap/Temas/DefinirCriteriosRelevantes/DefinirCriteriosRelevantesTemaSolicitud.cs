using Aprendizaje.Dominio.Roadmap;

namespace Aprendizaje.Aplicacion.Roadmap.Temas.DefinirCriteriosRelevantes;

public sealed record DefinirCriteriosRelevantesTemaSolicitud(
    Guid TemaId,
    IReadOnlyCollection<TipoCriterio> Criterios);
