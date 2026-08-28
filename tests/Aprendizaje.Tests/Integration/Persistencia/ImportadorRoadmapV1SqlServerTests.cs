using System.Diagnostics;
using Aprendizaje.Aplicacion.Roadmap.Importacion;
using Aprendizaje.Dominio.Nucleo;
using Aprendizaje.Dominio.Roadmap;
using Aprendizaje.Dominio.Study;
using Aprendizaje.Infraestructura.Importacion;
using Aprendizaje.Infraestructura.Persistencia;
using Aprendizaje.Infraestructura.Persistencia.Repositorios;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Aprendizaje.Tests.Integration.Persistencia;

[Collection(ColeccionPersistenciaSqlServer.Nombre)]
public sealed class ImportadorRoadmapV1SqlServerTests
{
    [Fact]
    public async Task ImportadorRoadmapV1_ImportaDatasetRealYNoCreaEvidence()
    {
        await using var ambiente = await AmbientePersistenciaSqlServer.CrearAsync(CancellationToken);
        var usuario = Usuario.Registrar("Roadmap V1", EmailUnico());
        var herramientaPreexistente = Herramienta.Crear("Nmap");
        var certificacionPreexistente = Certificacion.Crear("CompTIA A+", TipoCosto.Pago);

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            contexto.Usuarios.Add(usuario);
            contexto.Herramientas.Add(herramientaPreexistente);
            contexto.Certificaciones.Add(certificacionPreexistente);
            await contexto.GuardarCambiosAsync(CancellationToken);
        }

        var documento = await LeerDocumentoAsync();

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            var resultado = await CrearCasoUso(contexto).EjecutarAsync(
                new ImportarRoadmapV1Solicitud(usuario.Id, documento),
                CancellationToken);

            Assert.True(
                resultado.Estado == ImportarRoadmapV1Estado.Importado,
                string.Join(Environment.NewLine, resultado.Errores));
            Assert.Equal(new ImportacionRoadmapConteo(7, 0, 0, 0), resultado.Fases);
            Assert.Equal(new ImportacionRoadmapConteo(63, 0, 0, 0), resultado.Temas);
            Assert.Equal(new ImportacionRoadmapConteo(62, 0, 1, 1), resultado.Herramientas);
            Assert.Equal(new ImportacionRoadmapConteo(11, 0, 1, 1), resultado.Certificaciones);
            Assert.Equal(new ImportacionRoadmapConteo(30, 0, 0, 0), resultado.Recursos);
            Assert.Equal(0, resultado.EvidenceCreadas);
            Assert.Empty(resultado.Errores);
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            Assert.Equal(7, await contexto.Fases.CountAsync(f => f.UsuarioId == usuario.Id, CancellationToken));
            Assert.Equal(63, await contexto.Temas.CountAsync(t => t.UsuarioId == usuario.Id, CancellationToken));
            Assert.Equal(30, await contexto.Recursos.CountAsync(r => r.UsuarioId == usuario.Id, CancellationToken));
            Assert.Equal(63, await contexto.Herramientas.CountAsync(CancellationToken));
            Assert.Equal(12, await contexto.Certificaciones.CountAsync(CancellationToken));

            var faseUno = await contexto.Fases.AsNoTracking()
                .SingleAsync(f => f.UsuarioId == usuario.Id && f.Orden == 1, CancellationToken);

            Assert.Equal("Fundamentos de Informática y Redes", faseUno.Nombre);
            Assert.Equal("Fundamentos Sólidos", faseUno.Descripcion);
            Assert.Equal(1, faseUno.MesInicioRecomendado);
            Assert.Equal(4, faseUno.MesFinRecomendado);
            Assert.Equal("~10 hrs/semana", faseUno.CargaSemanalRecomendada);
            Assert.Equal(5, faseUno.Objetivos.Count);
            Assert.Equal(5, faseUno.CriteriosAvance.Count);

            Assert.True(await contexto.Temas.AnyAsync(
                t => t.UsuarioId == usuario.Id && t.Nombre == "Modelo OSI / TCP-IP",
                CancellationToken));
            Assert.True(await contexto.Temas.AnyAsync(
                t => t.UsuarioId == usuario.Id && t.Nombre == "DNS, DHCP, HTTP/S",
                CancellationToken));
            Assert.True(await contexto.Temas.AnyAsync(
                t => t.UsuarioId == usuario.Id && t.Nombre == "Splunk / Wazuh",
                CancellationToken));
            Assert.Equal(0, await contexto.Recursos.CountAsync(r => r.UsuarioId == usuario.Id && r.Url != null, CancellationToken));

            var nmap = await contexto.Herramientas.SingleAsync(h => h.Nombre == "Nmap", CancellationToken);
            var comptiaAPlus = await contexto.Certificaciones.SingleAsync(c => c.Nombre == "CompTIA A+", CancellationToken);
            Assert.Equal("Análisis de Red", nmap.Categoria);
            Assert.Equal("CompTIA", comptiaAPlus.Proveedor);

            await AssertNoEvidenceAsync(contexto, usuario.Id);
            Assert.Equal(0, await ContarRecursoTemaAsync(contexto));
            Assert.Equal(0, await ContarCertificacionTemaAsync(contexto));
            Assert.Equal(0, await ContarCompetenciaTemaAsync(contexto));
            Assert.Equal(0, await ContarTemaDependenciaAsync(contexto));
        }
    }

    [Fact]
    public async Task ImportadorRoadmapV1_SegundaEjecucionNoDuplica()
    {
        await using var ambiente = await AmbientePersistenciaSqlServer.CrearAsync(CancellationToken);
        var usuario = Usuario.Registrar("Roadmap V1", EmailUnico());
        var documento = await LeerDocumentoAsync();

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            contexto.Usuarios.Add(usuario);
            await contexto.GuardarCambiosAsync(CancellationToken);
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            var resultado = await CrearCasoUso(contexto).EjecutarAsync(
                new ImportarRoadmapV1Solicitud(usuario.Id, documento),
                CancellationToken);

            Assert.True(
                resultado.Estado == ImportarRoadmapV1Estado.Importado,
                string.Join(Environment.NewLine, resultado.Errores));
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            var resultado = await CrearCasoUso(contexto).EjecutarAsync(
                new ImportarRoadmapV1Solicitud(usuario.Id, documento),
                CancellationToken);

            Assert.True(
                resultado.Estado == ImportarRoadmapV1Estado.Importado,
                string.Join(Environment.NewLine, resultado.Errores));
            Assert.Equal(new ImportacionRoadmapConteo(0, 7, 0, 0), resultado.Fases);
            Assert.Equal(new ImportacionRoadmapConteo(0, 63, 0, 0), resultado.Temas);
            Assert.Equal(new ImportacionRoadmapConteo(0, 0, 0, 63), resultado.Herramientas);
            Assert.Equal(new ImportacionRoadmapConteo(0, 0, 0, 12), resultado.Certificaciones);
            Assert.Equal(new ImportacionRoadmapConteo(0, 30, 0, 0), resultado.Recursos);
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            Assert.Equal(7, await contexto.Fases.CountAsync(f => f.UsuarioId == usuario.Id, CancellationToken));
            Assert.Equal(63, await contexto.Temas.CountAsync(t => t.UsuarioId == usuario.Id, CancellationToken));
            Assert.Equal(30, await contexto.Recursos.CountAsync(r => r.UsuarioId == usuario.Id, CancellationToken));
            Assert.Equal(63, await contexto.Herramientas.CountAsync(CancellationToken));
            Assert.Equal(12, await contexto.Certificaciones.CountAsync(CancellationToken));
        }
    }

    [Fact]
    public async Task ImportadorRoadmapV1_ConflictoHaceRollbackSinCambiosParciales()
    {
        await using var ambiente = await AmbientePersistenciaSqlServer.CrearAsync(CancellationToken);
        var usuario = Usuario.Registrar("Roadmap V1", EmailUnico());
        var certificacionIncompatible = Certificacion.Crear("CompTIA A+", TipoCosto.Gratuita);
        var documento = await LeerDocumentoAsync();

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            contexto.Usuarios.Add(usuario);
            contexto.Certificaciones.Add(certificacionIncompatible);
            await contexto.GuardarCambiosAsync(CancellationToken);
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            var resultado = await CrearCasoUso(contexto).EjecutarAsync(
                new ImportarRoadmapV1Solicitud(usuario.Id, documento),
                CancellationToken);

            Assert.Equal(ImportarRoadmapV1Estado.Conflicto, resultado.Estado);
            Assert.Contains(resultado.Errores, e => e.Contains("TipoCosto", StringComparison.OrdinalIgnoreCase));
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            Assert.Equal(0, await contexto.Fases.CountAsync(f => f.UsuarioId == usuario.Id, CancellationToken));
            Assert.Equal(0, await contexto.Temas.CountAsync(t => t.UsuarioId == usuario.Id, CancellationToken));
            Assert.Equal(0, await contexto.Recursos.CountAsync(r => r.UsuarioId == usuario.Id, CancellationToken));
            Assert.Equal(0, await contexto.Herramientas.CountAsync(CancellationToken));
            Assert.Equal(1, await contexto.Certificaciones.CountAsync(CancellationToken));
            await AssertNoEvidenceAsync(contexto, usuario.Id);
        }
    }

    [Fact]
    public async Task Cli_ImportaRoadmapV1_ContraAprendizajeTestsDb()
    {
        await using var ambiente = await AmbientePersistenciaSqlServer.CrearAsync(CancellationToken);
        var usuario = Usuario.Registrar("CLI Roadmap V1", EmailUnico());

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            contexto.Usuarios.Add(usuario);
            await contexto.GuardarCambiosAsync(CancellationToken);
        }

        var repoRoot = ResolverRaizRepositorio();
        using var proceso = new Process();
        proceso.StartInfo.FileName = "dotnet";
        proceso.StartInfo.Arguments = $"run --project tools/Aprendizaje.ImportadorRoadmap -- --usuario-id {usuario.Id}";
        proceso.StartInfo.WorkingDirectory = repoRoot;
        proceso.StartInfo.RedirectStandardOutput = true;
        proceso.StartInfo.RedirectStandardError = true;
        proceso.StartInfo.UseShellExecute = false;
        proceso.StartInfo.Environment["APRENDIZAJE_IMPORTADOR_CONNECTION_STRING"] =
            AmbientePersistenciaSqlServer.ConnectionString;

        proceso.Start();

        var salida = await proceso.StandardOutput.ReadToEndAsync(CancellationToken);
        var error = await proceso.StandardError.ReadToEndAsync(CancellationToken);
        await proceso.WaitForExitAsync(CancellationToken);

        Assert.True(proceso.ExitCode == 0, $"ExitCode={proceso.ExitCode}\nOUT:\n{salida}\nERR:\n{error}");
        Assert.Contains("ROADMAP V1 IMPORTADO", salida);
        Assert.Contains("Evidence: 0", salida);

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            Assert.Equal(7, await contexto.Fases.CountAsync(f => f.UsuarioId == usuario.Id, CancellationToken));
            Assert.Equal(63, await contexto.Temas.CountAsync(t => t.UsuarioId == usuario.Id, CancellationToken));
            Assert.Equal(30, await contexto.Recursos.CountAsync(r => r.UsuarioId == usuario.Id, CancellationToken));
            await AssertNoEvidenceAsync(contexto, usuario.Id);
        }
    }

    private static ImportarRoadmapV1CasoUso CrearCasoUso(AprendizajeDbContext contexto) =>
        new(
            new UsuarioRepository(contexto),
            new FaseRepository(contexto),
            new TemaRepository(contexto),
            new HerramientaRepository(contexto),
            new CertificacionRepository(contexto),
            new RecursoRepository(contexto),
            contexto,
            new TransaccionAplicacion(contexto));

    private static Task<RoadmapV1Documento> LeerDocumentoAsync() =>
        new LectorRoadmapV1Json().LeerAsync(
            Path.Combine(ResolverRaizRepositorio(), "data", "roadmap", "roadmap-v1.json"),
            CancellationToken);

    private static async Task AssertNoEvidenceAsync(AprendizajeDbContext contexto, Guid usuarioId)
    {
        Assert.Equal(0, await contexto.Proyectos.CountAsync(p => p.UsuarioId == usuarioId, CancellationToken));
        Assert.Equal(0, await contexto.Laboratorios.CountAsync(l => l.UsuarioId == usuarioId, CancellationToken));
        Assert.Equal(0, await contexto.Writeups.CountAsync(w => w.UsuarioId == usuarioId, CancellationToken));
        Assert.Equal(0, await contexto.ArtefactosTecnicos.CountAsync(a => a.UsuarioId == usuarioId, CancellationToken));
        Assert.Equal(0, await contexto.CertificacionesObtenidas.CountAsync(c => c.UsuarioId == usuarioId, CancellationToken));
        Assert.Equal(0, await contexto.Notas.CountAsync(n => n.UsuarioId == usuarioId, CancellationToken));
        Assert.Equal(0, await contexto.SesionesEstudio.CountAsync(s => s.UsuarioId == usuarioId, CancellationToken));
        Assert.Equal(0, await contexto.EntradasBitacora.CountAsync(e => e.UsuarioId == usuarioId, CancellationToken));
    }

    private static async Task<int> ContarRecursoTemaAsync(AprendizajeDbContext contexto) =>
        await contexto.Database
            .SqlQueryRaw<int>("SELECT COUNT(*) AS [Value] FROM [resource].[RecursoTema]")
            .SingleAsync(CancellationToken);

    private static async Task<int> ContarCertificacionTemaAsync(AprendizajeDbContext contexto) =>
        await contexto.Database
            .SqlQueryRaw<int>("SELECT COUNT(*) AS [Value] FROM [roadmap].[CertificacionTema]")
            .SingleAsync(CancellationToken);

    private static async Task<int> ContarCompetenciaTemaAsync(AprendizajeDbContext contexto) =>
        await contexto.Database
            .SqlQueryRaw<int>("SELECT COUNT(*) AS [Value] FROM [roadmap].[CompetenciaTema]")
            .SingleAsync(CancellationToken);

    private static async Task<int> ContarTemaDependenciaAsync(AprendizajeDbContext contexto) =>
        await contexto.Database
            .SqlQueryRaw<int>("SELECT COUNT(*) AS [Value] FROM [roadmap].[TemaDependencia]")
            .SingleAsync(CancellationToken);

    private static string ResolverRaizRepositorio()
    {
        var directorio = new DirectoryInfo(AppContext.BaseDirectory);

        while (directorio is not null)
        {
            if (File.Exists(Path.Combine(directorio.FullName, "Aprendizaje.slnx")))
                return directorio.FullName;

            directorio = directorio.Parent;
        }

        throw new InvalidOperationException("No se pudo resolver la raíz del repositorio.");
    }

    private static string EmailUnico() => $"importador-roadmap-{Guid.CreateVersion7():N}@local.test";

    private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;
}
