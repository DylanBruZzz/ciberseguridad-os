using Aprendizaje.Dominio.Evidence;
using Xunit;

namespace Aprendizaje.Tests.Dominio.Evidence;

public sealed class LaboratorioTests
{
    [Fact]
    public void Crear_DebeCrearLaboratorioConDatosValidos()
    {
        var usuarioId = Guid.CreateVersion7();

        var laboratorio = Laboratorio.Crear(usuarioId, "Análisis de tráfico OSI");

        Assert.NotEqual(Guid.Empty, laboratorio.Id);
        Assert.Equal(usuarioId, laboratorio.UsuarioId);
        Assert.Equal("Análisis de tráfico OSI", laboratorio.Nombre);
        Assert.Equal(EstadoMadurez.Borrador, laboratorio.EstadoMadurez);
        Assert.Null(laboratorio.FechaEliminacionUtc);
    }

    [Fact]
    public void Crear_DebeNormalizarNombre()
    {
        var laboratorio = Laboratorio.Crear(Guid.CreateVersion7(), "  Análisis de tráfico OSI  ");

        Assert.Equal("Análisis de tráfico OSI", laboratorio.Nombre);
    }

    [Fact]
    public void Crear_DebeRechazarUsuarioVacio()
    {
        Assert.Throws<ArgumentException>(() => Laboratorio.Crear(Guid.Empty, "Análisis de tráfico OSI"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Crear_DebeRechazarNombreVacio(string nombre)
    {
        Assert.Throws<ArgumentException>(() => Laboratorio.Crear(Guid.CreateVersion7(), nombre));
    }

    [Fact]
    public void MetodosPublicos_DebenActualizarPropiedadesOpcionales()
    {
        var fecha = new DateOnly(2026, 8, 24);
        var laboratorio = Laboratorio.Crear(Guid.CreateVersion7(), "Análisis de tráfico OSI");

        laboratorio.ActualizarObjetivo("Identificar capas OSI");
        laboratorio.ActualizarEntornoVms("Kali + Windows");
        laboratorio.RegistrarHallazgos("Tráfico HTTP observado");
        laboratorio.ActualizarTiempoInvertido(45);
        laboratorio.RegistrarFecha(fecha);
        laboratorio.ActualizarFecha(null);
        laboratorio.AvanzarMadurez(EstadoMadurez.Documentado);

        Assert.Equal("Identificar capas OSI", laboratorio.Objetivo);
        Assert.Equal("Kali + Windows", laboratorio.EntornoVms);
        Assert.Equal("Tráfico HTTP observado", laboratorio.Hallazgos);
        Assert.Equal(45, laboratorio.TiempoInvertidoMinutos);
        Assert.Null(laboratorio.Fecha);
        Assert.Equal(EstadoMadurez.Documentado, laboratorio.EstadoMadurez);
    }

    [Fact]
    public void MarcarComoEliminado_DebeRegistrarFechaEliminacion()
    {
        var laboratorio = Laboratorio.Crear(Guid.CreateVersion7(), "Análisis de tráfico OSI");

        laboratorio.MarcarComoEliminado();

        Assert.NotNull(laboratorio.FechaEliminacionUtc);
    }
}
