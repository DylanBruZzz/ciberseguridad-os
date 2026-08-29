using Aprendizaje.Dominio.Evidence;
using Xunit;

namespace Aprendizaje.Tests.Dominio.Evidence;

public sealed class WriteupTests
{
    [Fact]
    public void Crear_DebeCrearWriteupConDatosValidos()
    {
        var usuarioId = Guid.CreateVersion7();

        var writeup = Writeup.Crear(usuarioId, "Análisis del modelo OSI con Wireshark");

        Assert.NotEqual(Guid.Empty, writeup.Id);
        Assert.Equal(usuarioId, writeup.UsuarioId);
        Assert.Equal("Análisis del modelo OSI con Wireshark", writeup.Titulo);
        Assert.Null(writeup.PlataformaOrigen);
        Assert.Null(writeup.Url);
        Assert.Equal(EstadoMadurez.Borrador, writeup.EstadoMadurez);
        Assert.Null(writeup.Fecha);
        Assert.Null(writeup.FechaEliminacionUtc);
    }

    [Fact]
    public void Crear_DebeNormalizarTitulo()
    {
        var writeup = Writeup.Crear(Guid.CreateVersion7(), "  Análisis del modelo OSI con Wireshark  ");

        Assert.Equal("Análisis del modelo OSI con Wireshark", writeup.Titulo);
    }

    [Fact]
    public void Crear_DebeRechazarUsuarioVacio()
    {
        Assert.Throws<ArgumentException>(() => Writeup.Crear(
            Guid.Empty,
            "Análisis del modelo OSI con Wireshark"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Crear_DebeRechazarTituloVacio(string titulo)
    {
        Assert.Throws<ArgumentException>(() => Writeup.Crear(Guid.CreateVersion7(), titulo));
    }

    [Fact]
    public void MetodosPublicos_DebenActualizarCamposMadurezYPermitirLimpiarFecha()
    {
        var writeup = Writeup.Crear(Guid.CreateVersion7(), "Análisis del modelo OSI con Wireshark");

        writeup.ActualizarPlataformaOrigen("Hack The Box");
        writeup.ActualizarUrl("https://example.local/writeup");
        writeup.ActualizarFecha(new DateOnly(2026, 8, 27));
        writeup.ActualizarFecha(null);
        writeup.AvanzarMadurez(EstadoMadurez.ListoPortafolio);

        Assert.Equal("Hack The Box", writeup.PlataformaOrigen);
        Assert.Equal("https://example.local/writeup", writeup.Url);
        Assert.Null(writeup.Fecha);
        Assert.Equal(EstadoMadurez.ListoPortafolio, writeup.EstadoMadurez);
    }

    [Fact]
    public void MarcarComoEliminado_DebeRegistrarFechaEliminacion()
    {
        var writeup = Writeup.Crear(Guid.CreateVersion7(), "Análisis del modelo OSI con Wireshark");

        writeup.MarcarComoEliminado();

        Assert.NotNull(writeup.FechaEliminacionUtc);
    }
}
