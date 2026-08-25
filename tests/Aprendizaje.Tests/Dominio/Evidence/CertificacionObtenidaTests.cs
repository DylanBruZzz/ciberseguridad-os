using Aprendizaje.Dominio.Evidence;
using Xunit;

namespace Aprendizaje.Tests.Dominio.Evidence;

public sealed class CertificacionObtenidaTests
{
    [Fact]
    public void Registrar_DebeCrearCertificacionObtenidaConDatosValidos()
    {
        var usuarioId = Guid.CreateVersion7();
        var certificacionId = Guid.CreateVersion7();
        var fechaObtencion = new DateOnly(2026, 8, 25);

        var certificacionObtenida = CertificacionObtenida.Registrar(usuarioId, certificacionId, fechaObtencion);

        Assert.NotEqual(Guid.Empty, certificacionObtenida.Id);
        Assert.Equal(usuarioId, certificacionObtenida.UsuarioId);
        Assert.Equal(certificacionId, certificacionObtenida.CertificacionId);
        Assert.Equal(fechaObtencion, certificacionObtenida.FechaObtencion);
        Assert.Equal(EstadoMadurez.Documentado, certificacionObtenida.EstadoMadurez);
        Assert.Null(certificacionObtenida.EvidenciaUrl);
        Assert.Null(certificacionObtenida.FechaEliminacionUtc);
    }

    [Fact]
    public void Registrar_DebeRechazarUsuarioVacio()
    {
        Assert.Throws<ArgumentException>(() => CertificacionObtenida.Registrar(
            Guid.Empty,
            Guid.CreateVersion7(),
            new DateOnly(2026, 8, 25)));
    }

    [Fact]
    public void Registrar_DebeRechazarCertificacionVacia()
    {
        Assert.Throws<ArgumentException>(() => CertificacionObtenida.Registrar(
            Guid.CreateVersion7(),
            Guid.Empty,
            new DateOnly(2026, 8, 25)));
    }

    [Fact]
    public void Registrar_DebeConservarFechaObtencion()
    {
        var fechaObtencion = new DateOnly(2025, 5, 10);

        var certificacionObtenida = CertificacionObtenida.Registrar(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            fechaObtencion);

        Assert.Equal(fechaObtencion, certificacionObtenida.FechaObtencion);
    }
}
