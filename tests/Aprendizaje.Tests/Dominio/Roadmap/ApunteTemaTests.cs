using Aprendizaje.Dominio.Roadmap;
using Xunit;

namespace Aprendizaje.Tests.Dominio.Roadmap;

public sealed class ApunteTemaTests
{
    [Fact]
    public void Crear_DebeNormalizarContenido()
    {
        var usuarioId = Guid.CreateVersion7();
        var temaId = Guid.CreateVersion7();

        var apunte = ApunteTema.Crear(usuarioId, temaId, "  Modelo OSI en mis palabras  ");

        Assert.Equal(usuarioId, apunte.UsuarioId);
        Assert.Equal(temaId, apunte.TemaId);
        Assert.Equal("Modelo OSI en mis palabras", apunte.Contenido);
    }

    [Fact]
    public void Crear_DebePermitirContenidoVacioNormalizado()
    {
        var apunte = ApunteTema.Crear(Guid.CreateVersion7(), Guid.CreateVersion7(), "  ");

        Assert.Equal(string.Empty, apunte.Contenido);
    }

    [Fact]
    public void Crear_DebeRechazarUsuarioVacio()
    {
        Assert.Throws<ArgumentException>(() =>
            ApunteTema.Crear(Guid.Empty, Guid.CreateVersion7(), "Apuntes"));
    }

    [Fact]
    public void Crear_DebeRechazarTemaVacio()
    {
        Assert.Throws<ArgumentException>(() =>
            ApunteTema.Crear(Guid.CreateVersion7(), Guid.Empty, "Apuntes"));
    }

    [Fact]
    public void Crear_DebeRechazarContenidoNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            ApunteTema.Crear(Guid.CreateVersion7(), Guid.CreateVersion7(), null!));
    }

    [Fact]
    public void ReemplazarContenido_DebeActualizarContenidoNormalizado()
    {
        var apunte = ApunteTema.Crear(Guid.CreateVersion7(), Guid.CreateVersion7(), "Inicial");

        apunte.ReemplazarContenido("  Actualizado  ");

        Assert.Equal("Actualizado", apunte.Contenido);
    }
}
