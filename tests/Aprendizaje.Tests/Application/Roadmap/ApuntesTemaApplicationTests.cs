using Aprendizaje.Aplicacion.Roadmap.Temas.GuardarApuntesTema;
using Aprendizaje.Aplicacion.Roadmap.Temas.ObtenerApuntesTema;
using Aprendizaje.Dominio.Roadmap;
using Aprendizaje.Tests.Soporte;
using Xunit;

namespace Aprendizaje.Tests.Application.Roadmap;

public sealed class ApuntesTemaApplicationTests
{
    [Fact]
    public async Task ObtenerApuntes_DebeRetornarTemaNoEncontrado()
    {
        var contexto = CrearContexto();
        var casoUso = new ObtenerApuntesTemaCasoUso(contexto.Apuntes, contexto.Temas);

        var resultado = await casoUso.EjecutarAsync(
            new ObtenerApuntesTemaSolicitud(contexto.UsuarioId, Guid.CreateVersion7()),
            CancellationToken);

        Assert.Equal(ObtenerApuntesTemaEstado.TemaNoEncontrado, resultado.Estado);
        Assert.Null(resultado.Apuntes);
    }

    [Fact]
    public async Task ObtenerApuntes_DebeRetornarUsuarioNoCoincide()
    {
        var contexto = CrearContexto(agregarTema: true);
        var casoUso = new ObtenerApuntesTemaCasoUso(contexto.Apuntes, contexto.Temas);

        var resultado = await casoUso.EjecutarAsync(
            new ObtenerApuntesTemaSolicitud(Guid.CreateVersion7(), contexto.Tema!.Id),
            CancellationToken);

        Assert.Equal(ObtenerApuntesTemaEstado.UsuarioNoCoincide, resultado.Estado);
        Assert.Null(resultado.Apuntes);
    }

    [Fact]
    public async Task ObtenerApuntes_SinRegistro_DebeRetornarContenidoVacio()
    {
        var contexto = CrearContexto(agregarTema: true);
        var casoUso = new ObtenerApuntesTemaCasoUso(contexto.Apuntes, contexto.Temas);

        var resultado = await casoUso.EjecutarAsync(
            new ObtenerApuntesTemaSolicitud(contexto.UsuarioId, contexto.Tema!.Id),
            CancellationToken);

        Assert.Equal(ObtenerApuntesTemaEstado.Encontrado, resultado.Estado);
        Assert.NotNull(resultado.Apuntes);
        Assert.Equal(contexto.Tema.Id, resultado.Apuntes.TemaId);
        Assert.Equal(string.Empty, resultado.Apuntes.Contenido);
        Assert.Null(resultado.Apuntes.FechaModificacionUtc);
    }

    [Fact]
    public async Task GuardarApuntes_DebeCrearCuandoNoExisten()
    {
        var contexto = CrearContexto(agregarTema: true);
        var casoUso = new GuardarApuntesTemaCasoUso(contexto.Apuntes, contexto.Temas, contexto.UnitOfWork);

        var resultado = await casoUso.EjecutarAsync(
            new GuardarApuntesTemaSolicitud(contexto.UsuarioId, contexto.Tema!.Id, "  Capa 3 enruta paquetes  "),
            CancellationToken);

        Assert.Equal(GuardarApuntesTemaEstado.Guardado, resultado.Estado);
        Assert.Equal(1, contexto.Apuntes.AgregarLlamadas);
        Assert.NotNull(contexto.Apuntes.UltimoApunteAgregado);
        Assert.Equal("Capa 3 enruta paquetes", contexto.Apuntes.UltimoApunteAgregado.Contenido);
        Assert.Equal(1, contexto.UnitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task GuardarApuntes_DebeActualizarCuandoExistenSinDuplicar()
    {
        var contexto = CrearContexto(agregarTema: true);
        var existente = ApunteTema.Crear(contexto.UsuarioId, contexto.Tema!.Id, "Inicial");
        contexto.Apuntes.Agregar(existente);
        var casoUso = new GuardarApuntesTemaCasoUso(contexto.Apuntes, contexto.Temas, contexto.UnitOfWork);

        var resultado = await casoUso.EjecutarAsync(
            new GuardarApuntesTemaSolicitud(contexto.UsuarioId, contexto.Tema.Id, "Actualizado"),
            CancellationToken);

        Assert.Equal(GuardarApuntesTemaEstado.Guardado, resultado.Estado);
        Assert.Equal("Actualizado", existente.Contenido);
        Assert.Equal(1, contexto.Apuntes.AgregarLlamadas);
        Assert.Equal(1, contexto.UnitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task GuardarApuntes_DebeRetornarTemaNoEncontradoSinGuardar()
    {
        var contexto = CrearContexto();
        var casoUso = new GuardarApuntesTemaCasoUso(contexto.Apuntes, contexto.Temas, contexto.UnitOfWork);

        var resultado = await casoUso.EjecutarAsync(
            new GuardarApuntesTemaSolicitud(contexto.UsuarioId, Guid.CreateVersion7(), "Apuntes"),
            CancellationToken);

        Assert.Equal(GuardarApuntesTemaEstado.TemaNoEncontrado, resultado.Estado);
        Assert.Equal(0, contexto.Apuntes.AgregarLlamadas);
        Assert.Equal(0, contexto.UnitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task GuardarApuntes_DebeRetornarUsuarioNoCoincideSinGuardar()
    {
        var contexto = CrearContexto(agregarTema: true);
        var casoUso = new GuardarApuntesTemaCasoUso(contexto.Apuntes, contexto.Temas, contexto.UnitOfWork);

        var resultado = await casoUso.EjecutarAsync(
            new GuardarApuntesTemaSolicitud(Guid.CreateVersion7(), contexto.Tema!.Id, "Apuntes"),
            CancellationToken);

        Assert.Equal(GuardarApuntesTemaEstado.UsuarioNoCoincide, resultado.Estado);
        Assert.Equal(0, contexto.Apuntes.AgregarLlamadas);
        Assert.Equal(0, contexto.UnitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task GuardarApuntes_DebePermitirVaciarContenido()
    {
        var contexto = CrearContexto(agregarTema: true);
        var casoUso = new GuardarApuntesTemaCasoUso(contexto.Apuntes, contexto.Temas, contexto.UnitOfWork);

        var resultado = await casoUso.EjecutarAsync(
            new GuardarApuntesTemaSolicitud(contexto.UsuarioId, contexto.Tema!.Id, "   "),
            CancellationToken);

        Assert.Equal(GuardarApuntesTemaEstado.Guardado, resultado.Estado);
        Assert.Equal(string.Empty, contexto.Apuntes.UltimoApunteAgregado!.Contenido);
    }

    private static Contexto CrearContexto(bool agregarTema = false)
    {
        var usuarioId = Guid.CreateVersion7();
        var temas = new FakeTemaRepository();
        var tema = Tema.Crear(usuarioId, "Modelo OSI", TipoConocimiento.Conceptual);

        if (agregarTema)
            temas.Agregar(tema);

        return new Contexto(
            usuarioId,
            agregarTema ? tema : null,
            temas,
            new FakeApunteTemaRepository(),
            new FakeUnitOfWork());
    }

    private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

    private sealed record Contexto(
        Guid UsuarioId,
        Tema? Tema,
        FakeTemaRepository Temas,
        FakeApunteTemaRepository Apuntes,
        FakeUnitOfWork UnitOfWork);
}
