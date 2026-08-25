namespace Aprendizaje.Infraestructura.Persistencia.ModelosUnion;

internal sealed class LaboratorioHerramienta
{
    public Guid LaboratorioId { get; private set; }
    public Guid HerramientaId { get; private set; }

    private LaboratorioHerramienta()
    {
    }

    internal LaboratorioHerramienta(Guid laboratorioId, Guid herramientaId)
    {
        LaboratorioId = laboratorioId;
        HerramientaId = herramientaId;
    }
}
