using Aprendizaje.Aplicacion.Roadmap.Certificaciones.VincularCertificacionATema;
using Aprendizaje.Aplicacion.Roadmap.Competencias.VincularCompetenciaATema;
using Aprendizaje.Dominio.Evidence;
using Aprendizaje.Dominio.Nucleo;
using Aprendizaje.Dominio.Roadmap;
using Aprendizaje.Infraestructura.Persistencia.Consultas;
using Aprendizaje.Infraestructura.Persistencia.Repositorios;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Aprendizaje.Tests.Integration.Persistencia;

[Collection(ColeccionPersistenciaSqlServer.Nombre)]
public sealed class ResumenCompetenciaCertificacionSqlServerTests
{
    [Fact]
    public async Task ResumenCompetencias_AgregaTemasVisiblesYAislaUsuarios()
    {
        await using var ambiente = await AmbientePersistenciaSqlServer.CrearAsync(CancellationToken);
        var usuarioA = Usuario.Registrar("Usuario A", EmailUnico());
        var usuarioB = Usuario.Registrar("Usuario B", EmailUnico());
        var temaA1 = Tema.Crear(usuarioA.Id, "Modelo OSI", TipoConocimiento.Conceptual);
        var temaA2 = Tema.Crear(usuarioA.Id, "TCP/IP", TipoConocimiento.Conceptual);
        var temaAEliminado = Tema.Crear(usuarioA.Id, "Tema eliminado", TipoConocimiento.Conceptual);
        var temaB1 = Tema.Crear(usuarioB.Id, "Linux", TipoConocimiento.Procedimental);
        var competenciaA = Competencia.Crear(usuarioA.Id, "Comprensión de redes");
        var competenciaB = Competencia.Crear(usuarioB.Id, "Administración Linux");

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            contexto.Usuarios.AddRange(usuarioA, usuarioB);
            contexto.Temas.AddRange(temaA1, temaA2, temaAEliminado, temaB1);
            contexto.Competencias.AddRange(competenciaA, competenciaB);
            await contexto.GuardarCambiosAsync(CancellationToken);
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            var casoUso = new VincularCompetenciaATemaCasoUso(
                new CompetenciaRepository(contexto),
                new TemaRepository(contexto),
                contexto);

            await casoUso.EjecutarAsync(
                new VincularCompetenciaATemaSolicitud(competenciaA.Id, temaA1.Id),
                CancellationToken);
            await casoUso.EjecutarAsync(
                new VincularCompetenciaATemaSolicitud(competenciaA.Id, temaA2.Id),
                CancellationToken);
            await casoUso.EjecutarAsync(
                new VincularCompetenciaATemaSolicitud(competenciaA.Id, temaAEliminado.Id),
                CancellationToken);
            await casoUso.EjecutarAsync(
                new VincularCompetenciaATemaSolicitud(competenciaB.Id, temaB1.Id),
                CancellationToken);

            var temaEliminado = await contexto.Temas.SingleAsync(t => t.Id == temaAEliminado.Id, CancellationToken);
            temaEliminado.MarcarComoEliminado();
            await contexto.GuardarCambiosAsync(CancellationToken);
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            var consulta = new ConsultaResumenCompetencias(contexto);

            var resumen = await consulta.ListarAsync(usuarioA.Id, CancellationToken);

            var item = Assert.Single(resumen);
            Assert.Equal(competenciaA.Id, item.CompetenciaId);
            Assert.Equal("Comprensión de redes", item.Nombre);
            Assert.Equal(2, item.TemasVinculados);
        }
    }

    [Fact]
    public async Task ResumenCertificaciones_AgregaRelacionesGlobalesYCertificacionesObtenidasVisibles()
    {
        await using var ambiente = await AmbientePersistenciaSqlServer.CrearAsync(CancellationToken);
        var usuarioA = Usuario.Registrar("Usuario A", EmailUnico());
        var usuarioB = Usuario.Registrar("Usuario B", EmailUnico());
        var temaA1 = Tema.Crear(usuarioA.Id, "Modelo OSI", TipoConocimiento.Conceptual);
        var temaAEliminado = Tema.Crear(usuarioA.Id, "Tema eliminado", TipoConocimiento.Conceptual);
        var temaB1 = Tema.Crear(usuarioB.Id, "Linux", TipoConocimiento.Procedimental);
        var certificacionX = Certificacion.Crear("CompTIA Network+", TipoCosto.Pago);
        var certificacionY = Certificacion.Crear("Linux Foundation", TipoCosto.Pago);
        var obtenidaA1 = CertificacionObtenida.Registrar(
            usuarioA.Id,
            certificacionX.Id,
            new DateOnly(2026, 8, 25));
        var obtenidaA2 = CertificacionObtenida.Registrar(
            usuarioA.Id,
            certificacionX.Id,
            new DateOnly(2026, 8, 26));
        var obtenidaAEliminada = CertificacionObtenida.Registrar(
            usuarioA.Id,
            certificacionX.Id,
            new DateOnly(2026, 8, 27));
        var obtenidaB = CertificacionObtenida.Registrar(
            usuarioB.Id,
            certificacionX.Id,
            new DateOnly(2026, 8, 28));

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            contexto.Usuarios.AddRange(usuarioA, usuarioB);
            contexto.Temas.AddRange(temaA1, temaAEliminado, temaB1);
            contexto.Certificaciones.AddRange(certificacionX, certificacionY);
            contexto.CertificacionesObtenidas.AddRange(obtenidaA1, obtenidaA2, obtenidaAEliminada, obtenidaB);
            await contexto.GuardarCambiosAsync(CancellationToken);
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            var casoUso = new VincularCertificacionATemaCasoUso(
                new CertificacionRepository(contexto),
                new TemaRepository(contexto),
                contexto);

            await casoUso.EjecutarAsync(
                new VincularCertificacionATemaSolicitud(certificacionX.Id, temaA1.Id),
                CancellationToken);
            await casoUso.EjecutarAsync(
                new VincularCertificacionATemaSolicitud(certificacionX.Id, temaAEliminado.Id),
                CancellationToken);
            await casoUso.EjecutarAsync(
                new VincularCertificacionATemaSolicitud(certificacionX.Id, temaB1.Id),
                CancellationToken);
            await casoUso.EjecutarAsync(
                new VincularCertificacionATemaSolicitud(certificacionY.Id, temaB1.Id),
                CancellationToken);

            var temaEliminado = await contexto.Temas.SingleAsync(t => t.Id == temaAEliminado.Id, CancellationToken);
            var certificacionObtenidaEliminada = await contexto.CertificacionesObtenidas.SingleAsync(
                c => c.Id == obtenidaAEliminada.Id,
                CancellationToken);
            temaEliminado.MarcarComoEliminado();
            certificacionObtenidaEliminada.MarcarComoEliminado();
            await contexto.GuardarCambiosAsync(CancellationToken);
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            var consulta = new ConsultaResumenCertificaciones(contexto);

            var resumen = await consulta.ListarAsync(usuarioA.Id, CancellationToken);
            var vinculosConPesoNull = await contexto.Database
                .SqlQuery<int>(
                    $"SELECT COUNT(*) AS [Value] FROM [roadmap].[CertificacionTema] WHERE [CertificacionId] = {certificacionX.Id} AND [Peso] IS NULL")
                .SingleAsync(CancellationToken);

            var item = Assert.Single(resumen);
            Assert.Equal(certificacionX.Id, item.CertificacionId);
            Assert.Equal("CompTIA Network+", item.Nombre);
            Assert.Null(item.Proveedor);
            Assert.Equal(TipoCosto.Pago, item.TipoCosto);
            Assert.Equal(1, item.TemasVinculados);
            Assert.Equal(2, item.CertificacionesObtenidas);
            Assert.Equal(3, vinculosConPesoNull);
        }
    }

    [Fact]
    public async Task Resumenes_UsuarioSinRelaciones_RetornanListasVacias()
    {
        await using var ambiente = await AmbientePersistenciaSqlServer.CrearAsync(CancellationToken);
        var usuario = Usuario.Registrar("Usuario sin relaciones", EmailUnico());

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            contexto.Usuarios.Add(usuario);
            await contexto.GuardarCambiosAsync(CancellationToken);
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            var competencias = await new ConsultaResumenCompetencias(contexto).ListarAsync(
                usuario.Id,
                CancellationToken);
            var certificaciones = await new ConsultaResumenCertificaciones(contexto).ListarAsync(
                usuario.Id,
                CancellationToken);

            Assert.Empty(competencias);
            Assert.Empty(certificaciones);
        }
    }

    private static string EmailUnico() => $"analytics-roadmap-{Guid.CreateVersion7():N}@local.test";

    private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;
}
