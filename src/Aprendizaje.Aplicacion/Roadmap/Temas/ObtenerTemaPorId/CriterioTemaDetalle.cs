using Aprendizaje.Dominio.Roadmap;

namespace Aprendizaje.Aplicacion.Roadmap.Temas.ObtenerTemaPorId;

public sealed record CriterioTemaDetalle(
    TipoCriterio Tipo,
    string? Descripcion,
    bool Cumplido,
    DateTime? FechaCumplido);
