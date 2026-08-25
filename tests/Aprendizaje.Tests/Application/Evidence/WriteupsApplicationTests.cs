using Aprendizaje.Aplicacion.Evidence.Writeups.CrearWriteup;
using Aprendizaje.Aplicacion.Evidence.Writeups.ListarWriteups;
using Aprendizaje.Aplicacion.Evidence.Writeups.ObtenerWriteupPorId;
using Aprendizaje.Aplicacion.Evidence.Writeups.VincularWriteupATema;
using Aprendizaje.Dominio.Evidence;
using Aprendizaje.Dominio.Roadmap;
using Aprendizaje.Tests.Soporte;
using Xunit;

namespace Aprendizaje.Tests.Application.Evidence;

public sealed class WriteupsApplicationTests
{
    [Fact]
    public async Task CrearWriteup_DebeAgregarWriteupYGuardar()
    {
        var writeups = new FakeWriteupRepository();
        var unitOfWork = new FakeUnitOfWork();
        var casoUso = new CrearWriteupCasoUso(writeups, unitOfWork);
        var usuarioId = Guid.CreateVersion7();

        var resultado = await casoUso.EjecutarAsync(
            new CrearWriteupSolicitud(usuarioId, " Análisis del modelo OSI con Wireshark "),
            CancellationToken);

        Assert.NotEqual(Guid.Empty, resultado.Id);
        Assert.Equal(usuarioId, resultado.UsuarioId);
        Assert.Equal("Análisis del modelo OSI con Wireshark", resultado.Titulo);
        Assert.Null(resultado.PlataformaOrigen);
        Assert.Null(resultado.Url);
        Assert.Equal(EstadoMadurez.Borrador, resultado.EstadoMadurez);
        Assert.Null(resultado.Fecha);
        Assert.NotNull(await writeups.ObtenerPorIdAsync(resultado.Id, CancellationToken));
        Assert.Equal(1, writeups.AgregarLlamadas);
        Assert.Equal(1, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task CrearWriteup_DebeRechazarUsuarioVacioSinGuardar()
    {
        var writeups = new FakeWriteupRepository();
        var unitOfWork = new FakeUnitOfWork();
        var casoUso = new CrearWriteupCasoUso(writeups, unitOfWork);

        await Assert.ThrowsAsync<ArgumentException>(() => casoUso.EjecutarAsync(
            new CrearWriteupSolicitud(Guid.Empty, "Análisis del modelo OSI con Wireshark"),
            CancellationToken));

        Assert.Equal(0, writeups.AgregarLlamadas);
        Assert.Equal(0, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task ObtenerWriteupPorId_DebeRetornarWriteupExistente()
    {
        var writeups = new FakeWriteupRepository();
        var writeup = CrearWriteup();
        writeups.Agregar(writeup);
        var casoUso = new ObtenerWriteupPorIdCasoUso(writeups);

        var resultado = await casoUso.EjecutarAsync(writeup.Id, CancellationToken);

        Assert.True(resultado.Encontrado);
        Assert.NotNull(resultado.Writeup);
        Assert.Equal(writeup.Id, resultado.Writeup.Id);
        Assert.Equal(writeup.Titulo, resultado.Writeup.Titulo);
    }

    [Fact]
    public async Task ObtenerWriteupPorId_DebeRetornarNoEncontrado()
    {
        var writeups = new FakeWriteupRepository();
        var casoUso = new ObtenerWriteupPorIdCasoUso(writeups);

        var resultado = await casoUso.EjecutarAsync(Guid.CreateVersion7(), CancellationToken);

        Assert.False(resultado.Encontrado);
        Assert.Null(resultado.Writeup);
    }

    [Fact]
    public async Task ListarWriteups_DebeMapearColeccionDelRepository()
    {
        var writeups = new FakeWriteupRepository();
        var usuarioId = Guid.CreateVersion7();
        var writeup = CrearWriteup(usuarioId);
        writeups.Agregar(writeup);
        writeups.Agregar(CrearWriteup(Guid.CreateVersion7()));
        var casoUso = new ListarWriteupsCasoUso(writeups);

        var resultado = await casoUso.EjecutarAsync(new ListarWriteupsSolicitud(usuarioId), CancellationToken);

        var resumen = Assert.Single(resultado.Writeups);
        Assert.Equal(writeup.Id, resumen.Id);
        Assert.Equal(usuarioId, resumen.UsuarioId);
        Assert.Equal(writeup.Titulo, resumen.Titulo);
        Assert.Equal(writeup.EstadoMadurez, resumen.EstadoMadurez);
    }

    [Fact]
    public async Task VincularWriteupATema_DebeRetornarWriteupNoEncontradoSinGuardar()
    {
        var writeups = new FakeWriteupRepository();
        var temas = new FakeTemaRepository();
        var unitOfWork = new FakeUnitOfWork();
        var casoUso = new VincularWriteupATemaCasoUso(writeups, temas, unitOfWork);

        var resultado = await casoUso.EjecutarAsync(
            new VincularWriteupATemaSolicitud(Guid.CreateVersion7(), Guid.CreateVersion7()),
            CancellationToken);

        Assert.Equal(VincularWriteupATemaEstado.WriteupNoEncontrado, resultado.Estado);
        Assert.Equal(0, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task VincularWriteupATema_DebeRetornarTemaNoEncontradoSinGuardar()
    {
        var writeups = new FakeWriteupRepository();
        var temas = new FakeTemaRepository();
        var unitOfWork = new FakeUnitOfWork();
        var writeup = CrearWriteup();
        writeups.Agregar(writeup);
        var casoUso = new VincularWriteupATemaCasoUso(writeups, temas, unitOfWork);

        var resultado = await casoUso.EjecutarAsync(
            new VincularWriteupATemaSolicitud(writeup.Id, Guid.CreateVersion7()),
            CancellationToken);

        Assert.Equal(VincularWriteupATemaEstado.TemaNoEncontrado, resultado.Estado);
        Assert.Equal(0, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task VincularWriteupATema_DebeRetornarUsuarioNoCoincideSinGuardar()
    {
        var writeups = new FakeWriteupRepository();
        var temas = new FakeTemaRepository();
        var unitOfWork = new FakeUnitOfWork();
        var writeup = CrearWriteup();
        var tema = Tema.Crear(Guid.CreateVersion7(), "Modelo OSI", TipoConocimiento.Conceptual);
        writeups.Agregar(writeup);
        temas.Agregar(tema);
        var casoUso = new VincularWriteupATemaCasoUso(writeups, temas, unitOfWork);

        var resultado = await casoUso.EjecutarAsync(
            new VincularWriteupATemaSolicitud(writeup.Id, tema.Id),
            CancellationToken);

        Assert.Equal(VincularWriteupATemaEstado.UsuarioNoCoincide, resultado.Estado);
        Assert.Equal(0, writeups.VincularTemaLlamadas);
        Assert.Equal(0, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task VincularWriteupATema_DebeVincularYGuardar()
    {
        var writeups = new FakeWriteupRepository();
        var temas = new FakeTemaRepository();
        var unitOfWork = new FakeUnitOfWork();
        var usuarioId = Guid.CreateVersion7();
        var writeup = CrearWriteup(usuarioId);
        var tema = Tema.Crear(usuarioId, "Modelo OSI", TipoConocimiento.Conceptual);
        writeups.Agregar(writeup);
        temas.Agregar(tema);
        var casoUso = new VincularWriteupATemaCasoUso(writeups, temas, unitOfWork);

        var resultado = await casoUso.EjecutarAsync(
            new VincularWriteupATemaSolicitud(writeup.Id, tema.Id),
            CancellationToken);

        Assert.Equal(VincularWriteupATemaEstado.Actualizado, resultado.Estado);
        Assert.True(await writeups.ExisteVinculoTemaAsync(writeup.Id, tema.Id, CancellationToken));
        Assert.Equal(1, writeups.VincularTemaLlamadas);
        Assert.Equal(1, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task VincularWriteupATema_DebeSerIdempotenteSinGuardarDeNuevo()
    {
        var writeups = new FakeWriteupRepository();
        var temas = new FakeTemaRepository();
        var unitOfWork = new FakeUnitOfWork();
        var usuarioId = Guid.CreateVersion7();
        var writeup = CrearWriteup(usuarioId);
        var tema = Tema.Crear(usuarioId, "Modelo OSI", TipoConocimiento.Conceptual);
        writeups.Agregar(writeup);
        temas.Agregar(tema);
        var casoUso = new VincularWriteupATemaCasoUso(writeups, temas, unitOfWork);
        await casoUso.EjecutarAsync(new VincularWriteupATemaSolicitud(writeup.Id, tema.Id), CancellationToken);

        var resultado = await casoUso.EjecutarAsync(
            new VincularWriteupATemaSolicitud(writeup.Id, tema.Id),
            CancellationToken);

        Assert.Equal(VincularWriteupATemaEstado.Actualizado, resultado.Estado);
        Assert.Equal(1, writeups.VincularTemaLlamadas);
        Assert.Equal(1, unitOfWork.GuardarCambiosLlamadas);
    }

    private static Writeup CrearWriteup(Guid? usuarioId = null) =>
        Writeup.Crear(usuarioId ?? Guid.CreateVersion7(), "Análisis del modelo OSI con Wireshark");

    private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;
}
