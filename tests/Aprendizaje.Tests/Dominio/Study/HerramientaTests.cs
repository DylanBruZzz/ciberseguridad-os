using Aprendizaje.Dominio.Study;
using Xunit;

namespace Aprendizaje.Tests.Dominio.Study;

public sealed class HerramientaTests
{
    [Fact]
    public void Crear_DebeCrearHerramientaConNombreValido()
    {
        var herramienta = Herramienta.Crear("Wireshark");

        Assert.NotEqual(Guid.Empty, herramienta.Id);
        Assert.Equal("Wireshark", herramienta.Nombre);
        Assert.Null(herramienta.Categoria);
    }

    [Fact]
    public void Crear_DebeNormalizarNombre()
    {
        var herramienta = Herramienta.Crear("  Wireshark  ");

        Assert.Equal("Wireshark", herramienta.Nombre);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Crear_DebeRechazarNombreVacio(string nombre)
    {
        Assert.Throws<ArgumentException>(() => Herramienta.Crear(nombre));
    }

    [Fact]
    public void ActualizarCategoria_DebeAsignarCategoria()
    {
        var herramienta = Herramienta.Crear("Wireshark");

        herramienta.ActualizarCategoria("Redes");

        Assert.Equal("Redes", herramienta.Categoria);
    }

    [Fact]
    public void CambiarNombre_DebeNormalizarNombre()
    {
        var herramienta = Herramienta.Crear("Wireshark");

        herramienta.CambiarNombre("  Nmap  ");

        Assert.Equal("Nmap", herramienta.Nombre);
    }
}
