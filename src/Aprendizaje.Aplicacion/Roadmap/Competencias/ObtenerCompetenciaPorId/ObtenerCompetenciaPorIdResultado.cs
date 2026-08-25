namespace Aprendizaje.Aplicacion.Roadmap.Competencias.ObtenerCompetenciaPorId;

public sealed record ObtenerCompetenciaPorIdResultado(bool Encontrada, CompetenciaDetalle? Competencia)
{
    public static ObtenerCompetenciaPorIdResultado EncontradaCon(CompetenciaDetalle competencia) => new(true, competencia);

    public static ObtenerCompetenciaPorIdResultado NoEncontrada() => new(false, null);
}
