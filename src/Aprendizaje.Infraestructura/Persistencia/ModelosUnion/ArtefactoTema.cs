namespace Aprendizaje.Infraestructura.Persistencia.ModelosUnion;

internal sealed class ArtefactoTema
{
    public Guid ArtefactoTecnicoId { get; private set; }
    public Guid TemaId { get; private set; }

    private ArtefactoTema()
    {
    }

    internal ArtefactoTema(Guid artefactoTecnicoId, Guid temaId)
    {
        ArtefactoTecnicoId = artefactoTecnicoId;
        TemaId = temaId;
    }
}
