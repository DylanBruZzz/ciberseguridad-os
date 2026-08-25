using Aprendizaje.Aplicacion.Study.SesionesEstudio.VincularHerramientaASesionEstudio;
using Aprendizaje.Dominio.Study;
using Aprendizaje.Tests.Soporte;
using Xunit;

namespace Aprendizaje.Tests.Application.Study;

public sealed class SesionHerramientaApplicationTests
{
    [Fact]
    public async Task VincularHerramientaASesionEstudio_DebeRetornarSesionNoEncontradaSinGuardar()
    {
        var sesiones = new FakeSesionEstudioRepository();
        var herramientas = new FakeHerramientaRepository();
        var unitOfWork = new FakeUnitOfWork();
        var casoUso = new VincularHerramientaASesionEstudioCasoUso(sesiones, herramientas, unitOfWork);

        var resultado = await casoUso.EjecutarAsync(
            new VincularHerramientaASesionEstudioSolicitud(Guid.CreateVersion7(), Guid.CreateVersion7()),
            CancellationToken);

        Assert.Equal(VincularHerramientaASesionEstudioEstado.SesionNoEncontrada, resultado.Estado);
        Assert.Equal(0, sesiones.VincularHerramientaLlamadas);
        Assert.Equal(0, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task VincularHerramientaASesionEstudio_DebeRetornarHerramientaNoEncontradaSinGuardar()
    {
        var sesiones = new FakeSesionEstudioRepository();
        var herramientas = new FakeHerramientaRepository();
        var unitOfWork = new FakeUnitOfWork();
        var sesion = CrearSesion();
        sesiones.Agregar(sesion);
        var casoUso = new VincularHerramientaASesionEstudioCasoUso(sesiones, herramientas, unitOfWork);

        var resultado = await casoUso.EjecutarAsync(
            new VincularHerramientaASesionEstudioSolicitud(sesion.Id, Guid.CreateVersion7()),
            CancellationToken);

        Assert.Equal(VincularHerramientaASesionEstudioEstado.HerramientaNoEncontrada, resultado.Estado);
        Assert.Equal(0, sesiones.VincularHerramientaLlamadas);
        Assert.Equal(0, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task VincularHerramientaASesionEstudio_DebeVincularYGuardar()
    {
        var sesiones = new FakeSesionEstudioRepository();
        var herramientas = new FakeHerramientaRepository();
        var unitOfWork = new FakeUnitOfWork();
        var sesion = CrearSesion();
        var herramienta = Herramienta.Crear("Wireshark");
        sesiones.Agregar(sesion);
        herramientas.Agregar(herramienta);
        var casoUso = new VincularHerramientaASesionEstudioCasoUso(sesiones, herramientas, unitOfWork);

        var resultado = await casoUso.EjecutarAsync(
            new VincularHerramientaASesionEstudioSolicitud(sesion.Id, herramienta.Id),
            CancellationToken);

        Assert.Equal(VincularHerramientaASesionEstudioEstado.Actualizado, resultado.Estado);
        Assert.True(await sesiones.ExisteVinculoHerramientaAsync(sesion.Id, herramienta.Id, CancellationToken));
        Assert.Equal(1, sesiones.VincularHerramientaLlamadas);
        Assert.Equal(1, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task VincularHerramientaASesionEstudio_DebeSerIdempotenteSinGuardarDeNuevo()
    {
        var sesiones = new FakeSesionEstudioRepository();
        var herramientas = new FakeHerramientaRepository();
        var unitOfWork = new FakeUnitOfWork();
        var sesion = CrearSesion();
        var herramienta = Herramienta.Crear("Wireshark");
        sesiones.Agregar(sesion);
        herramientas.Agregar(herramienta);
        var casoUso = new VincularHerramientaASesionEstudioCasoUso(sesiones, herramientas, unitOfWork);
        await casoUso.EjecutarAsync(
            new VincularHerramientaASesionEstudioSolicitud(sesion.Id, herramienta.Id),
            CancellationToken);

        var resultado = await casoUso.EjecutarAsync(
            new VincularHerramientaASesionEstudioSolicitud(sesion.Id, herramienta.Id),
            CancellationToken);

        Assert.Equal(VincularHerramientaASesionEstudioEstado.Actualizado, resultado.Estado);
        Assert.Equal(1, sesiones.VincularHerramientaLlamadas);
        Assert.Equal(1, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task VincularHerramientaASesionEstudio_DebeRechazarIdsVacios()
    {
        var sesiones = new FakeSesionEstudioRepository();
        var herramientas = new FakeHerramientaRepository();
        var unitOfWork = new FakeUnitOfWork();
        var casoUso = new VincularHerramientaASesionEstudioCasoUso(sesiones, herramientas, unitOfWork);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            casoUso.EjecutarAsync(
                new VincularHerramientaASesionEstudioSolicitud(Guid.Empty, Guid.CreateVersion7()),
                CancellationToken));
        await Assert.ThrowsAsync<ArgumentException>(() =>
            casoUso.EjecutarAsync(
                new VincularHerramientaASesionEstudioSolicitud(Guid.CreateVersion7(), Guid.Empty),
                CancellationToken));

        Assert.Equal(0, unitOfWork.GuardarCambiosLlamadas);
    }

    private static SesionEstudio CrearSesion() =>
        SesionEstudio.Registrar(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            new DateOnly(2026, 8, 20),
            45,
            TipoSesion.Teoria,
            "Estudio inicial");

    private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;
}
