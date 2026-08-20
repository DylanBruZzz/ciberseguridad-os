namespace Aprendizaje.Aplicacion.Roadmap.Temas.DefinirCriteriosRelevantes;

public enum DefinirCriteriosRelevantesTemaEstado
{
    Actualizado,
    TemaNoEncontrado,
    ProgresoRegistrado
}

public sealed record DefinirCriteriosRelevantesTemaResultado(DefinirCriteriosRelevantesTemaEstado Estado)
{
    public static DefinirCriteriosRelevantesTemaResultado Actualizado() =>
        new(DefinirCriteriosRelevantesTemaEstado.Actualizado);

    public static DefinirCriteriosRelevantesTemaResultado TemaNoEncontrado() =>
        new(DefinirCriteriosRelevantesTemaEstado.TemaNoEncontrado);

    public static DefinirCriteriosRelevantesTemaResultado ProgresoRegistrado() =>
        new(DefinirCriteriosRelevantesTemaEstado.ProgresoRegistrado);
}
