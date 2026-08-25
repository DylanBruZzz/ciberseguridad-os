namespace Aprendizaje.Infraestructura.Persistencia.ModelosUnion;

internal sealed class LaboratorioTema
{
    public Guid LaboratorioId { get; private set; }
    public Guid TemaId { get; private set; }

    private LaboratorioTema()
    {
    }

    internal LaboratorioTema(Guid laboratorioId, Guid temaId)
    {
        LaboratorioId = laboratorioId;
        TemaId = temaId;
    }
}
