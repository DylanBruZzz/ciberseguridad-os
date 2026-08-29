using Aprendizaje.Aplicacion.Evidence.Writeups.ActualizarWriteup;
using Aprendizaje.Aplicacion.Evidence.Writeups.CrearWriteup;
using Aprendizaje.Aplicacion.Evidence.Writeups.EliminarWriteup;
using Aprendizaje.Aplicacion.Evidence.Writeups.ListarWriteups;
using Aprendizaje.Aplicacion.Evidence.Writeups.ObtenerWriteupPorId;
using Aprendizaje.Aplicacion.Evidence.Writeups.VincularWriteupATema;
using Aprendizaje.Dominio.Evidence;
using Aprendizaje.Dominio.Nucleo;
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

    [Fact]
    public async Task ActualizarWriteup_DebeActualizarCamposMadurezYGuardar()
    {
        var writeups = new FakeWriteupRepository();
        var usuarios = new FakeUsuarioRepository();
        var unitOfWork = new FakeUnitOfWork();
        var usuario = Usuario.Registrar("Usuario Writeup", EmailUnico());
        var writeup = CrearWriteup(usuario.Id);
        usuarios.Agregar(usuario);
        writeups.Agregar(writeup);
        var casoUso = new ActualizarWriteupCasoUso(writeups, usuarios, unitOfWork);

        var resultado = await casoUso.EjecutarAsync(new ActualizarWriteupSolicitud(
            writeup.Id,
            usuario.Id,
            " Writeup corregido ",
            "Hack The Box",
            "https://example.local/writeup",
            new DateOnly(2026, 8, 27),
            EstadoMadurez.ListoPortafolio), CancellationToken);

        Assert.Equal(ActualizarWriteupEstado.Actualizado, resultado.Estado);
        Assert.Equal("Writeup corregido", writeup.Titulo);
        Assert.Equal("Hack The Box", writeup.PlataformaOrigen);
        Assert.Equal("https://example.local/writeup", writeup.Url);
        Assert.Equal(new DateOnly(2026, 8, 27), writeup.Fecha);
        Assert.Equal(EstadoMadurez.ListoPortafolio, writeup.EstadoMadurez);
        Assert.Equal(1, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task ActualizarWriteup_DebeRetornarNoEncontradoSinGuardar()
    {
        var usuarios = new FakeUsuarioRepository();
        var unitOfWork = new FakeUnitOfWork();
        var usuario = Usuario.Registrar("Usuario Writeup", EmailUnico());
        usuarios.Agregar(usuario);
        var casoUso = new ActualizarWriteupCasoUso(new FakeWriteupRepository(), usuarios, unitOfWork);

        var resultado = await casoUso.EjecutarAsync(new ActualizarWriteupSolicitud(
            Guid.CreateVersion7(),
            usuario.Id,
            "Writeup",
            null,
            null,
            null,
            EstadoMadurez.Borrador), CancellationToken);

        Assert.Equal(ActualizarWriteupEstado.WriteupNoEncontrado, resultado.Estado);
        Assert.Equal(0, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task ActualizarWriteup_DebeRetornarUsuarioNoEncontradoSinGuardar()
    {
        var unitOfWork = new FakeUnitOfWork();
        var casoUso = new ActualizarWriteupCasoUso(new FakeWriteupRepository(), new FakeUsuarioRepository(), unitOfWork);

        var resultado = await casoUso.EjecutarAsync(new ActualizarWriteupSolicitud(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            "Writeup",
            null,
            null,
            null,
            EstadoMadurez.Borrador), CancellationToken);

        Assert.Equal(ActualizarWriteupEstado.UsuarioNoEncontrado, resultado.Estado);
        Assert.Equal(0, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task ActualizarWriteup_DebeRetornarUsuarioNoCoincideSinGuardar()
    {
        var writeups = new FakeWriteupRepository();
        var usuarios = new FakeUsuarioRepository();
        var unitOfWork = new FakeUnitOfWork();
        var usuarioA = Usuario.Registrar("Usuario A", EmailUnico());
        var usuarioB = Usuario.Registrar("Usuario B", EmailUnico());
        var writeup = CrearWriteup(usuarioA.Id);
        usuarios.Agregar(usuarioA);
        usuarios.Agregar(usuarioB);
        writeups.Agregar(writeup);
        var casoUso = new ActualizarWriteupCasoUso(writeups, usuarios, unitOfWork);

        var resultado = await casoUso.EjecutarAsync(new ActualizarWriteupSolicitud(
            writeup.Id,
            usuarioB.Id,
            "No persistir",
            null,
            null,
            null,
            EstadoMadurez.Publicado), CancellationToken);

        Assert.Equal(ActualizarWriteupEstado.UsuarioNoCoincide, resultado.Estado);
        Assert.Equal("Análisis del modelo OSI con Wireshark", writeup.Titulo);
        Assert.Equal(EstadoMadurez.Borrador, writeup.EstadoMadurez);
        Assert.Equal(0, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task EliminarWriteup_DebeMarcarComoEliminadoYGuardar()
    {
        var writeups = new FakeWriteupRepository();
        var usuarios = new FakeUsuarioRepository();
        var unitOfWork = new FakeUnitOfWork();
        var usuario = Usuario.Registrar("Usuario Writeup", EmailUnico());
        var writeup = CrearWriteup(usuario.Id);
        usuarios.Agregar(usuario);
        writeups.Agregar(writeup);
        var casoUso = new EliminarWriteupCasoUso(writeups, usuarios, unitOfWork);

        var resultado = await casoUso.EjecutarAsync(
            new EliminarWriteupSolicitud(writeup.Id, usuario.Id),
            CancellationToken);

        Assert.Equal(EliminarWriteupEstado.Eliminado, resultado.Estado);
        Assert.NotNull(writeup.FechaEliminacionUtc);
        Assert.Equal(1, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task EliminarWriteup_DebeRetornarWriteupNoEncontradoSinGuardar()
    {
        var usuarios = new FakeUsuarioRepository();
        var unitOfWork = new FakeUnitOfWork();
        var usuario = Usuario.Registrar("Usuario Writeup", EmailUnico());
        usuarios.Agregar(usuario);
        var casoUso = new EliminarWriteupCasoUso(new FakeWriteupRepository(), usuarios, unitOfWork);

        var resultado = await casoUso.EjecutarAsync(
            new EliminarWriteupSolicitud(Guid.CreateVersion7(), usuario.Id),
            CancellationToken);

        Assert.Equal(EliminarWriteupEstado.WriteupNoEncontrado, resultado.Estado);
        Assert.Equal(0, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task EliminarWriteup_DebeRetornarUsuarioNoCoincideSinGuardar()
    {
        var writeups = new FakeWriteupRepository();
        var usuarios = new FakeUsuarioRepository();
        var unitOfWork = new FakeUnitOfWork();
        var usuarioA = Usuario.Registrar("Usuario A", EmailUnico());
        var usuarioB = Usuario.Registrar("Usuario B", EmailUnico());
        var writeup = CrearWriteup(usuarioA.Id);
        usuarios.Agregar(usuarioA);
        usuarios.Agregar(usuarioB);
        writeups.Agregar(writeup);
        var casoUso = new EliminarWriteupCasoUso(writeups, usuarios, unitOfWork);

        var resultado = await casoUso.EjecutarAsync(
            new EliminarWriteupSolicitud(writeup.Id, usuarioB.Id),
            CancellationToken);

        Assert.Equal(EliminarWriteupEstado.UsuarioNoCoincide, resultado.Estado);
        Assert.Null(writeup.FechaEliminacionUtc);
        Assert.Equal(0, unitOfWork.GuardarCambiosLlamadas);
    }

    private static Writeup CrearWriteup(Guid? usuarioId = null) =>
        Writeup.Crear(usuarioId ?? Guid.CreateVersion7(), "Análisis del modelo OSI con Wireshark");

    private static string EmailUnico() => $"writeup-editable-{Guid.CreateVersion7():N}@local.test";

    private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;
}
