namespace Aprendizaje.Infraestructura.Persistencia.ModelosUnion;

internal sealed class ProyectoHerramienta
{
    public Guid ProyectoId { get; private set; }
    public Guid HerramientaId { get; private set; }

    private ProyectoHerramienta()
    {
    }

    internal ProyectoHerramienta(Guid proyectoId, Guid herramientaId)
    {
        ProyectoId = proyectoId;
        HerramientaId = herramientaId;
    }
}
