namespace Aprendizaje.Infraestructura.Persistencia.ModelosUnion;

internal sealed class WriteupTema
{
    public Guid WriteupId { get; private set; }
    public Guid TemaId { get; private set; }

    private WriteupTema()
    {
    }

    internal WriteupTema(Guid writeupId, Guid temaId)
    {
        WriteupId = writeupId;
        TemaId = temaId;
    }
}
