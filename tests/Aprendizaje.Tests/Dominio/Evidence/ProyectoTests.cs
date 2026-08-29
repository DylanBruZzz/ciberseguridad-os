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

    [Fact]
    public void ActualizarFechas_DebePermitirLimpiarFechas()
    {
        var proyecto = Proyecto.Crear(Guid.CreateVersion7(), "Analizador de tráfico OSI");
        proyecto.ActualizarFechas(new DateOnly(2026, 8, 1), new DateOnly(2026, 8, 20));

        proyecto.ActualizarFechas(null, null);

        Assert.Null(proyecto.FechaInicio);
        Assert.Null(proyecto.FechaFin);
    }

    [Fact]
    public void ActualizarFechas_DebeRechazarInicioPosteriorAFin()
    {
        var proyecto = Proyecto.Crear(Guid.CreateVersion7(), "Analizador de tráfico OSI");

        Assert.Throws<InvalidOperationException>(() =>
            proyecto.ActualizarFechas(new DateOnly(2026, 8, 21), new DateOnly(2026, 8, 20)));
    }

    [Fact]
    public void MarcarComoEliminado_DebeRegistrarFechaEliminacion()
    {
        var proyecto = Proyecto.Crear(Guid.CreateVersion7(), "Analizador de tráfico OSI");

        proyecto.MarcarComoEliminado();

        Assert.NotNull(proyecto.FechaEliminacionUtc);
    }
}
