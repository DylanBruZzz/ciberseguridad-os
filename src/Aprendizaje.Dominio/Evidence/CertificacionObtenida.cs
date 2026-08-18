using Aprendizaje.Dominio.Comun;

namespace Aprendizaje.Dominio.Evidence;

/// <summary>
/// Instancia real de una certificación obtenida por el usuario (distinta del catálogo
/// roadmap.Certificacion, al que solo referencia por Guid). Su EstadoMadurez por defecto es
/// Documentado, no Borrador: la certificación en sí ya es la evidencia, a diferencia de un
/// Proyecto/Laboratorio/Writeup que empieza sin documentar. Sin VersionFila, sin Domain Event
/// ni Value Objects: mismos criterios ya aplicados al resto de Evidence.
/// </summary>
public sealed class CertificacionObtenida : AggregateRoot, IEliminableLogicamente
{
    public Guid UsuarioId { get; private set; }
    public Guid CertificacionId { get; private set; }
    public DateOnly FechaObtencion { get; private set; }
    public string? EvidenciaUrl { get; private set; }
    public EstadoMadurez EstadoMadurez { get; private set; }

    public DateTime? FechaEliminacionUtc { get; private set; }

    private CertificacionObtenida() { } // requerido por EF Core

    private CertificacionObtenida(Guid id, Guid usuarioId, Guid certificacionId, DateOnly fechaObtencion) : base(id)
    {
        UsuarioId = usuarioId;
        CertificacionId = certificacionId;
        FechaObtencion = fechaObtencion;
        EstadoMadurez = EstadoMadurez.Documentado;
    }

    public static CertificacionObtenida Registrar(Guid usuarioId, Guid certificacionId, DateOnly fechaObtencion)
    {
        if (usuarioId == Guid.Empty)
            throw new ArgumentException("La Certificación obtenida debe pertenecer a un usuario válido.", nameof(usuarioId));

        if (certificacionId == Guid.Empty)
            throw new ArgumentException(
                "La Certificación obtenida debe referenciar una Certificación válida del catálogo.", nameof(certificacionId));

        return new CertificacionObtenida(Guid.CreateVersion7(), usuarioId, certificacionId, fechaObtencion);
    }

    public void ActualizarEvidenciaUrl(string? evidenciaUrl) => EvidenciaUrl = evidenciaUrl;

    public void AvanzarMadurez(EstadoMadurez estadoMadurez) => EstadoMadurez = estadoMadurez;

    public void MarcarComoEliminado() => FechaEliminacionUtc = DateTime.UtcNow;
}
