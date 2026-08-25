using Aprendizaje.Aplicacion.Study.EntradasBitacora.CrearEntradaBitacora;
using Aprendizaje.Aplicacion.Study.EntradasBitacora.ListarEntradasBitacora;
using Aprendizaje.Aplicacion.Study.EntradasBitacora.ObtenerEntradaBitacoraPorId;
using Aprendizaje.Dominio.Roadmap;
using Aprendizaje.Dominio.Study;
using Aprendizaje.Tests.Soporte;
using Xunit;

namespace Aprendizaje.Tests.Application.Study;

public sealed class EntradasBitacoraApplicationTests
{
    [Fact]
    public async Task CrearEntradaBitacora_DebeCrearEntradaSinTemaYGuardar()
    {
        var entradas = new FakeEntradaBitacoraRepository();
        var temas = new FakeTemaRepository();
        var unitOfWork = new FakeUnitOfWork();
        var casoUso = new CrearEntradaBitacoraCasoUso(entradas, temas, unitOfWork);
        var usuarioId = Guid.CreateVersion7();

        var resultado = await casoUso.EjecutarAsync(
            new CrearEntradaBitacoraSolicitud(usuarioId, " Repaso inicial ", null),
            CancellationToken);

        Assert.Equal(CrearEntradaBitacoraEstado.Creada, resultado.Estado);
        Assert.NotNull(resultado.Id);
        Assert.Equal(usuarioId, resultado.UsuarioId);
        Assert.Null(resultado.TemaId);
        Assert.Equal("Repaso inicial", resultado.Texto);
        Assert.Equal(1, entradas.AgregarLlamadas);
        Assert.Equal(1, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task CrearEntradaBitacora_DebeRetornarTemaNoEncontradoSinGuardar()
    {
        var entradas = new FakeEntradaBitacoraRepository();
        var temas = new FakeTemaRepository();
        var unitOfWork = new FakeUnitOfWork();
        var casoUso = new CrearEntradaBitacoraCasoUso(entradas, temas, unitOfWork);

        var resultado = await casoUso.EjecutarAsync(
            new CrearEntradaBitacoraSolicitud(Guid.CreateVersion7(), "Repaso inicial", Guid.CreateVersion7()),
            CancellationToken);

        Assert.Equal(CrearEntradaBitacoraEstado.TemaNoEncontrado, resultado.Estado);
        Assert.Equal(0, entradas.AgregarLlamadas);
        Assert.Equal(0, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task CrearEntradaBitacora_DebeRetornarUsuarioNoCoincideSinGuardar()
    {
        var entradas = new FakeEntradaBitacoraRepository();
        var temas = new FakeTemaRepository();
        var unitOfWork = new FakeUnitOfWork();
        var tema = Tema.Crear(Guid.CreateVersion7(), "Modelo OSI", TipoConocimiento.Conceptual);
        temas.Agregar(tema);
        var casoUso = new CrearEntradaBitacoraCasoUso(entradas, temas, unitOfWork);

        var resultado = await casoUso.EjecutarAsync(
            new CrearEntradaBitacoraSolicitud(Guid.CreateVersion7(), "Repaso inicial", tema.Id),
            CancellationToken);

        Assert.Equal(CrearEntradaBitacoraEstado.UsuarioNoCoincide, resultado.Estado);
        Assert.Equal(0, entradas.AgregarLlamadas);
        Assert.Equal(0, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task CrearEntradaBitacora_DebeCrearEntradaConTemaYGuardar()
    {
        var entradas = new FakeEntradaBitacoraRepository();
        var temas = new FakeTemaRepository();
        var unitOfWork = new FakeUnitOfWork();
        var tema = Tema.Crear(Guid.CreateVersion7(), "Modelo OSI", TipoConocimiento.Conceptual);
        temas.Agregar(tema);
        var casoUso = new CrearEntradaBitacoraCasoUso(entradas, temas, unitOfWork);

        var resultado = await casoUso.EjecutarAsync(
            new CrearEntradaBitacoraSolicitud(tema.UsuarioId, "Repaso inicial", tema.Id),
            CancellationToken);

        Assert.Equal(CrearEntradaBitacoraEstado.Creada, resultado.Estado);
        Assert.Equal(tema.UsuarioId, resultado.UsuarioId);
        Assert.Equal(tema.Id, resultado.TemaId);
        Assert.Equal(1, entradas.AgregarLlamadas);
        Assert.Equal(1, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task ObtenerEntradaBitacoraPorId_DebeRetornarEntradaExistente()
    {
        var entradas = new FakeEntradaBitacoraRepository();
        var entrada = EntradaBitacora.Escribir(Guid.CreateVersion7(), "Repaso inicial");
        entradas.Agregar(entrada);
        var casoUso = new ObtenerEntradaBitacoraPorIdCasoUso(entradas);

        var resultado = await casoUso.EjecutarAsync(entrada.Id, CancellationToken);

        Assert.True(resultado.Encontrada);
        Assert.NotNull(resultado.Entrada);
        Assert.Equal(entrada.Id, resultado.Entrada.Id);
        Assert.Equal(entrada.Texto, resultado.Entrada.Texto);
    }

    [Fact]
    public async Task ObtenerEntradaBitacoraPorId_DebeRetornarNoEncontrada()
    {
        var entradas = new FakeEntradaBitacoraRepository();
        var casoUso = new ObtenerEntradaBitacoraPorIdCasoUso(entradas);

        var resultado = await casoUso.EjecutarAsync(Guid.CreateVersion7(), CancellationToken);

        Assert.False(resultado.Encontrada);
        Assert.Null(resultado.Entrada);
    }

    [Fact]
    public async Task ListarEntradasBitacora_DebeMapearColeccionDelRepository()
    {
        var entradas = new FakeEntradaBitacoraRepository();
        var usuarioId = Guid.CreateVersion7();
        var entrada = EntradaBitacora.Escribir(usuarioId, "Repaso inicial");
        entradas.Agregar(entrada);
        entradas.Agregar(EntradaBitacora.Escribir(Guid.CreateVersion7(), "Otra entrada"));
        var casoUso = new ListarEntradasBitacoraCasoUso(entradas);

        var resultado = await casoUso.EjecutarAsync(new ListarEntradasBitacoraSolicitud(usuarioId), CancellationToken);

        var resumen = Assert.Single(resultado.Entradas);
        Assert.Equal(entrada.Id, resumen.Id);
        Assert.Equal(usuarioId, resumen.UsuarioId);
        Assert.Equal(entrada.Texto, resumen.Texto);
    }

    private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;
}
