using Aprendizaje.Aplicacion.Study.SesionesEstudio.CorregirDuracionSesionEstudio;
using Aprendizaje.Aplicacion.Study.SesionesEstudio.ListarSesionesEstudio;
using Aprendizaje.Aplicacion.Study.SesionesEstudio.ObtenerSesionEstudioPorId;
using Aprendizaje.Aplicacion.Study.SesionesEstudio.RegistrarSesionEstudio;
using Aprendizaje.Dominio.Roadmap;
using Aprendizaje.Dominio.Study;
using Aprendizaje.Tests.Soporte;
using Xunit;

namespace Aprendizaje.Tests.Application.Study;

public sealed class SesionesEstudioApplicationTests
{
    [Fact]
    public async Task RegistrarSesionEstudio_DebeRetornarTemaNoEncontradoSinGuardar()
    {
        var sesiones = new FakeSesionEstudioRepository();
        var temas = new FakeTemaRepository();
        var unitOfWork = new FakeUnitOfWork();
        var casoUso = new RegistrarSesionEstudioCasoUso(sesiones, temas, unitOfWork);

        var resultado = await casoUso.EjecutarAsync(CrearSolicitud(Guid.CreateVersion7()), CancellationToken);

        Assert.Equal(RegistrarSesionEstudioEstado.TemaNoEncontrado, resultado.Estado);
        Assert.Equal(0, sesiones.AgregarLlamadas);
        Assert.Equal(0, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task RegistrarSesionEstudio_DebeRetornarUsuarioNoCoincideSinGuardar()
    {
        var sesiones = new FakeSesionEstudioRepository();
        var temas = new FakeTemaRepository();
        var unitOfWork = new FakeUnitOfWork();
        var tema = CrearTema();
        temas.Agregar(tema);
        var casoUso = new RegistrarSesionEstudioCasoUso(sesiones, temas, unitOfWork);

        var resultado = await casoUso.EjecutarAsync(CrearSolicitud(tema.Id, Guid.CreateVersion7()), CancellationToken);

        Assert.Equal(RegistrarSesionEstudioEstado.UsuarioNoCoincide, resultado.Estado);
        Assert.Equal(0, sesiones.AgregarLlamadas);
        Assert.Equal(0, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task RegistrarSesionEstudio_DebeCrearSesionAgregarYGuardar()
    {
        var sesiones = new FakeSesionEstudioRepository();
        var temas = new FakeTemaRepository();
        var unitOfWork = new FakeUnitOfWork();
        var tema = CrearTema();
        temas.Agregar(tema);
        var casoUso = new RegistrarSesionEstudioCasoUso(sesiones, temas, unitOfWork);

        var resultado = await casoUso.EjecutarAsync(CrearSolicitud(tema.Id, tema.UsuarioId), CancellationToken);

        Assert.Equal(RegistrarSesionEstudioEstado.Creada, resultado.Estado);
        Assert.NotNull(resultado.Id);
        Assert.Equal(tema.UsuarioId, resultado.UsuarioId);
        Assert.Equal(tema.Id, resultado.TemaId);
        Assert.Equal(45, resultado.DuracionMinutos);
        Assert.Equal(TipoSesion.Teoria, resultado.Tipo);
        Assert.Equal("Estudio inicial", resultado.Notas);
        Assert.Equal(1, sesiones.AgregarLlamadas);
        Assert.Equal(1, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task ObtenerSesionEstudioPorId_DebeRetornarSesionExistente()
    {
        var sesiones = new FakeSesionEstudioRepository();
        var sesion = CrearSesion();
        sesiones.Agregar(sesion);
        var casoUso = new ObtenerSesionEstudioPorIdCasoUso(sesiones);

        var resultado = await casoUso.EjecutarAsync(sesion.Id, CancellationToken);

        Assert.True(resultado.Encontrado);
        Assert.NotNull(resultado.Sesion);
        Assert.Equal(sesion.Id, resultado.Sesion.Id);
        Assert.Equal(sesion.DuracionMinutos, resultado.Sesion.DuracionMinutos);
    }

    [Fact]
    public async Task ObtenerSesionEstudioPorId_DebeRetornarNoEncontrada()
    {
        var sesiones = new FakeSesionEstudioRepository();
        var casoUso = new ObtenerSesionEstudioPorIdCasoUso(sesiones);

        var resultado = await casoUso.EjecutarAsync(Guid.CreateVersion7(), CancellationToken);

        Assert.False(resultado.Encontrado);
        Assert.Null(resultado.Sesion);
    }

    [Fact]
    public async Task ListarSesionesEstudio_DebeMapearColeccionDelRepository()
    {
        var sesiones = new FakeSesionEstudioRepository();
        var usuarioId = Guid.CreateVersion7();
        var sesion = CrearSesion(usuarioId);
        sesiones.Agregar(sesion);
        sesiones.Agregar(CrearSesion(Guid.CreateVersion7()));
        var casoUso = new ListarSesionesEstudioCasoUso(sesiones);

        var resultado = await casoUso.EjecutarAsync(new ListarSesionesEstudioSolicitud(usuarioId), CancellationToken);

        var resumen = Assert.Single(resultado.Sesiones);
        Assert.Equal(sesion.Id, resumen.Id);
        Assert.Equal(usuarioId, resumen.UsuarioId);
        Assert.Equal(sesion.TemaId, resumen.TemaId);
    }

    [Fact]
    public async Task CorregirDuracion_DebeRetornarSesionNoEncontradaSinGuardar()
    {
        var sesiones = new FakeSesionEstudioRepository();
        var unitOfWork = new FakeUnitOfWork();
        var casoUso = new CorregirDuracionSesionEstudioCasoUso(sesiones, unitOfWork);

        var resultado = await casoUso.EjecutarAsync(
            new CorregirDuracionSesionEstudioSolicitud(Guid.CreateVersion7(), 60),
            CancellationToken);

        Assert.Equal(CorregirDuracionSesionEstudioEstado.SesionNoEncontrada, resultado.Estado);
        Assert.Equal(0, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task CorregirDuracion_DebeActualizarDuracionYGuardar()
    {
        var sesiones = new FakeSesionEstudioRepository();
        var unitOfWork = new FakeUnitOfWork();
        var sesion = CrearSesion();
        sesiones.Agregar(sesion);
        var casoUso = new CorregirDuracionSesionEstudioCasoUso(sesiones, unitOfWork);

        var resultado = await casoUso.EjecutarAsync(
            new CorregirDuracionSesionEstudioSolicitud(sesion.Id, 60),
            CancellationToken);

        Assert.Equal(CorregirDuracionSesionEstudioEstado.Actualizada, resultado.Estado);
        Assert.Equal(60, sesion.DuracionMinutos);
        Assert.Equal(1, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task CorregirDuracion_DebePropagarDuracionInvalidaSinGuardar()
    {
        var sesiones = new FakeSesionEstudioRepository();
        var unitOfWork = new FakeUnitOfWork();
        var sesion = CrearSesion();
        sesiones.Agregar(sesion);
        var casoUso = new CorregirDuracionSesionEstudioCasoUso(sesiones, unitOfWork);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            casoUso.EjecutarAsync(
                new CorregirDuracionSesionEstudioSolicitud(sesion.Id, 0),
                CancellationToken));

        Assert.Equal(0, unitOfWork.GuardarCambiosLlamadas);
    }

    private static Tema CrearTema(Guid? usuarioId = null) =>
        Tema.Crear(usuarioId ?? Guid.CreateVersion7(), "Modelo OSI", TipoConocimiento.Conceptual);

    private static RegistrarSesionEstudioSolicitud CrearSolicitud(Guid temaId, Guid? usuarioId = null) =>
        new(
            usuarioId ?? Guid.CreateVersion7(),
            temaId,
            new DateOnly(2026, 8, 20),
            45,
            TipoSesion.Teoria,
            "Estudio inicial");

    private static SesionEstudio CrearSesion(Guid? usuarioId = null) =>
        SesionEstudio.Registrar(
            usuarioId ?? Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            new DateOnly(2026, 8, 20),
            45,
            TipoSesion.Teoria,
            "Estudio inicial");

    private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;
}
