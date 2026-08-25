namespace Aprendizaje.Infraestructura.Persistencia.ModelosUnion;

internal sealed class ProyectoTema
{
    public Guid ProyectoId { get; private set; }
    public Guid TemaId { get; private set; }

    private ProyectoTema()
    {
    }

    internal ProyectoTema(Guid proyectoId, Guid temaId)
    {
        ProyectoId = proyectoId;
        TemaId = temaId;
    }
}
