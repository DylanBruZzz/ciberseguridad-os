namespace Aprendizaje.Infraestructura.Persistencia.ModelosUnion;

internal sealed class SesionHerramienta
{
    public Guid SesionId { get; private set; }
    public Guid HerramientaId { get; private set; }

    private SesionHerramienta()
    {
    }

    internal SesionHerramienta(Guid sesionId, Guid herramientaId)
    {
        SesionId = sesionId;
        HerramientaId = herramientaId;
    }
}
