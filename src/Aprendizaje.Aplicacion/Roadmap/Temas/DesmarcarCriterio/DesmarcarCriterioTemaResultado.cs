namespace Aprendizaje.Aplicacion.Roadmap.Temas.DesmarcarCriterio;

public enum DesmarcarCriterioTemaEstado
{
    Actualizado,
    TemaNoEncontrado,
    CriterioNoDefinido
}

public sealed record DesmarcarCriterioTemaResultado(DesmarcarCriterioTemaEstado Estado)
{
    public static DesmarcarCriterioTemaResultado Actualizado() => new(DesmarcarCriterioTemaEstado.Actualizado);

    public static DesmarcarCriterioTemaResultado TemaNoEncontrado() => new(DesmarcarCriterioTemaEstado.TemaNoEncontrado);

    public static DesmarcarCriterioTemaResultado CriterioNoDefinido() =>
        new(DesmarcarCriterioTemaEstado.CriterioNoDefinido);
}
