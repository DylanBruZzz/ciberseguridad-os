namespace Aprendizaje.Infraestructura.Persistencia.ModelosUnion;

internal sealed class CertificacionTema
{
    public Guid CertificacionId { get; private set; }
    public Guid TemaId { get; private set; }
    public decimal? Peso { get; private set; }
}
