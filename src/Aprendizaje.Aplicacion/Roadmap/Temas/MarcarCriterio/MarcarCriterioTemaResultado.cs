namespace Aprendizaje.Aplicacion.Roadmap.Temas.MarcarCriterio;

public enum MarcarCriterioTemaEstado
{
    Actualizado,
    TemaNoEncontrado,
    CriterioNoDefinido
}

public sealed record MarcarCriterioTemaResultado(MarcarCriterioTemaEstado Estado)
{
    public static MarcarCriterioTemaResultado Actualizado() => new(MarcarCriterioTemaEstado.Actualizado);

    public static MarcarCriterioTemaResultado TemaNoEncontrado() => new(MarcarCriterioTemaEstado.TemaNoEncontrado);

    public static MarcarCriterioTemaResultado CriterioNoDefinido() => new(MarcarCriterioTemaEstado.CriterioNoDefinido);
}
