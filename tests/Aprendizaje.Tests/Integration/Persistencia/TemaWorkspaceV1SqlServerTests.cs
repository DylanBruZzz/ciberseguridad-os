using System.Data.Common;
using Aprendizaje.Dominio.Evidence;
using Aprendizaje.Dominio.Nucleo;
using Aprendizaje.Dominio.Resource;
using Aprendizaje.Dominio.Roadmap;
using Aprendizaje.Dominio.Roadmap.ValueObjects;
using Aprendizaje.Dominio.Study;
using Aprendizaje.Infraestructura.Persistencia;
using Aprendizaje.Infraestructura.Persistencia.Consultas.Roadmap;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Xunit;

namespace Aprendizaje.Tests.Integration.Persistencia;

[Collection(ColeccionPersistenciaSqlServer.Nombre)]
public sealed class TemaWorkspaceV1SqlServerTests
{
    private static readonly DateTime AhoraUtc = new(2026, 9, 1, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task TemaWorkspace_TemaExistente_ComponeDatosYResumenesContextuales()
    {
        await using var ambiente = await AmbientePersistenciaSqlServer.CrearAsync(CancellationToken);
        var usuario = Usuario.Registrar("Usuario workspace", EmailUnico());
        usuario.ConfigurarIntervaloRepasoDefecto(30);
        var otroUsuario = Usuario.Registrar("Otro workspace", EmailUnico());
        var fase = Fase.Crear(usuario.Id, "Fundamentos", 1);
        var tema = CrearTemaDominado(usuario.Id, fase.Id, "TCP/IP");
        tema.ActualizarDescripcion("Modelo y diagnostico de redes");
        EstablecerObjetivosParaPrueba(tema);
        tema.ActualizarDificultadPercibida(NivelPercepcion.Crear(4));
        tema.ActualizarConfianza(NivelPercepcion.Crear(3));
        tema.ConfigurarIntervaloRepaso(IntervaloRepaso.Crear(10));
        var apunte = ApunteTema.Crear(usuario.Id, tema.Id, "Capas, encapsulacion y troubleshooting");
        var herramienta = Herramienta.Crear("Wireshark");
        var recurso = Recurso.Guardar(usuario.Id, TipoRecurso.Documentacion, "RFC TCP");
        var recursoAjeno = Recurso.Guardar(otroUsuario.Id, TipoRecurso.Video, "No contar");
        var proyecto = Proyecto.Crear(usuario.Id, "Proyecto redes");
        var laboratorio = Laboratorio.Crear(usuario.Id, "Lab Wireshark");
        var writeup = Writeup.Crear(usuario.Id, "Writeup traceroute");
        var artefacto = ArtefactoTecnico.Crear(usuario.Id, TipoArtefacto.Cheatsheet, "Cheatsheet TCP");
        var certificacion = Certificacion.Crear("Network+", TipoCosto.Pago);
        var certificacionObtenida = CertificacionObtenida.Registrar(
            usuario.Id,
            certificacion.Id,
            new DateOnly(2026, 8, 15));

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            contexto.Usuarios.AddRange(usuario, otroUsuario);
            contexto.Fases.Add(fase);
            contexto.Temas.Add(tema);
            contexto.ApuntesTema.Add(apunte);
            contexto.Herramientas.Add(herramienta);
            contexto.Recursos.AddRange(recurso, recursoAjeno);
            contexto.Proyectos.Add(proyecto);
            contexto.Laboratorios.Add(laboratorio);
            contexto.Writeups.Add(writeup);
            contexto.ArtefactosTecnicos.Add(artefacto);
            contexto.Certificaciones.Add(certificacion);
            contexto.CertificacionesObtenidas.Add(certificacionObtenida);
            contexto.SesionesEstudio.AddRange(
                SesionEstudio.Registrar(usuario.Id, tema.Id, new DateOnly(2026, 8, 1), 30, TipoSesion.Practica),
                SesionEstudio.Registrar(usuario.Id, tema.Id, new DateOnly(2026, 8, 10), 45, TipoSesion.Repaso),
                SesionEstudio.Registrar(otroUsuario.Id, tema.Id, new DateOnly(2026, 8, 30), 999, TipoSesion.Practica));
            await contexto.GuardarCambiosAsync(CancellationToken);
            await VincularTemaHerramientaAsync(contexto, tema.Id, herramienta.Id);
            await VincularRecursoTemaAsync(contexto, recurso.Id, tema.Id);
            await VincularRecursoTemaAsync(contexto, recursoAjeno.Id, tema.Id);
            await VincularProyectoTemaAsync(contexto, proyecto.Id, tema.Id);
            await VincularLaboratorioTemaAsync(contexto, laboratorio.Id, tema.Id);
            await VincularWriteupTemaAsync(contexto, writeup.Id, tema.Id);
            await VincularArtefactoTemaAsync(contexto, artefacto.Id, tema.Id);
            await VincularCertificacionTemaAsync(contexto, certificacion.Id, tema.Id);
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            var consulta = new ConsultaTemaWorkspaceV1(contexto);

            var workspace = await consulta.ObtenerAsync(usuario.Id, tema.Id, AhoraUtc, CancellationToken);

            Assert.NotNull(workspace);
            Assert.Equal(tema.Id, workspace.Tema.Id);
            Assert.Equal("TCP/IP", workspace.Tema.Nombre);
            Assert.Equal("Modelo y diagnostico de redes", workspace.Tema.Descripcion);
            Assert.Equal(TipoConocimiento.Conceptual, workspace.Tema.TipoConocimiento);
            Assert.Equal(EstadoTema.EnRepaso, workspace.Tema.Estado);
            Assert.Equal(4, workspace.Tema.DificultadPercibida);
            Assert.Equal(3, workspace.Tema.Confianza);
            Assert.Equal(10, workspace.Tema.IntervaloRepasoDias);
            Assert.Equal(2, workspace.Tema.CriteriosTotal);
            Assert.Equal(2, workspace.Tema.CriteriosCumplidos);
            Assert.Equal(100, workspace.Tema.ProgresoPorcentaje);
            Assert.Equal(["Entender TCP", "Diagnosticar conectividad"], workspace.Tema.Objetivos);
            Assert.All(workspace.Tema.Criterios, c => Assert.True(c.Cumplido));
            Assert.All(workspace.Tema.Criterios, c => Assert.NotNull(c.FechaCumplidoUtc));
            Assert.NotNull(workspace.Fase);
            Assert.Equal(fase.Id, workspace.Fase.Id);
            Assert.Equal(1, workspace.Fase.Orden);
            Assert.Equal("Fundamentos", workspace.Fase.Nombre);
            Assert.Equal("Capas, encapsulacion y troubleshooting", workspace.Apuntes.Contenido);
            Assert.NotNull(workspace.Apuntes.FechaModificacionUtc);
            var herramientaWorkspace = Assert.Single(workspace.Herramientas);
            Assert.Equal(herramienta.Id, herramientaWorkspace.Id);
            Assert.Equal("Wireshark", herramientaWorkspace.Nombre);
            var certificacionWorkspace = Assert.Single(workspace.Certificaciones);
            Assert.Equal(certificacion.Id, certificacionWorkspace.Id);
            Assert.Equal("Network+", certificacionWorkspace.Nombre);
            Assert.Equal(TipoCosto.Pago, certificacionWorkspace.TipoCosto);
            Assert.NotNull(workspace.UltimaSesion);
            Assert.Equal(new DateOnly(2026, 8, 10), workspace.UltimaSesion.Fecha);
            Assert.Equal(45, workspace.UltimaSesion.DuracionMinutos);
            Assert.Equal(TipoSesion.Repaso, workspace.UltimaSesion.Tipo);
            Assert.Equal(new DateOnly(2026, 8, 20), workspace.Repaso.ProximaFechaRepaso);
            Assert.True(workspace.Repaso.RepasoRecomendado);
            Assert.Equal(1, workspace.ResourcesResumen.Total);
            Assert.Equal(2, workspace.SesionesResumen.Total);
            Assert.Equal(75, workspace.SesionesResumen.TotalMinutos);
            Assert.Equal(5, workspace.EvidenceResumen.Total);
            Assert.Equal(1, workspace.EvidenceResumen.Proyectos);
            Assert.Equal(1, workspace.EvidenceResumen.Laboratorios);
            Assert.Equal(1, workspace.EvidenceResumen.Writeups);
            Assert.Equal(1, workspace.EvidenceResumen.ArtefactosTecnicos);
            Assert.Equal(1, workspace.EvidenceResumen.CertificacionesObtenidas);
        }
    }

    [Fact]
    public async Task TemaWorkspace_TemaInexistenteOAjeno_RetornaNull()
    {
        await using var ambiente = await AmbientePersistenciaSqlServer.CrearAsync(CancellationToken);
        var usuario = Usuario.Registrar("Usuario propietario", EmailUnico());
        var otroUsuario = Usuario.Registrar("Usuario ajeno", EmailUnico());
        var temaAjeno = Tema.Crear(otroUsuario.Id, "Ajeno", TipoConocimiento.Conceptual);

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            contexto.Usuarios.AddRange(usuario, otroUsuario);
            contexto.Temas.Add(temaAjeno);
            await contexto.GuardarCambiosAsync(CancellationToken);
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            var consulta = new ConsultaTemaWorkspaceV1(contexto);

            var ajeno = await consulta.ObtenerAsync(usuario.Id, temaAjeno.Id, AhoraUtc, CancellationToken);
            var inexistente = await consulta.ObtenerAsync(usuario.Id, Guid.CreateVersion7(), AhoraUtc, CancellationToken);

            Assert.Null(ajeno);
            Assert.Null(inexistente);
        }
    }

    [Fact]
    public async Task TemaWorkspace_SinRelaciones_RetornaVaciosYConteosCero()
    {
        await using var ambiente = await AmbientePersistenciaSqlServer.CrearAsync(CancellationToken);
        var usuario = Usuario.Registrar("Usuario sin relaciones", EmailUnico());
        var tema = Tema.Crear(usuario.Id, "Tema aislado", TipoConocimiento.Conceptual);

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            contexto.Usuarios.Add(usuario);
            contexto.Temas.Add(tema);
            await contexto.GuardarCambiosAsync(CancellationToken);
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            var consulta = new ConsultaTemaWorkspaceV1(contexto);

            var workspace = await consulta.ObtenerAsync(usuario.Id, tema.Id, AhoraUtc, CancellationToken);

            Assert.NotNull(workspace);
            Assert.Null(workspace.Fase);
            Assert.Equal(EstadoTema.NoIniciado, workspace.Tema.Estado);
            Assert.Equal(0, workspace.Tema.CriteriosTotal);
            Assert.Equal(0, workspace.Tema.CriteriosCumplidos);
            Assert.Equal(0, workspace.Tema.ProgresoPorcentaje);
            Assert.Empty(workspace.Tema.Criterios);
            Assert.Empty(workspace.Tema.Objetivos);
            Assert.Equal(string.Empty, workspace.Apuntes.Contenido);
            Assert.Null(workspace.Apuntes.FechaModificacionUtc);
            Assert.Null(workspace.UltimaSesion);
            Assert.Null(workspace.Repaso.ProximaFechaRepaso);
            Assert.False(workspace.Repaso.RepasoRecomendado);
            Assert.Equal(0, workspace.ResourcesResumen.Total);
            Assert.Equal(0, workspace.SesionesResumen.Total);
            Assert.Equal(0, workspace.SesionesResumen.TotalMinutos);
            Assert.Equal(0, workspace.EvidenceResumen.Total);
        }
    }

    [Fact]
    public async Task TemaWorkspace_ProgresoEstadoYRepaso_CoincidenConRoadmapVista()
    {
        await using var ambiente = await AmbientePersistenciaSqlServer.CrearAsync(CancellationToken);
        var usuario = Usuario.Registrar("Usuario paridad", EmailUnico());
        var fase = Fase.Crear(usuario.Id, "Fase", 1);
        var tema = CrearTemaDominado(usuario.Id, fase.Id, "Tema repaso");
        tema.ConfigurarIntervaloRepaso(IntervaloRepaso.Crear(7));

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            contexto.Usuarios.Add(usuario);
            contexto.Fases.Add(fase);
            contexto.Temas.Add(tema);
            contexto.SesionesEstudio.Add(
                SesionEstudio.Registrar(usuario.Id, tema.Id, new DateOnly(2026, 8, 1), 25, TipoSesion.Practica));
            await contexto.GuardarCambiosAsync(CancellationToken);
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            var roadmap = await new ConsultaRoadmapVistaV1(contexto)
                .ObtenerAsync(usuario.Id, AhoraUtc, CancellationToken);
            var workspace = await new ConsultaTemaWorkspaceV1(contexto)
                .ObtenerAsync(usuario.Id, tema.Id, AhoraUtc, CancellationToken);
            var temaRoadmap = roadmap.Fases.Single().Temas.Single(t => t.Id == tema.Id);

            Assert.NotNull(workspace);
            Assert.Equal(temaRoadmap.ProgresoPorcentaje, workspace.Tema.ProgresoPorcentaje);
            Assert.Equal(temaRoadmap.CriteriosTotal, workspace.Tema.CriteriosTotal);
            Assert.Equal(temaRoadmap.CriteriosCumplidos, workspace.Tema.CriteriosCumplidos);
            Assert.Equal(temaRoadmap.Estado, workspace.Tema.Estado);
            Assert.Equal(temaRoadmap.ProximaFechaRepaso, workspace.Repaso.ProximaFechaRepaso);
            Assert.Equal(temaRoadmap.RepasoRecomendado, workspace.Repaso.RepasoRecomendado);
        }
    }

    [Fact]
    public async Task TemaWorkspace_ConsultaUsaNumeroConstanteDeComandos()
    {
        await using var ambiente = await AmbientePersistenciaSqlServer.CrearAsync(CancellationToken);
        var usuario = Usuario.Registrar("Usuario performance workspace", EmailUnico());
        var fase = Fase.Crear(usuario.Id, "Fase", 1);
        var tema = CrearTemaDominado(usuario.Id, fase.Id, "Tema");

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            contexto.Usuarios.Add(usuario);
            contexto.Fases.Add(fase);
            contexto.Temas.Add(tema);
            contexto.SesionesEstudio.AddRange(
                Enumerable.Range(1, 6).Select(i =>
                    SesionEstudio.Registrar(
                        usuario.Id,
                        tema.Id,
                        new DateOnly(2026, 8, i),
                        20,
                        TipoSesion.Practica)));
            await contexto.GuardarCambiosAsync(CancellationToken);
        }

        var contador = new ContadorComandosInterceptor();
        await using (var contexto = CrearContextoContado(contador))
        {
            var consulta = new ConsultaTemaWorkspaceV1(contexto);

            _ = await consulta.ObtenerAsync(usuario.Id, tema.Id, AhoraUtc, CancellationToken);
        }

        Assert.True(contador.ComandosLectura <= 10, $"Se esperaban como maximo 10 comandos; se ejecutaron {contador.ComandosLectura}.");
    }

    private static Tema CrearTemaDominado(Guid usuarioId, Guid faseId, string nombre)
    {
        var tema = Tema.Crear(usuarioId, nombre, TipoConocimiento.Conceptual);
        tema.AsignarFase(faseId);
        tema.DefinirCriteriosRelevantes([TipoCriterio.Teoria, TipoCriterio.Practica]);
        tema.MarcarCriterio(TipoCriterio.Teoria);
        tema.MarcarCriterio(TipoCriterio.Practica);

        return tema;
    }

    private static async Task VincularRecursoTemaAsync(AprendizajeDbContext contexto, Guid recursoId, Guid temaId) =>
        await contexto.Database.ExecuteSqlInterpolatedAsync(
            $"INSERT INTO [resource].[RecursoTema] ([RecursoId], [TemaId]) VALUES ({recursoId}, {temaId})",
            CancellationToken);

    private static async Task VincularTemaHerramientaAsync(AprendizajeDbContext contexto, Guid temaId, Guid herramientaId) =>
        await contexto.Database.ExecuteSqlInterpolatedAsync(
            $"INSERT INTO [roadmap].[TemaHerramienta] ([TemaId], [HerramientaId]) VALUES ({temaId}, {herramientaId})",
            CancellationToken);

    private static async Task VincularProyectoTemaAsync(AprendizajeDbContext contexto, Guid proyectoId, Guid temaId) =>
        await contexto.Database.ExecuteSqlInterpolatedAsync(
            $"INSERT INTO [evidence].[ProyectoTema] ([ProyectoId], [TemaId]) VALUES ({proyectoId}, {temaId})",
            CancellationToken);

    private static async Task VincularLaboratorioTemaAsync(AprendizajeDbContext contexto, Guid laboratorioId, Guid temaId) =>
        await contexto.Database.ExecuteSqlInterpolatedAsync(
            $"INSERT INTO [evidence].[LaboratorioTema] ([LaboratorioId], [TemaId]) VALUES ({laboratorioId}, {temaId})",
            CancellationToken);

    private static async Task VincularWriteupTemaAsync(AprendizajeDbContext contexto, Guid writeupId, Guid temaId) =>
        await contexto.Database.ExecuteSqlInterpolatedAsync(
            $"INSERT INTO [evidence].[WriteupTema] ([WriteupId], [TemaId]) VALUES ({writeupId}, {temaId})",
            CancellationToken);

    private static async Task VincularArtefactoTemaAsync(AprendizajeDbContext contexto, Guid artefactoId, Guid temaId) =>
        await contexto.Database.ExecuteSqlInterpolatedAsync(
            $"INSERT INTO [evidence].[ArtefactoTema] ([ArtefactoTecnicoId], [TemaId]) VALUES ({artefactoId}, {temaId})",
            CancellationToken);

    private static async Task VincularCertificacionTemaAsync(AprendizajeDbContext contexto, Guid certificacionId, Guid temaId) =>
        await contexto.Database.ExecuteSqlInterpolatedAsync(
            $"INSERT INTO [roadmap].[CertificacionTema] ([CertificacionId], [TemaId]) VALUES ({certificacionId}, {temaId})",
            CancellationToken);

    private static AprendizajeDbContext CrearContextoContado(ContadorComandosInterceptor contador)
    {
        var opciones = new DbContextOptionsBuilder<AprendizajeDbContext>()
            .UseSqlServer(AmbientePersistenciaSqlServer.ConnectionString)
            .AddInterceptors(contador)
            .Options;

        return new AprendizajeDbContext(opciones);
    }

    private sealed class ContadorComandosInterceptor : DbCommandInterceptor
    {
        public int ComandosLectura { get; private set; }

        public override InterceptionResult<DbDataReader> ReaderExecuting(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<DbDataReader> result)
        {
            ComandosLectura++;

            return base.ReaderExecuting(command, eventData, result);
        }

        public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<DbDataReader> result,
            CancellationToken cancellationToken = default)
        {
            ComandosLectura++;

            return base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
        }
    }

    private static void EstablecerObjetivosParaPrueba(Tema tema) =>
        tema.EstablecerObjetivos(["Entender TCP", "Diagnosticar conectividad"]);

    private static string EmailUnico() => $"tema-workspace-{Guid.CreateVersion7():N}@local.test";

    private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;
}
