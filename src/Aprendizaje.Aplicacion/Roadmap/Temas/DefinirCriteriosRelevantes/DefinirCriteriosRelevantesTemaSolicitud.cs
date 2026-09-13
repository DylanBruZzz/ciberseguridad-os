using Aprendizaje.Dominio.Roadmap;

namespace Aprendizaje.Aplicacion.Roadmap.Temas.DefinirCriteriosRelevantes;

public sealed record DefinirCriteriosRelevantesTemaSolicitud(
    Guid TemaId,
    IReadOnlyCollection<DefinicionCriterioTemaSolicitud> Criterios);

public sealed record DefinicionCriterioTemaSolicitud(
    TipoCriterio Tipo,
    string Descripcion);
