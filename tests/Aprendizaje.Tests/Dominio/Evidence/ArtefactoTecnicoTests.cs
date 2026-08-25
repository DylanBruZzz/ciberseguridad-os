using Aprendizaje.Dominio.Evidence;
using Xunit;

namespace Aprendizaje.Tests.Dominio.Evidence;

public sealed class ArtefactoTecnicoTests
{
    [Fact]
    public void Crear_DebeCrearArtefactoTecnicoConDatosValidos()
    {
        var usuarioId = Guid.CreateVersion7();

        var artefacto = ArtefactoTecnico.Crear(
            usuarioId,
            TipoArtefacto.Cheatsheet,
            "Filtros Wireshark para análisis OSI");

        Assert.NotEqual(Guid.Empty, artefacto.Id);
        Assert.Equal(usuarioId, artefacto.UsuarioId);
        Assert.Equal(TipoArtefacto.Cheatsheet, artefacto.TipoArtefacto);
        Assert.Equal("Filtros Wireshark para análisis OSI", artefacto.Nombre);
        Assert.Null(artefacto.ContenidoOUrl);
        Assert.Null(artefacto.LenguajeTecnologia);
        Assert.Equal(EstadoMadurez.Borrador, artefacto.EstadoMadurez);
        Assert.Null(artefacto.FechaEliminacionUtc);
    }

    [Fact]
    public void Crear_DebeNormalizarNombre()
    {
        var artefacto = ArtefactoTecnico.Crear(
            Guid.CreateVersion7(),
            TipoArtefacto.Cheatsheet,
            "  Filtros Wireshark para análisis OSI  ");

        Assert.Equal("Filtros Wireshark para análisis OSI", artefacto.Nombre);
    }

    [Fact]
    public void Crear_DebeRechazarUsuarioVacio()
    {
        Assert.Throws<ArgumentException>(() => ArtefactoTecnico.Crear(
            Guid.Empty,
            TipoArtefacto.Cheatsheet,
            "Filtros Wireshark para análisis OSI"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Crear_DebeRechazarNombreVacio(string nombre)
    {
        Assert.Throws<ArgumentException>(() => ArtefactoTecnico.Crear(
            Guid.CreateVersion7(),
            TipoArtefacto.Cheatsheet,
            nombre));
    }
}
