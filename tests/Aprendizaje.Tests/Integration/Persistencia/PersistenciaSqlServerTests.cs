using Aprendizaje.Aplicacion.Resource.Recursos.VincularRecursoATema;
using Aprendizaje.Aplicacion.Study.SesionesEstudio.VincularHerramientaASesionEstudio;
using Aprendizaje.Dominio.Nucleo;
using Aprendizaje.Dominio.Resource;
using Aprendizaje.Dominio.Roadmap;
using Aprendizaje.Dominio.Study;
using Aprendizaje.Infraestructura.Persistencia.Repositorios;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Aprendizaje.Tests.Integration.Persistencia;

[Collection(ColeccionPersistenciaSqlServer.Nombre)]
public sealed class PersistenciaSqlServerTests
{
    [Fact]
    public async Task Tema_ObjetivosVacios_PersistenYMaterializanCorrectamente()
    {
        await using var ambiente = await AmbientePersistenciaSqlServer.CrearAsync(CancellationToken);
        var usuario = Usuario.Registrar("Dylan Tests", EmailUnico());
        var tema = Tema.Crear(usuario.Id, "Fundamentos de redes", TipoConocimiento.Conceptual);

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            contexto.Usuarios.Add(usuario);
            contexto.Temas.Add(tema);
            await contexto.GuardarCambiosAsync(CancellationToken);
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            var objetivosFisicos = await contexto.Database
                .SqlQueryRaw<string?>(
                    "SELECT [Objetivos] AS [Value] FROM [roadmap].[Tema] WHERE [Id] = {0}",
                    tema.Id)
                .SingleAsync(CancellationToken);

            Assert.Null(objetivosFisicos);
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            var rematerializado = await contexto.Temas.SingleAsync(t => t.Id == tema.Id, CancellationToken);

            Assert.NotNull(rematerializado.Objetivos);
            Assert.Empty(rematerializado.Objetivos);
        }
    }

    [Fact]
    public async Task SesionEstudio_RowVersion_CambiaTrasUpdate()
    {
        await using var ambiente = await AmbientePersistenciaSqlServer.CrearAsync(CancellationToken);
        var usuario = Usuario.Registrar("Dylan Tests", EmailUnico());
        var tema = Tema.Crear(usuario.Id, "Modelo OSI", TipoConocimiento.Conceptual);
        var sesion = SesionEstudio.Registrar(
            usuario.Id,
            tema.Id,
            new DateOnly(2026, 8, 24),
            45,
            TipoSesion.Teoria,
            "Estudio inicial");

        byte[] versionInicial;
        byte[] versionPosterior;

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            contexto.Usuarios.Add(usuario);
            contexto.Temas.Add(tema);
            contexto.SesionesEstudio.Add(sesion);
            await contexto.GuardarCambiosAsync(CancellationToken);

            versionInicial = Assert.IsType<byte[]>(sesion.VersionFila);
            Assert.NotEmpty(versionInicial);
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            var persistida = await contexto.SesionesEstudio.SingleAsync(s => s.Id == sesion.Id, CancellationToken);

            persistida.CorregirDuracion(60);
            await contexto.GuardarCambiosAsync(CancellationToken);

            versionPosterior = Assert.IsType<byte[]>(persistida.VersionFila);
            Assert.NotEmpty(versionPosterior);
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            var rematerializada = await contexto.SesionesEstudio
                .AsNoTracking()
                .SingleAsync(s => s.Id == sesion.Id, CancellationToken);

            Assert.Equal(60, rematerializada.DuracionMinutos);
            Assert.NotEqual(versionInicial, versionPosterior);
        }
    }

    [Fact]
    public async Task RecursoTema_VinculoNoSeDuplica()
    {
        await using var ambiente = await AmbientePersistenciaSqlServer.CrearAsync(CancellationToken);
        var usuario = Usuario.Registrar("Dylan Tests", EmailUnico());
        var tema = Tema.Crear(usuario.Id, "Modelo OSI", TipoConocimiento.Conceptual);
        var recurso = Recurso.Guardar(usuario.Id, TipoRecurso.Documentacion, "Documentación modelo OSI");

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            contexto.Usuarios.Add(usuario);
            contexto.Temas.Add(tema);
            contexto.Recursos.Add(recurso);
            await contexto.GuardarCambiosAsync(CancellationToken);
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            var casoUso = new VincularRecursoATemaCasoUso(
                new RecursoRepository(contexto),
                new TemaRepository(contexto),
                contexto);

            var primerResultado = await casoUso.EjecutarAsync(
                new VincularRecursoATemaSolicitud(recurso.Id, tema.Id),
                CancellationToken);
            var segundoResultado = await casoUso.EjecutarAsync(
                new VincularRecursoATemaSolicitud(recurso.Id, tema.Id),
                CancellationToken);

            Assert.Equal(VincularRecursoATemaEstado.Actualizado, primerResultado.Estado);
            Assert.Equal(VincularRecursoATemaEstado.Actualizado, segundoResultado.Estado);
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            var filas = await contexto.Database
                .SqlQueryRaw<int>(
                    "SELECT COUNT(*) AS [Value] FROM [resource].[RecursoTema] WHERE [RecursoId] = {0} AND [TemaId] = {1}",
                    recurso.Id,
                    tema.Id)
                .SingleAsync(CancellationToken);

            Assert.Equal(1, filas);
        }
    }

    [Fact]
    public async Task SesionHerramienta_VinculoNoSeDuplica()
    {
        await using var ambiente = await AmbientePersistenciaSqlServer.CrearAsync(CancellationToken);
        var usuario = Usuario.Registrar("Dylan Tests", EmailUnico());
        var tema = Tema.Crear(usuario.Id, "Modelo OSI", TipoConocimiento.Conceptual);
        var sesion = SesionEstudio.Registrar(
            usuario.Id,
            tema.Id,
            new DateOnly(2026, 8, 24),
            45,
            TipoSesion.Teoria,
            "Estudio inicial");
        var herramienta = Herramienta.Crear($"Wireshark Integration {Guid.CreateVersion7():N}");

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            contexto.Usuarios.Add(usuario);
            contexto.Temas.Add(tema);
            contexto.SesionesEstudio.Add(sesion);
            contexto.Herramientas.Add(herramienta);
            await contexto.GuardarCambiosAsync(CancellationToken);
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            var casoUso = new VincularHerramientaASesionEstudioCasoUso(
                new SesionEstudioRepository(contexto),
                new HerramientaRepository(contexto),
                contexto);

            var primerResultado = await casoUso.EjecutarAsync(
                new VincularHerramientaASesionEstudioSolicitud(sesion.Id, herramienta.Id),
                CancellationToken);
            var segundoResultado = await casoUso.EjecutarAsync(
                new VincularHerramientaASesionEstudioSolicitud(sesion.Id, herramienta.Id),
                CancellationToken);

            Assert.Equal(VincularHerramientaASesionEstudioEstado.Actualizado, primerResultado.Estado);
            Assert.Equal(VincularHerramientaASesionEstudioEstado.Actualizado, segundoResultado.Estado);
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            var filas = await contexto.Database
                .SqlQueryRaw<int>(
                    "SELECT COUNT(*) AS [Value] FROM [study].[SesionHerramienta] WHERE [SesionId] = {0} AND [HerramientaId] = {1}",
                    sesion.Id,
                    herramienta.Id)
                .SingleAsync(CancellationToken);

            Assert.Equal(1, filas);
        }
    }

    [Fact]
    public async Task Tema_QueryFilter_OcultaEliminadoLogicamente()
    {
        await using var ambiente = await AmbientePersistenciaSqlServer.CrearAsync(CancellationToken);
        var usuario = Usuario.Registrar("Dylan Tests", EmailUnico());
        var tema = Tema.Crear(usuario.Id, "Modelo OSI", TipoConocimiento.Conceptual);

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            contexto.Usuarios.Add(usuario);
            contexto.Temas.Add(tema);
            await contexto.GuardarCambiosAsync(CancellationToken);
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            var activo = await contexto.Temas.SingleOrDefaultAsync(t => t.Id == tema.Id, CancellationToken);

            Assert.NotNull(activo);
            activo.MarcarComoEliminado();
            await contexto.GuardarCambiosAsync(CancellationToken);
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            var consultaNormal = await contexto.Temas.SingleOrDefaultAsync(t => t.Id == tema.Id, CancellationToken);
            var consultaSinFiltro = await contexto.Temas
                .IgnoreQueryFilters()
                .SingleOrDefaultAsync(t => t.Id == tema.Id, CancellationToken);

            Assert.Null(consultaNormal);
            Assert.NotNull(consultaSinFiltro);
            Assert.NotNull(consultaSinFiltro.FechaEliminacionUtc);
        }
    }

    [Fact]
    public async Task SesionEstudio_FkTemaInvalida_EsRechazadaPorSqlServer()
    {
        await using var ambiente = await AmbientePersistenciaSqlServer.CrearAsync(CancellationToken);
        var usuario = Usuario.Registrar("Dylan Tests", EmailUnico());
        var sesion = SesionEstudio.Registrar(
            usuario.Id,
            Guid.CreateVersion7(),
            new DateOnly(2026, 8, 24),
            45,
            TipoSesion.Teoria);

        await using var contexto = ambiente.CrearNuevoContexto();
        contexto.Usuarios.Add(usuario);
        contexto.SesionesEstudio.Add(sesion);

        await Assert.ThrowsAsync<DbUpdateException>(() => contexto.GuardarCambiosAsync(CancellationToken));
    }

    private static string EmailUnico() => $"integracion-{Guid.CreateVersion7():N}@local.test";

    private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;
}
