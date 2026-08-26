namespace Aprendizaje.Infraestructura.Persistencia.ModelosUnion;

internal sealed class CertificacionTema
{
    private CertificacionTema()
    {
    }

    internal CertificacionTema(Guid certificacionId, Guid temaId)
    {
        CertificacionId = certificacionId;
        TemaId = temaId;
    }

    public Guid CertificacionId { get; private set; }
    public Guid TemaId { get; private set; }
    public decimal? Peso { get; private set; }
}
