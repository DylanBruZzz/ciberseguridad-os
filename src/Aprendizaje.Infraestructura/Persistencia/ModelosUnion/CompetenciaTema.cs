namespace Aprendizaje.Infraestructura.Persistencia.ModelosUnion;

internal sealed class CompetenciaTema
{
    public Guid CompetenciaId { get; private set; }
    public Guid TemaId { get; private set; }

    private CompetenciaTema()
    {
    }

    internal CompetenciaTema(Guid competenciaId, Guid temaId)
    {
        CompetenciaId = competenciaId;
        TemaId = temaId;
    }
}
