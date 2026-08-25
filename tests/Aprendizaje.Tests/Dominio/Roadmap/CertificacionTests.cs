using Aprendizaje.Dominio.Roadmap;
using Xunit;

namespace Aprendizaje.Tests.Dominio.Roadmap;

public sealed class CertificacionTests
{
    [Fact]
    public void Crear_DebeCrearCertificacionConDatosValidos()
    {
        var certificacion = Certificacion.Crear("CompTIA Network+", TipoCosto.Pago);

        Assert.NotEqual(Guid.Empty, certificacion.Id);
        Assert.Equal("CompTIA Network+", certificacion.Nombre);
        Assert.Equal(TipoCosto.Pago, certificacion.TipoCosto);
        Assert.Null(certificacion.Proveedor);
        Assert.Null(certificacion.Url);
    }

    [Fact]
    public void Crear_DebeNormalizarNombre()
    {
        var certificacion = Certificacion.Crear("  CompTIA Network+  ", TipoCosto.Gratuita);

        Assert.Equal("CompTIA Network+", certificacion.Nombre);
    }

    [Fact]
    public void Crear_DebeConservarTipoCosto()
    {
        var certificacion = Certificacion.Crear("CompTIA Network+", TipoCosto.Gratuita);

        Assert.Equal(TipoCosto.Gratuita, certificacion.TipoCosto);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Crear_DebeRechazarNombreVacio(string nombre)
    {
        Assert.Throws<ArgumentException>(() => Certificacion.Crear(nombre, TipoCosto.Pago));
    }
}
