using Aprendizaje.Aplicacion.Roadmap.Temas.DesvincularHerramientaDeTema;
using Aprendizaje.Aplicacion.Roadmap.Temas.VincularHerramientaATema;
using Aprendizaje.Dominio.Roadmap;
using Aprendizaje.Dominio.Study;
using Aprendizaje.Tests.Soporte;
using Xunit;

namespace Aprendizaje.Tests.Application.Roadmap;

public sealed class TemaHerramientaApplicationTests
{
    [Fact]
    public async Task VincularHerramientaATema_DebeVincularYGuardar()
    {
        var temas = new FakeTemaRepository();
        var herramientas = new FakeHerramientaRepository();
        var unitOfWork = new FakeUnitOfWork();
        var usuarioId = Guid.CreateVersion7();
        var tema = Tema.Crear(usuarioId, "Analisis de trafico", TipoConocimiento.Procedimental);
        var herramienta = Herramienta.Crear("Wireshark");
        temas.Agregar(tema);
        herramientas.Agregar(herramienta);
        var casoUso = new VincularHerramientaATemaCasoUso(temas, herramientas, unitOfWork);

        var resultado = await casoUso.EjecutarAsync(
            new VincularHerramientaATemaSolicitud(usuarioId, tema.Id, herramienta.Id),
            CancellationToken);

        Assert.Equal(VincularHerramientaATemaEstado.Actualizado, resultado.Estado);
        Assert.True(await temas.ExisteVinculoHerramientaAsync(tema.Id, herramienta.Id, CancellationToken));
        Assert.Equal(1, temas.VincularHerramientaLlamadas);
        Assert.Equal(1, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task VincularHerramientaATema_DebeSerIdempotenteSinGuardarDeNuevo()
    {
        var temas = new FakeTemaRepository();
        var herramientas = new FakeHerramientaRepository();
        var unitOfWork = new FakeUnitOfWork();
        var usuarioId = Guid.CreateVersion7();
        var tema = Tema.Crear(usuarioId, "Active Directory", TipoConocimiento.Procedimental);
        var herramienta = Herramienta.Crear("BloodHound");
        temas.Agregar(tema);
        herramientas.Agregar(herramienta);
        var casoUso = new VincularHerramientaATemaCasoUso(temas, herramientas, unitOfWork);
        await casoUso.EjecutarAsync(
            new VincularHerramientaATemaSolicitud(usuarioId, tema.Id, herramienta.Id),
            CancellationToken);

        var resultado = await casoUso.EjecutarAsync(
            new VincularHerramientaATemaSolicitud(usuarioId, tema.Id, herramienta.Id),
            CancellationToken);

        Assert.Equal(VincularHerramientaATemaEstado.Actualizado, resultado.Estado);
        Assert.Equal(1, temas.VincularHerramientaLlamadas);
        Assert.Equal(1, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task VincularHerramientaATema_DebeRetornarNoEncontradoSinGuardar()
    {
        var temas = new FakeTemaRepository();
        var herramientas = new FakeHerramientaRepository();
        var unitOfWork = new FakeUnitOfWork();
        var usuarioId = Guid.CreateVersion7();
        var tema = Tema.Crear(usuarioId, "Tema", TipoConocimiento.Conceptual);
        var herramienta = Herramienta.Crear("nmap");
        temas.Agregar(tema);
        herramientas.Agregar(herramienta);
        var casoUso = new VincularHerramientaATemaCasoUso(temas, herramientas, unitOfWork);

        var temaInexistente = await casoUso.EjecutarAsync(
            new VincularHerramientaATemaSolicitud(usuarioId, Guid.CreateVersion7(), herramienta.Id),
            CancellationToken);
        var herramientaInexistente = await casoUso.EjecutarAsync(
            new VincularHerramientaATemaSolicitud(usuarioId, tema.Id, Guid.CreateVersion7()),
            CancellationToken);

        Assert.Equal(VincularHerramientaATemaEstado.TemaNoEncontrado, temaInexistente.Estado);
        Assert.Equal(VincularHerramientaATemaEstado.HerramientaNoEncontrada, herramientaInexistente.Estado);
        Assert.Equal(0, temas.VincularHerramientaLlamadas);
        Assert.Equal(0, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task VincularHerramientaATema_DebeProtegerOwnershipDelTema()
    {
        var temas = new FakeTemaRepository();
        var herramientas = new FakeHerramientaRepository();
        var unitOfWork = new FakeUnitOfWork();
        var tema = Tema.Crear(Guid.CreateVersion7(), "Tema ajeno", TipoConocimiento.Conceptual);
        var herramienta = Herramienta.Crear("PowerShell");
        temas.Agregar(tema);
        herramientas.Agregar(herramienta);
        var casoUso = new VincularHerramientaATemaCasoUso(temas, herramientas, unitOfWork);

        var resultado = await casoUso.EjecutarAsync(
            new VincularHerramientaATemaSolicitud(Guid.CreateVersion7(), tema.Id, herramienta.Id),
            CancellationToken);

        Assert.Equal(VincularHerramientaATemaEstado.UsuarioNoCoincide, resultado.Estado);
        Assert.Equal(0, temas.VincularHerramientaLlamadas);
        Assert.Equal(0, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task DesvincularHerramientaDeTema_DebeEliminarVinculoYGuardar()
    {
        var temas = new FakeTemaRepository();
        var herramientas = new FakeHerramientaRepository();
        var unitOfWork = new FakeUnitOfWork();
        var usuarioId = Guid.CreateVersion7();
        var tema = Tema.Crear(usuarioId, "Forense", TipoConocimiento.Procedimental);
        var herramienta = Herramienta.Crear("Autopsy");
        temas.Agregar(tema);
        herramientas.Agregar(herramienta);
        temas.VincularHerramienta(tema.Id, herramienta.Id);
        var casoUso = new DesvincularHerramientaDeTemaCasoUso(temas, herramientas, unitOfWork);

        var resultado = await casoUso.EjecutarAsync(
            new DesvincularHerramientaDeTemaSolicitud(usuarioId, tema.Id, herramienta.Id),
            CancellationToken);

        Assert.Equal(DesvincularHerramientaDeTemaEstado.Actualizado, resultado.Estado);
        Assert.False(await temas.ExisteVinculoHerramientaAsync(tema.Id, herramienta.Id, CancellationToken));
        Assert.Equal(1, temas.DesvincularHerramientaLlamadas);
        Assert.Equal(1, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task DesvincularHerramientaDeTema_DebeSerIdempotenteSinGuardarSiNoExisteVinculo()
    {
        var temas = new FakeTemaRepository();
        var herramientas = new FakeHerramientaRepository();
        var unitOfWork = new FakeUnitOfWork();
        var usuarioId = Guid.CreateVersion7();
        var tema = Tema.Crear(usuarioId, "Cloud", TipoConocimiento.Conceptual);
        var herramienta = Herramienta.Crear("AWS CLI");
        temas.Agregar(tema);
        herramientas.Agregar(herramienta);
        var casoUso = new DesvincularHerramientaDeTemaCasoUso(temas, herramientas, unitOfWork);

        var resultado = await casoUso.EjecutarAsync(
            new DesvincularHerramientaDeTemaSolicitud(usuarioId, tema.Id, herramienta.Id),
            CancellationToken);

        Assert.Equal(DesvincularHerramientaDeTemaEstado.Actualizado, resultado.Estado);
        Assert.Equal(1, temas.DesvincularHerramientaLlamadas);
        Assert.Equal(0, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task DesvincularHerramientaDeTema_DebeRetornarNoEncontradoYProtegerOwnership()
    {
        var temas = new FakeTemaRepository();
        var herramientas = new FakeHerramientaRepository();
        var unitOfWork = new FakeUnitOfWork();
        var usuarioId = Guid.CreateVersion7();
        var tema = Tema.Crear(usuarioId, "Tema", TipoConocimiento.Conceptual);
        var herramienta = Herramienta.Crear("tcpdump");
        temas.Agregar(tema);
        herramientas.Agregar(herramienta);
        var casoUso = new DesvincularHerramientaDeTemaCasoUso(temas, herramientas, unitOfWork);

        var temaInexistente = await casoUso.EjecutarAsync(
            new DesvincularHerramientaDeTemaSolicitud(usuarioId, Guid.CreateVersion7(), herramienta.Id),
            CancellationToken);
        var herramientaInexistente = await casoUso.EjecutarAsync(
            new DesvincularHerramientaDeTemaSolicitud(usuarioId, tema.Id, Guid.CreateVersion7()),
            CancellationToken);
        var ownership = await casoUso.EjecutarAsync(
            new DesvincularHerramientaDeTemaSolicitud(Guid.CreateVersion7(), tema.Id, herramienta.Id),
            CancellationToken);

        Assert.Equal(DesvincularHerramientaDeTemaEstado.TemaNoEncontrado, temaInexistente.Estado);
        Assert.Equal(DesvincularHerramientaDeTemaEstado.HerramientaNoEncontrada, herramientaInexistente.Estado);
        Assert.Equal(DesvincularHerramientaDeTemaEstado.UsuarioNoCoincide, ownership.Estado);
        Assert.Equal(0, unitOfWork.GuardarCambiosLlamadas);
    }

    private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;
}
