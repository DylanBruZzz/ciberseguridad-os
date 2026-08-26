using Aprendizaje.Dominio.Nucleo;
using Aprendizaje.Dominio.Roadmap;
using Aprendizaje.Dominio.Study;
using Aprendizaje.Infraestructura.Persistencia.Consultas;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Aprendizaje.Tests.Integration.Persistencia;

[Collection(ColeccionPersistenciaSqlServer.Nombre)]
public sealed class ResumenEstudioSqlServerTests
{
    [Fact]
    public async Task ResumenEstudio_UsuarioConSesiones_AgregaDatosDirectosYAislaUsuarios()
    {
        await using var ambiente = await AmbientePersistenciaSqlServer.CrearAsync(CancellationToken);
        var usuarioA = Usuario.Registrar("Usuario A", EmailUnico());
        var usuarioB = Usuario.Registrar("Usuario B", EmailUnico());
        var temaA1 = Tema.Crear(usuarioA.Id, "Modelo OSI", TipoConocimiento.Conceptual);
        var temaA2 = Tema.Crear(usuarioA.Id, "TCP/IP", TipoConocimiento.Conceptual);
        var temaB = Tema.Crear(usuarioB.Id, "Linux", TipoConocimiento.Procedimental);
        var sesionA1 = SesionEstudio.Registrar(
            usuarioA.Id,
            temaA1.Id,
            new DateOnly(2026, 8, 20),
            30,
            TipoSesion.Teoria);
        var sesionA2 = SesionEstudio.Registrar(
            usuarioA.Id,
            temaA1.Id,
            new DateOnly(2026, 8, 21),
            20,
            TipoSesion.Teoria);
        var sesionA3 = SesionEstudio.Registrar(
            usuarioA.Id,
            temaA2.Id,
            new DateOnly(2026, 8, 22),
            40,
            TipoSesion.Practica);
        var sesionB = SesionEstudio.Registrar(
            usuarioB.Id,
            temaB.Id,
            new DateOnly(2026, 8, 23),
            999,
            TipoSesion.Laboratorio);

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            contexto.Usuarios.AddRange(usuarioA, usuarioB);
            contexto.Temas.AddRange(temaA1, temaA2, temaB);
            contexto.SesionesEstudio.AddRange(sesionA1, sesionA2, sesionA3, sesionB);
            await contexto.GuardarCambiosAsync(CancellationToken);
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            var consulta = new ConsultaResumenEstudio(contexto);

            var resumen = await consulta.ObtenerAsync(usuarioA.Id, CancellationToken);

            Assert.Equal(usuarioA.Id, resumen.UsuarioId);
            Assert.Equal(3, resumen.SesionesTotales);
            Assert.Equal(90, resumen.MinutosTotales);
            Assert.Equal(2, resumen.TemasEstudiados);
            Assert.Equal(new DateOnly(2026, 8, 22), resumen.UltimaSesion);

            Assert.Collection(
                resumen.SesionesPorTipo,
                teoria =>
                {
                    Assert.Equal(TipoSesion.Teoria, teoria.Tipo);
                    Assert.Equal(2, teoria.Sesiones);
                    Assert.Equal(50, teoria.Minutos);
                },
                practica =>
                {
                    Assert.Equal(TipoSesion.Practica, practica.Tipo);
                    Assert.Equal(1, practica.Sesiones);
                    Assert.Equal(40, practica.Minutos);
                });
        }
    }

    [Fact]
    public async Task ResumenEstudio_UsuarioSinSesiones_RetornaResumenVacio()
    {
        await using var ambiente = await AmbientePersistenciaSqlServer.CrearAsync(CancellationToken);
        var usuario = Usuario.Registrar("Usuario sin sesiones", EmailUnico());

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            contexto.Usuarios.Add(usuario);
            await contexto.GuardarCambiosAsync(CancellationToken);
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            var consulta = new ConsultaResumenEstudio(contexto);

            var resumen = await consulta.ObtenerAsync(usuario.Id, CancellationToken);

            Assert.Equal(usuario.Id, resumen.UsuarioId);
            Assert.Equal(0, resumen.SesionesTotales);
            Assert.Equal(0, resumen.MinutosTotales);
            Assert.Equal(0, resumen.TemasEstudiados);
            Assert.Null(resumen.UltimaSesion);
            Assert.Empty(resumen.SesionesPorTipo);
        }
    }

    [Fact]
    public async Task ResumenEstudio_NoCuentaSesionesEliminadasLogicamente()
    {
        await using var ambiente = await AmbientePersistenciaSqlServer.CrearAsync(CancellationToken);
        var usuario = Usuario.Registrar("Usuario con soft delete", EmailUnico());
        var tema = Tema.Crear(usuario.Id, "Modelo OSI", TipoConocimiento.Conceptual);
        var sesionActiva = SesionEstudio.Registrar(
            usuario.Id,
            tema.Id,
            new DateOnly(2026, 8, 20),
            30,
            TipoSesion.Teoria);
        var sesionEliminada = SesionEstudio.Registrar(
            usuario.Id,
            tema.Id,
            new DateOnly(2026, 8, 21),
            90,
            TipoSesion.Practica);
        sesionEliminada.MarcarComoEliminado();

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            contexto.Usuarios.Add(usuario);
            contexto.Temas.Add(tema);
            contexto.SesionesEstudio.AddRange(sesionActiva, sesionEliminada);
            await contexto.GuardarCambiosAsync(CancellationToken);
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            var consulta = new ConsultaResumenEstudio(contexto);

            var resumen = await consulta.ObtenerAsync(usuario.Id, CancellationToken);
            var sesionesFisicas = await contexto.SesionesEstudio
                .IgnoreQueryFilters()
                .CountAsync(s => s.UsuarioId == usuario.Id, CancellationToken);

            Assert.Equal(2, sesionesFisicas);
            Assert.Equal(1, resumen.SesionesTotales);
            Assert.Equal(30, resumen.MinutosTotales);
            Assert.Equal(1, resumen.TemasEstudiados);
            Assert.Equal(new DateOnly(2026, 8, 20), resumen.UltimaSesion);
            var porTipo = Assert.Single(resumen.SesionesPorTipo);
            Assert.Equal(TipoSesion.Teoria, porTipo.Tipo);
            Assert.Equal(1, porTipo.Sesiones);
            Assert.Equal(30, porTipo.Minutos);
        }
    }

    private static string EmailUnico() => $"analytics-{Guid.CreateVersion7():N}@local.test";

    private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;
}
