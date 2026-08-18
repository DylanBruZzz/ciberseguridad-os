namespace Aprendizaje.Infraestructura.Persistencia.ModelosUnion;

internal sealed class SesionHerramienta
{
    public Guid SesionId { get; private set; }
    public Guid HerramientaId { get; private set; }
}
