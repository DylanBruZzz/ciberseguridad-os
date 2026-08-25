using Aprendizaje.Aplicacion.Study.Herramientas.CrearHerramienta;
using Aprendizaje.Aplicacion.Study.Herramientas.ListarHerramientas;
using Aprendizaje.Aplicacion.Study.Herramientas.ObtenerHerramientaPorId;
using Aprendizaje.Dominio.Study;
using Aprendizaje.Tests.Soporte;
using Xunit;

namespace Aprendizaje.Tests.Application.Study;

public sealed class HerramientasApplicationTests
{
    [Fact]
    public async Task CrearHerramienta_DebeAgregarHerramientaYGuardar()
    {
        var herramientas = new FakeHerramientaRepository();
        var unitOfWork = new FakeUnitOfWork();
        var casoUso = new CrearHerramientaCasoUso(herramientas, unitOfWork);

        var resultado = await casoUso.EjecutarAsync(
            new CrearHerramientaSolicitud(" Wireshark ", "Redes"),
            CancellationToken);

        Assert.NotEqual(Guid.Empty, resultado.Id);
        Assert.Equal("Wireshark", resultado.Nombre);
        Assert.Equal("Redes", resultado.Categoria);
        Assert.Equal(1, herramientas.AgregarLlamadas);
        Assert.Equal(1, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task ObtenerHerramientaPorId_DebeRetornarHerramientaExistente()
    {
        var herramientas = new FakeHerramientaRepository();
        var herramienta = Herramienta.Crear("Wireshark");
        herramientas.Agregar(herramienta);
        var casoUso = new ObtenerHerramientaPorIdCasoUso(herramientas);

        var resultado = await casoUso.EjecutarAsync(herramienta.Id, CancellationToken);

        Assert.True(resultado.Encontrada);
        Assert.NotNull(resultado.Herramienta);
        Assert.Equal(herramienta.Id, resultado.Herramienta.Id);
        Assert.Equal(herramienta.Nombre, resultado.Herramienta.Nombre);
    }

    [Fact]
    public async Task ObtenerHerramientaPorId_DebeRetornarNoEncontrada()
    {
        var herramientas = new FakeHerramientaRepository();
        var casoUso = new ObtenerHerramientaPorIdCasoUso(herramientas);

        var resultado = await casoUso.EjecutarAsync(Guid.CreateVersion7(), CancellationToken);

        Assert.False(resultado.Encontrada);
        Assert.Null(resultado.Herramienta);
    }

    [Fact]
    public async Task ListarHerramientas_DebeMapearColeccionDelRepository()
    {
        var herramientas = new FakeHerramientaRepository();
        var herramienta = Herramienta.Crear("Wireshark");
        herramientas.Agregar(herramienta);
        var casoUso = new ListarHerramientasCasoUso(herramientas);

        var resultado = await casoUso.EjecutarAsync(CancellationToken);

        var resumen = Assert.Single(resultado.Herramientas);
        Assert.Equal(herramienta.Id, resumen.Id);
        Assert.Equal(herramienta.Nombre, resumen.Nombre);
    }

    private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;
}
