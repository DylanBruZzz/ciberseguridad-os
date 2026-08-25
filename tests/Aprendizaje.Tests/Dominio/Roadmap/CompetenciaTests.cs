using Aprendizaje.Dominio.Roadmap;
using Xunit;

namespace Aprendizaje.Tests.Dominio.Roadmap;

public sealed class CompetenciaTests
{
    [Fact]
    public void Crear_DebeCrearCompetenciaValida()
    {
        var usuarioId = Guid.CreateVersion7();

        var competencia = Competencia.Crear(usuarioId, "Comprensión de fundamentos de redes");

        Assert.NotEqual(Guid.Empty, competencia.Id);
        Assert.Equal(usuarioId, competencia.UsuarioId);
        Assert.Equal("Comprensión de fundamentos de redes", competencia.Nombre);
        Assert.Null(competencia.Descripcion);
        Assert.Empty(competencia.EventosDominio);
    }

    [Fact]
    public void Crear_DebeRechazarUsuarioVacio()
    {
        Assert.Throws<ArgumentException>(() =>
            Competencia.Crear(Guid.Empty, "Comprensión de fundamentos de redes"));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Crear_DebeRechazarNombreVacio(string nombre)
    {
        Assert.Throws<ArgumentException>(() =>
            Competencia.Crear(Guid.CreateVersion7(), nombre));
    }

    [Fact]
    public void Crear_DebeNormalizarNombre()
    {
        var competencia = Competencia.Crear(
            Guid.CreateVersion7(),
            "  Comprensión de fundamentos de redes  ");

        Assert.Equal("Comprensión de fundamentos de redes", competencia.Nombre);
    }
}
