using Aprendizaje.Dominio.Study;
using Xunit;

namespace Aprendizaje.Tests.Dominio.Study;

public sealed class EntradaBitacoraTests
{
    [Fact]
    public void Escribir_DebeCrearEntradaConUsuarioValido()
    {
        var usuarioId = Guid.CreateVersion7();

        var entrada = EntradaBitacora.Escribir(usuarioId, "Repaso inicial");

        Assert.NotEqual(Guid.Empty, entrada.Id);
        Assert.Equal(usuarioId, entrada.UsuarioId);
        Assert.Null(entrada.TemaId);
        Assert.Equal("Repaso inicial", entrada.Texto);
        Assert.True(entrada.Fecha <= DateTime.UtcNow);
    }

    [Fact]
    public void Escribir_DebeRechazarUsuarioVacio()
    {
        Assert.Throws<ArgumentException>(() =>
            EntradaBitacora.Escribir(Guid.Empty, "Repaso inicial"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Escribir_DebeRechazarTextoVacio(string texto)
    {
        Assert.Throws<ArgumentException>(() =>
            EntradaBitacora.Escribir(Guid.CreateVersion7(), texto));
    }

    [Fact]
    public void Escribir_DebeNormalizarTexto()
    {
        var entrada = EntradaBitacora.Escribir(Guid.CreateVersion7(), "  Repaso inicial  ");

        Assert.Equal("Repaso inicial", entrada.Texto);
    }

    [Fact]
    public void Escribir_DebePermitirTemaNulo()
    {
        var entrada = EntradaBitacora.Escribir(Guid.CreateVersion7(), "Repaso inicial", null);

        Assert.Null(entrada.TemaId);
    }

    [Fact]
    public void Escribir_DebeConservarTemaInformado()
    {
        var temaId = Guid.CreateVersion7();

        var entrada = EntradaBitacora.Escribir(Guid.CreateVersion7(), "Repaso inicial", temaId);

        Assert.Equal(temaId, entrada.TemaId);
    }

    [Fact]
    public void ActualizarTexto_DebeNormalizarTexto()
    {
        var entrada = EntradaBitacora.Escribir(Guid.CreateVersion7(), "Repaso inicial");

        entrada.ActualizarTexto("  Repaso actualizado  ");

        Assert.Equal("Repaso actualizado", entrada.Texto);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void ActualizarTexto_DebeRechazarTextoVacio(string texto)
    {
        var entrada = EntradaBitacora.Escribir(Guid.CreateVersion7(), "Repaso inicial");

        Assert.Throws<ArgumentException>(() => entrada.ActualizarTexto(texto));
    }
}
