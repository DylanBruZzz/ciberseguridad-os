using Aprendizaje.Aplicacion.Study.SesionesEstudio.ActualizarSesionEstudio;
using Aprendizaje.Aplicacion.Study.SesionesEstudio.CorregirDuracionSesionEstudio;
using Aprendizaje.Aplicacion.Study.SesionesEstudio.EliminarSesionEstudio;
using Aprendizaje.Aplicacion.Study.SesionesEstudio.ListarSesionesEstudio;
using Aprendizaje.Aplicacion.Study.SesionesEstudio.ObtenerSesionEstudioPorId;
using Aprendizaje.Aplicacion.Study.SesionesEstudio.RegistrarSesionEstudio;
using Aprendizaje.Dominio.Nucleo;
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

    [Fact]
    public async Task ActualizarSesionEstudio_DebeRetornarUsuarioNoEncontradoSinGuardar()
    {
        var sesiones = new FakeSesionEstudioRepository();
        var temas = new FakeTemaRepository();
        var usuarios = new FakeUsuarioRepository();
        var unitOfWork = new FakeUnitOfWork();
        var casoUso = new ActualizarSesionEstudioCasoUso(sesiones, temas, usuarios, unitOfWork);

        var resultado = await casoUso.EjecutarAsync(
            SolicitudActualizar(Guid.CreateVersion7(), Guid.CreateVersion7(), Guid.CreateVersion7()),
            CancellationToken);

        Assert.Equal(ActualizarSesionEstudioEstado.UsuarioNoEncontrado, resultado.Estado);
        Assert.Equal(0, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task ActualizarSesionEstudio_DebeRetornarSesionNoEncontradaSinGuardar()
    {
        var sesiones = new FakeSesionEstudioRepository();
        var temas = new FakeTemaRepository();
        var usuarios = new FakeUsuarioRepository();
        var unitOfWork = new FakeUnitOfWork();
        var usuario = Usuario.Registrar("Dylan", EmailUnico());
        usuarios.Agregar(usuario);
        var casoUso = new ActualizarSesionEstudioCasoUso(sesiones, temas, usuarios, unitOfWork);

        var resultado = await casoUso.EjecutarAsync(
            SolicitudActualizar(Guid.CreateVersion7(), usuario.Id, Guid.CreateVersion7()),
            CancellationToken);

        Assert.Equal(ActualizarSesionEstudioEstado.SesionNoEncontrada, resultado.Estado);
        Assert.Equal(0, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task ActualizarSesionEstudio_DebeRetornarUsuarioNoCoincideSiSesionEsDeOtroUsuario()
    {
        var sesiones = new FakeSesionEstudioRepository();
        var temas = new FakeTemaRepository();
        var usuarios = new FakeUsuarioRepository();
        var unitOfWork = new FakeUnitOfWork();
        var usuarioA = Usuario.Registrar("Usuario A", EmailUnico());
        var usuarioB = Usuario.Registrar("Usuario B", EmailUnico());
        var sesion = CrearSesion(usuarioA.Id);
        usuarios.Agregar(usuarioA);
        usuarios.Agregar(usuarioB);
        sesiones.Agregar(sesion);
        var casoUso = new ActualizarSesionEstudioCasoUso(sesiones, temas, usuarios, unitOfWork);

        var resultado = await casoUso.EjecutarAsync(
            SolicitudActualizar(sesion.Id, usuarioB.Id, Guid.CreateVersion7()),
            CancellationToken);

        Assert.Equal(ActualizarSesionEstudioEstado.UsuarioNoCoincide, resultado.Estado);
        Assert.Equal(new DateOnly(2026, 8, 20), sesion.Fecha);
        Assert.Equal(0, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task ActualizarSesionEstudio_DebeRetornarTemaNoEncontradoSinGuardar()
    {
        var sesiones = new FakeSesionEstudioRepository();
        var temas = new FakeTemaRepository();
        var usuarios = new FakeUsuarioRepository();
        var unitOfWork = new FakeUnitOfWork();
        var usuario = Usuario.Registrar("Dylan", EmailUnico());
        var sesion = CrearSesion(usuario.Id);
        usuarios.Agregar(usuario);
        sesiones.Agregar(sesion);
        var casoUso = new ActualizarSesionEstudioCasoUso(sesiones, temas, usuarios, unitOfWork);

        var resultado = await casoUso.EjecutarAsync(
            SolicitudActualizar(sesion.Id, usuario.Id, Guid.CreateVersion7()),
            CancellationToken);

        Assert.Equal(ActualizarSesionEstudioEstado.TemaNoEncontrado, resultado.Estado);
        Assert.Equal(0, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task ActualizarSesionEstudio_DebeRetornarUsuarioNoCoincideSiTemaEsDeOtroUsuario()
    {
        var sesiones = new FakeSesionEstudioRepository();
        var temas = new FakeTemaRepository();
        var usuarios = new FakeUsuarioRepository();
        var unitOfWork = new FakeUnitOfWork();
        var usuarioA = Usuario.Registrar("Usuario A", EmailUnico());
        var usuarioB = Usuario.Registrar("Usuario B", EmailUnico());
        var sesion = CrearSesion(usuarioA.Id);
        var temaB = CrearTema(usuarioB.Id);
        usuarios.Agregar(usuarioA);
        usuarios.Agregar(usuarioB);
        sesiones.Agregar(sesion);
        temas.Agregar(temaB);
        var casoUso = new ActualizarSesionEstudioCasoUso(sesiones, temas, usuarios, unitOfWork);

        var resultado = await casoUso.EjecutarAsync(
            SolicitudActualizar(sesion.Id, usuarioA.Id, temaB.Id),
            CancellationToken);

        Assert.Equal(ActualizarSesionEstudioEstado.UsuarioNoCoincide, resultado.Estado);
        Assert.NotEqual(temaB.Id, sesion.TemaId);
        Assert.Equal(0, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task ActualizarSesionEstudio_DebeActualizarCamposYGuardar()
    {
        var sesiones = new FakeSesionEstudioRepository();
        var temas = new FakeTemaRepository();
        var usuarios = new FakeUsuarioRepository();
        var unitOfWork = new FakeUnitOfWork();
        var usuario = Usuario.Registrar("Dylan", EmailUnico());
        var tema = CrearTema(usuario.Id);
        var sesion = CrearSesion(usuario.Id);
        usuarios.Agregar(usuario);
        temas.Agregar(tema);
        sesiones.Agregar(sesion);
        var casoUso = new ActualizarSesionEstudioCasoUso(sesiones, temas, usuarios, unitOfWork);

        var resultado = await casoUso.EjecutarAsync(
            new ActualizarSesionEstudioSolicitud(
                sesion.Id,
                usuario.Id,
                tema.Id,
                new DateOnly(2026, 9, 1),
                90,
                TipoSesion.Laboratorio,
                "Notas corregidas"),
            CancellationToken);

        Assert.Equal(ActualizarSesionEstudioEstado.Actualizada, resultado.Estado);
        Assert.Equal(tema.Id, sesion.TemaId);
        Assert.Equal(new DateOnly(2026, 9, 1), sesion.Fecha);
        Assert.Equal(90, sesion.DuracionMinutos);
        Assert.Equal(TipoSesion.Laboratorio, sesion.Tipo);
        Assert.Equal("Notas corregidas", sesion.Notas);
        Assert.Equal(1, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task ActualizarSesionEstudio_DebePermitirNotasNull()
    {
        var sesiones = new FakeSesionEstudioRepository();
        var temas = new FakeTemaRepository();
        var usuarios = new FakeUsuarioRepository();
        var unitOfWork = new FakeUnitOfWork();
        var usuario = Usuario.Registrar("Dylan", EmailUnico());
        var tema = CrearTema(usuario.Id);
        var sesion = CrearSesion(usuario.Id);
        usuarios.Agregar(usuario);
        temas.Agregar(tema);
        sesiones.Agregar(sesion);
        var casoUso = new ActualizarSesionEstudioCasoUso(sesiones, temas, usuarios, unitOfWork);

        var resultado = await casoUso.EjecutarAsync(
            new ActualizarSesionEstudioSolicitud(
                sesion.Id,
                usuario.Id,
                tema.Id,
                new DateOnly(2026, 9, 1),
                60,
                TipoSesion.Repaso,
                null),
            CancellationToken);

        Assert.Equal(ActualizarSesionEstudioEstado.Actualizada, resultado.Estado);
        Assert.Null(sesion.Notas);
        Assert.Equal(1, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task ActualizarSesionEstudio_DebePropagarErrorDeDominioSinGuardar()
    {
        var sesiones = new FakeSesionEstudioRepository();
        var temas = new FakeTemaRepository();
        var usuarios = new FakeUsuarioRepository();
        var unitOfWork = new FakeUnitOfWork();
        var usuario = Usuario.Registrar("Dylan", EmailUnico());
        var tema = CrearTema(usuario.Id);
        var sesion = CrearSesion(usuario.Id);
        usuarios.Agregar(usuario);
        temas.Agregar(tema);
        sesiones.Agregar(sesion);
        var casoUso = new ActualizarSesionEstudioCasoUso(sesiones, temas, usuarios, unitOfWork);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            casoUso.EjecutarAsync(
                SolicitudActualizar(sesion.Id, usuario.Id, tema.Id, duracionMinutos: 0),
                CancellationToken));

        Assert.Equal(0, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task ActualizarSesionEstudio_DebeRechazarIdsVacios()
    {
        var casoUso = new ActualizarSesionEstudioCasoUso(
            new FakeSesionEstudioRepository(),
            new FakeTemaRepository(),
            new FakeUsuarioRepository(),
            new FakeUnitOfWork());

        await Assert.ThrowsAsync<ArgumentException>(() => casoUso.EjecutarAsync(
            SolicitudActualizar(Guid.Empty, Guid.CreateVersion7(), Guid.CreateVersion7()),
            CancellationToken));
        await Assert.ThrowsAsync<ArgumentException>(() => casoUso.EjecutarAsync(
            SolicitudActualizar(Guid.CreateVersion7(), Guid.Empty, Guid.CreateVersion7()),
            CancellationToken));
        await Assert.ThrowsAsync<ArgumentException>(() => casoUso.EjecutarAsync(
            SolicitudActualizar(Guid.CreateVersion7(), Guid.CreateVersion7(), Guid.Empty),
            CancellationToken));
    }

    [Fact]
    public async Task EliminarSesionEstudio_DebeRetornarUsuarioNoEncontradoSinGuardar()
    {
        var unitOfWork = new FakeUnitOfWork();
        var casoUso = new EliminarSesionEstudioCasoUso(
            new FakeSesionEstudioRepository(),
            new FakeUsuarioRepository(),
            unitOfWork);

        var resultado = await casoUso.EjecutarAsync(
            new EliminarSesionEstudioSolicitud(Guid.CreateVersion7(), Guid.CreateVersion7()),
            CancellationToken);

        Assert.Equal(EliminarSesionEstudioEstado.UsuarioNoEncontrado, resultado.Estado);
        Assert.Equal(0, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task EliminarSesionEstudio_DebeRetornarSesionNoEncontradaSinGuardar()
    {
        var sesiones = new FakeSesionEstudioRepository();
        var usuarios = new FakeUsuarioRepository();
        var unitOfWork = new FakeUnitOfWork();
        var usuario = Usuario.Registrar("Dylan", EmailUnico());
        usuarios.Agregar(usuario);
        var casoUso = new EliminarSesionEstudioCasoUso(sesiones, usuarios, unitOfWork);

        var resultado = await casoUso.EjecutarAsync(
            new EliminarSesionEstudioSolicitud(Guid.CreateVersion7(), usuario.Id),
            CancellationToken);

        Assert.Equal(EliminarSesionEstudioEstado.SesionNoEncontrada, resultado.Estado);
        Assert.Equal(0, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task EliminarSesionEstudio_DebeRetornarUsuarioNoCoincideSinGuardar()
    {
        var sesiones = new FakeSesionEstudioRepository();
        var usuarios = new FakeUsuarioRepository();
        var unitOfWork = new FakeUnitOfWork();
        var usuarioA = Usuario.Registrar("Usuario A", EmailUnico());
        var usuarioB = Usuario.Registrar("Usuario B", EmailUnico());
        var sesion = CrearSesion(usuarioA.Id);
        usuarios.Agregar(usuarioA);
        usuarios.Agregar(usuarioB);
        sesiones.Agregar(sesion);
        var casoUso = new EliminarSesionEstudioCasoUso(sesiones, usuarios, unitOfWork);

        var resultado = await casoUso.EjecutarAsync(
            new EliminarSesionEstudioSolicitud(sesion.Id, usuarioB.Id),
            CancellationToken);

        Assert.Equal(EliminarSesionEstudioEstado.UsuarioNoCoincide, resultado.Estado);
        Assert.Null(sesion.FechaEliminacionUtc);
        Assert.Equal(0, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task EliminarSesionEstudio_DebeMarcarEliminadaYGuardar()
    {
        var sesiones = new FakeSesionEstudioRepository();
        var usuarios = new FakeUsuarioRepository();
        var unitOfWork = new FakeUnitOfWork();
        var usuario = Usuario.Registrar("Dylan", EmailUnico());
        var sesion = CrearSesion(usuario.Id);
        usuarios.Agregar(usuario);
        sesiones.Agregar(sesion);
        var casoUso = new EliminarSesionEstudioCasoUso(sesiones, usuarios, unitOfWork);

        var resultado = await casoUso.EjecutarAsync(
            new EliminarSesionEstudioSolicitud(sesion.Id, usuario.Id),
            CancellationToken);

        Assert.Equal(EliminarSesionEstudioEstado.Eliminada, resultado.Estado);
        Assert.NotNull(sesion.FechaEliminacionUtc);
        Assert.Equal(1, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task EliminarSesionEstudio_DebeRechazarIdsVacios()
    {
        var casoUso = new EliminarSesionEstudioCasoUso(
            new FakeSesionEstudioRepository(),
            new FakeUsuarioRepository(),
            new FakeUnitOfWork());

        await Assert.ThrowsAsync<ArgumentException>(() => casoUso.EjecutarAsync(
            new EliminarSesionEstudioSolicitud(Guid.Empty, Guid.CreateVersion7()),
            CancellationToken));
        await Assert.ThrowsAsync<ArgumentException>(() => casoUso.EjecutarAsync(
            new EliminarSesionEstudioSolicitud(Guid.CreateVersion7(), Guid.Empty),
            CancellationToken));
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

    private static ActualizarSesionEstudioSolicitud SolicitudActualizar(
        Guid sesionId,
        Guid usuarioId,
        Guid temaId,
        int duracionMinutos = 60) =>
        new(
            sesionId,
            usuarioId,
            temaId,
            new DateOnly(2026, 9, 1),
            duracionMinutos,
            TipoSesion.Practica,
            "Notas corregidas");

    private static string EmailUnico() => $"study-{Guid.CreateVersion7():N}@local.test";

    private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;
}
