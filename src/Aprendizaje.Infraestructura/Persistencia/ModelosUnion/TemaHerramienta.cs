namespace Aprendizaje.Infraestructura.Persistencia.ModelosUnion;

internal sealed class TemaHerramienta
{
    public Guid TemaId { get; private set; }
    public Guid HerramientaId { get; private set; }

    private TemaHerramienta()
    {
    }

    internal TemaHerramienta(Guid temaId, Guid herramientaId)
    {
        TemaId = temaId;
        HerramientaId = herramientaId;
    }
}
