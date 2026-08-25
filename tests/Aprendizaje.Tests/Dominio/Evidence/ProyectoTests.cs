using Aprendizaje.Dominio.Evidence;
using Xunit;

namespace Aprendizaje.Tests.Dominio.Evidence;

public sealed class ProyectoTests
{
    [Fact]
    public void Crear_DebeCrearProyectoConDatosValidos()
    {
        var usuarioId = Guid.CreateVersion7();

        var proyecto = Proyecto.Crear(usuarioId, "Analizador de tráfico OSI");

        Assert.NotEqual(Guid.Empty, proyecto.Id);
        Assert.Equal(usuarioId, proyecto.UsuarioId);
        Assert.Equal("Analizador de tráfico OSI", proyecto.Nombre);
        Assert.Equal(EstadoProyecto.Idea, proyecto.Estado);
        Assert.Equal(EstadoMadurez.Borrador, proyecto.EstadoMadurez);
        Assert.Null(proyecto.FechaEliminacionUtc);
    }

    [Fact]
    public void Crear_DebeNormalizarNombre()
    {
        var proyecto = Proyecto.Crear(Guid.CreateVersion7(), "  Analizador de tráfico OSI  ");

        Assert.Equal("Analizador de tráfico OSI", proyecto.Nombre);
    }

    [Fact]
    public void Crear_DebeRechazarUsuarioVacio()
    {
        Assert.Throws<ArgumentException>(() => Proyecto.Crear(Guid.Empty, "Analizador de tráfico OSI"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Crear_DebeRechazarNombreVacio(string nombre)
    {
        Assert.Throws<ArgumentException>(() => Proyecto.Crear(Guid.CreateVersion7(), nombre));
    }

    [Fact]
    public void IniciarDesarrollo_DebeRechazarFechaPosteriorAFinRegistrado()
    {
        var proyecto = Proyecto.Crear(Guid.CreateVersion7(), "Analizador de tráfico OSI");
        proyecto.FinalizarDesarrollo(new DateOnly(2026, 8, 24));

        Assert.Throws<InvalidOperationException>(() => proyecto.IniciarDesarrollo(new DateOnly(2026, 8, 25)));
    }

    [Fact]
    public void FinalizarDesarrollo_DebeRechazarFechaAnteriorAInicioRegistrado()
    {
        var proyecto = Proyecto.Crear(Guid.CreateVersion7(), "Analizador de tráfico OSI");
        proyecto.IniciarDesarrollo(new DateOnly(2026, 8, 24));

        Assert.Throws<InvalidOperationException>(() => proyecto.FinalizarDesarrollo(new DateOnly(2026, 8, 23)));
    }
}
