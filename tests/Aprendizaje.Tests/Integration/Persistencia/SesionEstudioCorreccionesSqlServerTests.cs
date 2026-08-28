using Aprendizaje.Aplicacion.Analytics.Estudio;
using Aprendizaje.Aplicacion.Study.SesionesEstudio.ActualizarSesionEstudio;
using Aprendizaje.Aplicacion.Study.SesionesEstudio.EliminarSesionEstudio;
using Aprendizaje.Aplicacion.Study.SesionesEstudio.VincularHerramientaASesionEstudio;
using Aprendizaje.Dominio.Nucleo;
using Aprendizaje.Dominio.Roadmap;
using Aprendizaje.Dominio.Study;
using Aprendizaje.Infraestructura.Persistencia;
using Aprendizaje.Infraestructura.Persistencia.Consultas;
using Aprendizaje.Infraestructura.Persistencia.Repositorios;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Aprendizaje.Tests.Integration.Persistencia;

[Collection(ColeccionPersistenciaSqlServer.Nombre)]
public sealed class SesionEstudioCorreccionesSqlServerTests
{
    [Fact]
    public async Task SesionEstudioCorrections_ActualizacionPersisteYPreservaHerramientas()
    {
        await using var ambiente = await AmbientePersistenciaSqlServer.CrearAsync(CancellationToken);
        var usuario = Usuario.Registrar("Usuario Study", EmailUnico());
        var temaInicial = Tema.Crear(usuario.Id, "Modelo OSI / TCP-IP", TipoConocimiento.Conceptual);
        var temaDestino = Tema.Crear(usuario.Id, "DNS, DHCP, HTTP/S", TipoConocimiento.Conceptual);
        var herramienta = Herramienta.Crear($"Wireshark Study {Guid.CreateVersion7():N}");
        var sesion = SesionEstudio.Registrar(
            usuario.Id,
            temaInicial.Id,
            new DateOnly(2026, 8, 20),
            45,
            TipoSesion.Teoria,
            "Estudio inicial");

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            contexto.Usuarios.Add(usuario);
            contexto.Temas.AddRange(temaInicial, temaDestino);
            contexto.Herramientas.Add(herramienta);
            contexto.SesionesEstudio.Add(sesion);
            await contexto.GuardarCambiosAsync(CancellationToken);
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            await new VincularHerramientaASesionEstudioCasoUso(
                    new SesionEstudioRepository(contexto),
                    new HerramientaRepository(contexto),
                    contexto)
                .EjecutarAsync(new VincularHerramientaASesionEstudioSolicitud(sesion.Id, herramienta.Id), CancellationToken);

            var resultado = await CrearCasoActualizar(contexto).EjecutarAsync(
                new ActualizarSesionEstudioSolicitud(
                    sesion.Id,
                    usuario.Id,
                    temaDestino.Id,
                    new DateOnly(2026, 9, 1),
                    90,
                    TipoSesion.Laboratorio,
                    "Notas corregidas"),
                CancellationToken);

            Assert.Equal(ActualizarSesionEstudioEstado.Actualizada, resultado.Estado);
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            var persistida = await contexto.SesionesEstudio
                .AsNoTracking()
                .SingleAsync(s => s.Id == sesion.Id, CancellationToken);
            var vinculosHerramienta = await ContarVinculosHerramientaAsync(contexto, sesion.Id, herramienta.Id);

            Assert.Equal(usuario.Id, persistida.UsuarioId);
            Assert.Equal(temaDestino.Id, persistida.TemaId);
            Assert.Equal(new DateOnly(2026, 9, 1), persistida.Fecha);
            Assert.Equal(90, persistida.DuracionMinutos);
            Assert.Equal(TipoSesion.Laboratorio, persistida.Tipo);
            Assert.Equal("Notas corregidas", persistida.Notas);
            Assert.Equal(1, vinculosHerramienta);
        }
    }

    [Fact]
    public async Task SesionEstudioCorrections_OwnershipIncorrectoNoActualizaNiCambiaTema()
    {
        await using var ambiente = await AmbientePersistenciaSqlServer.CrearAsync(CancellationToken);
        var usuarioA = Usuario.Registrar("Usuario A", EmailUnico());
        var usuarioB = Usuario.Registrar("Usuario B", EmailUnico());
        var temaA = Tema.Crear(usuarioA.Id, "Modelo OSI", TipoConocimiento.Conceptual);
        var temaB = Tema.Crear(usuarioB.Id, "Splunk / Wazuh", TipoConocimiento.Herramienta);
        var sesion = SesionEstudio.Registrar(
            usuarioA.Id,
            temaA.Id,
            new DateOnly(2026, 8, 20),
            45,
            TipoSesion.Teoria,
            "Estudio inicial");

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            contexto.Usuarios.AddRange(usuarioA, usuarioB);
            contexto.Temas.AddRange(temaA, temaB);
            contexto.SesionesEstudio.Add(sesion);
            await contexto.GuardarCambiosAsync(CancellationToken);
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            var resultadoSesionDeOtroUsuario = await CrearCasoActualizar(contexto).EjecutarAsync(
                new ActualizarSesionEstudioSolicitud(
                    sesion.Id,
                    usuarioB.Id,
                    temaB.Id,
                    new DateOnly(2026, 9, 1),
                    90,
                    TipoSesion.Laboratorio,
                    "No debe persistir"),
                CancellationToken);

            var resultadoTemaDeOtroUsuario = await CrearCasoActualizar(contexto).EjecutarAsync(
                new ActualizarSesionEstudioSolicitud(
                    sesion.Id,
                    usuarioA.Id,
                    temaB.Id,
                    new DateOnly(2026, 9, 1),
                    90,
                    TipoSesion.Laboratorio,
                    "No debe persistir"),
                CancellationToken);

            Assert.Equal(ActualizarSesionEstudioEstado.UsuarioNoCoincide, resultadoSesionDeOtroUsuario.Estado);
            Assert.Equal(ActualizarSesionEstudioEstado.UsuarioNoCoincide, resultadoTemaDeOtroUsuario.Estado);
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            var persistida = await contexto.SesionesEstudio
                .AsNoTracking()
                .SingleAsync(s => s.Id == sesion.Id, CancellationToken);

            Assert.Equal(temaA.Id, persistida.TemaId);
            Assert.Equal(new DateOnly(2026, 8, 20), persistida.Fecha);
            Assert.Equal(45, persistida.DuracionMinutos);
            Assert.Equal(TipoSesion.Teoria, persistida.Tipo);
            Assert.Equal("Estudio inicial", persistida.Notas);
            Assert.Null(persistida.FechaEliminacionUtc);
        }
    }

    [Fact]
    public async Task SesionEstudioCorrections_SoftDeleteOcultaConsultaNormalPreservaFilaYNoCuentaEnAnalytics()
    {
        await using var ambiente = await AmbientePersistenciaSqlServer.CrearAsync(CancellationToken);
        var usuario = Usuario.Registrar("Usuario Study", EmailUnico());
        var tema = Tema.Crear(usuario.Id, "Modelo OSI / TCP-IP", TipoConocimiento.Conceptual);
        var herramienta = Herramienta.Crear($"Nmap Study {Guid.CreateVersion7():N}");
        var sesion = SesionEstudio.Registrar(
            usuario.Id,
            tema.Id,
            new DateOnly(2026, 8, 20),
            45,
            TipoSesion.Teoria,
            "Estudio a eliminar");

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            contexto.Usuarios.Add(usuario);
            contexto.Temas.Add(tema);
            contexto.Herramientas.Add(herramienta);
            contexto.SesionesEstudio.Add(sesion);
            await contexto.GuardarCambiosAsync(CancellationToken);
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            await new VincularHerramientaASesionEstudioCasoUso(
                    new SesionEstudioRepository(contexto),
                    new HerramientaRepository(contexto),
                    contexto)
                .EjecutarAsync(new VincularHerramientaASesionEstudioSolicitud(sesion.Id, herramienta.Id), CancellationToken);

            var resultado = await CrearCasoEliminar(contexto).EjecutarAsync(
                new EliminarSesionEstudioSolicitud(sesion.Id, usuario.Id),
                CancellationToken);

            Assert.Equal(EliminarSesionEstudioEstado.Eliminada, resultado.Estado);
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            var consultaNormal = await contexto.SesionesEstudio
                .SingleOrDefaultAsync(s => s.Id == sesion.Id, CancellationToken);
            var consultaSinFiltro = await contexto.SesionesEstudio
                .IgnoreQueryFilters()
                .SingleOrDefaultAsync(s => s.Id == sesion.Id, CancellationToken);
            var vinculosHerramienta = await ContarVinculosHerramientaAsync(contexto, sesion.Id, herramienta.Id);
            var resumen = await new ConsultaResumenEstudio(contexto).ObtenerAsync(usuario.Id, CancellationToken);

            Assert.Null(consultaNormal);
            Assert.NotNull(consultaSinFiltro);
            Assert.NotNull(consultaSinFiltro.FechaEliminacionUtc);
            Assert.Equal(1, vinculosHerramienta);
            Assert.Equal(0, resumen.SesionesTotales);
            Assert.Equal(0, resumen.MinutosTotales);
            Assert.Equal(0, resumen.TemasEstudiados);
            Assert.Null(resumen.UltimaSesion);
            Assert.Empty(resumen.SesionesPorTipo);
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            var segundaEliminacion = await CrearCasoEliminar(contexto).EjecutarAsync(
                new EliminarSesionEstudioSolicitud(sesion.Id, usuario.Id),
                CancellationToken);

            Assert.Equal(EliminarSesionEstudioEstado.SesionNoEncontrada, segundaEliminacion.Estado);
        }
    }

    [Fact]
    public async Task SesionEstudioCorrections_OwnershipIncorrectoNoElimina()
    {
        await using var ambiente = await AmbientePersistenciaSqlServer.CrearAsync(CancellationToken);
        var usuarioA = Usuario.Registrar("Usuario A", EmailUnico());
        var usuarioB = Usuario.Registrar("Usuario B", EmailUnico());
        var temaA = Tema.Crear(usuarioA.Id, "Modelo OSI", TipoConocimiento.Conceptual);
        var sesion = SesionEstudio.Registrar(
            usuarioA.Id,
            temaA.Id,
            new DateOnly(2026, 8, 20),
            45,
            TipoSesion.Teoria,
            "Estudio inicial");

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            contexto.Usuarios.AddRange(usuarioA, usuarioB);
            contexto.Temas.Add(temaA);
            contexto.SesionesEstudio.Add(sesion);
            await contexto.GuardarCambiosAsync(CancellationToken);
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            var resultado = await CrearCasoEliminar(contexto).EjecutarAsync(
                new EliminarSesionEstudioSolicitud(sesion.Id, usuarioB.Id),
                CancellationToken);

            Assert.Equal(EliminarSesionEstudioEstado.UsuarioNoCoincide, resultado.Estado);
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            var persistida = await contexto.SesionesEstudio
                .AsNoTracking()
                .SingleAsync(s => s.Id == sesion.Id, CancellationToken);

            Assert.Null(persistida.FechaEliminacionUtc);
        }
    }

    private static ActualizarSesionEstudioCasoUso CrearCasoActualizar(AprendizajeDbContext contexto) =>
        new(new SesionEstudioRepository(contexto), new TemaRepository(contexto), new UsuarioRepository(contexto), contexto);

    private static EliminarSesionEstudioCasoUso CrearCasoEliminar(AprendizajeDbContext contexto) =>
        new(new SesionEstudioRepository(contexto), new UsuarioRepository(contexto), contexto);

    private static async Task<int> ContarVinculosHerramientaAsync(
        AprendizajeDbContext contexto,
        Guid sesionId,
        Guid herramientaId) =>
        await contexto.Database
            .SqlQueryRaw<int>(
                "SELECT COUNT(*) AS [Value] FROM [study].[SesionHerramienta] WHERE [SesionId] = {0} AND [HerramientaId] = {1}",
                sesionId,
                herramientaId)
            .SingleAsync(CancellationToken);

    private static string EmailUnico() => $"study-corrections-{Guid.CreateVersion7():N}@local.test";

    private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;
}
