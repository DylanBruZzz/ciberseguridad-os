namespace Aprendizaje.Infraestructura.Persistencia.ModelosUnion;

internal sealed class RecursoTema
{
    public Guid RecursoId { get; private set; }
    public Guid TemaId { get; private set; }

    private RecursoTema()
    {
    }

    internal RecursoTema(Guid recursoId, Guid temaId)
    {
        RecursoId = recursoId;
        TemaId = temaId;
    }
}
