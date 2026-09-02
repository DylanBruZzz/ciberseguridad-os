using System.Data.Common;
using Aprendizaje.Aplicacion.Evidence.Vistas.EvidenceListaV1;
using Aprendizaje.Aplicacion.Portafolio;
using Aprendizaje.Dominio.Evidence;
using Aprendizaje.Dominio.Nucleo;
using Aprendizaje.Dominio.Roadmap;
using Aprendizaje.Dominio.Study;
using Aprendizaje.Infraestructura.Persistencia;
using Aprendizaje.Infraestructura.Persistencia.Consultas;
using Aprendizaje.Infraestructura.Persistencia.Consultas.Evidence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Xunit;

namespace Aprendizaje.Tests.Integration.Persistencia;

[Collection(ColeccionPersistenciaSqlServer.Nombre)]
public sealed class EvidenceListaV1SqlServerTests
{
    [Fact]
    public async Task EvidenceLista_UsuarioSinEvidence_RetornaListaVacia()
    {
        await using var ambiente = await AmbientePersistenciaSqlServer.CrearAsync(CancellationToken);
        var usuario = Usuario.Registrar("Usuario sin evidence", EmailUnico());

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            contexto.Usuarios.Add(usuario);
            await contexto.GuardarCambiosAsync(CancellationToken);
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            var lista = await new ConsultaEvidenceListaV1(contexto).ObtenerAsync(
                new ObtenerEvidenceListaV1Solicitud(usuario.Id),
                CancellationToken);

            Assert.Equal(0, lista.Total);
            Assert.Empty(lista.Items);
        }
    }

    [Fact]
    public async Task EvidenceLista_UnItemDeCadaTipo_ProyectaCamposRelacionesYMadurez()
    {
        await using var ambiente = await AmbientePersistenciaSqlServer.CrearAsync(CancellationToken);
        var datos = await CrearDatasetEvidenceAsync(ambiente);

        await using var contexto = ambiente.CrearNuevoContexto();
        var lista = await new ConsultaEvidenceListaV1(contexto).ObtenerAsync(
            new ObtenerEvidenceListaV1Solicitud(datos.UsuarioA.Id),
            CancellationToken);

        Assert.Equal(5, lista.Total);
        Assert.Equal(5, lista.Items.Count);
        Assert.True(lista.Items.SequenceEqual(OrdenarComoReadModel(lista.Items)));
        Assert.All(lista.Items, item => Assert.True(item.FechaActividadUtc == (item.FechaModificacionUtc ?? item.FechaCreacionUtc)));

        var proyecto = Assert.Single(lista.Items, i => i.TipoEvidence == TipoEvidenceV1.Proyecto);
        Assert.Equal(datos.Proyecto.Id, proyecto.Id);
        Assert.Equal("Proyecto Evidence", proyecto.Titulo);
        Assert.Equal(EstadoMadurez.Borrador, proyecto.EstadoMadurez);
        Assert.Equal(new DateOnly(2026, 8, 10), proyecto.FechaReferencia);
        Assert.Equal(datos.TemaRedes.Id, Assert.Single(proyecto.Temas).Id);
        Assert.Equal(datos.HerramientaWireshark.Id, Assert.Single(proyecto.Herramientas).Id);

        var laboratorio = Assert.Single(lista.Items, i => i.TipoEvidence == TipoEvidenceV1.Laboratorio);
        Assert.Equal("Laboratorio Evidence", laboratorio.Titulo);
        Assert.Equal(EstadoMadurez.Documentado, laboratorio.EstadoMadurez);
        Assert.Equal(new DateOnly(2026, 8, 20), laboratorio.FechaReferencia);
        Assert.Equal(datos.TemaWeb.Id, Assert.Single(laboratorio.Temas).Id);
        Assert.Equal(datos.HerramientaBurp.Id, Assert.Single(laboratorio.Herramientas).Id);

        var writeup = Assert.Single(lista.Items, i => i.TipoEvidence == TipoEvidenceV1.Writeup);
        Assert.Equal("Writeup Evidence", writeup.Titulo);
        Assert.Equal(EstadoMadurez.ListoPortafolio, writeup.EstadoMadurez);
        Assert.Equal(new DateOnly(2026, 8, 25), writeup.FechaReferencia);
        Assert.Equal(datos.TemaRedes.Id, Assert.Single(writeup.Temas).Id);
        Assert.Empty(writeup.Herramientas);

        var artefacto = Assert.Single(lista.Items, i => i.TipoEvidence == TipoEvidenceV1.ArtefactoTecnico);
        Assert.Equal("Script Evidence", artefacto.Titulo);
        Assert.Equal(EstadoMadurez.Publicado, artefacto.EstadoMadurez);
        Assert.Null(artefacto.FechaReferencia);
        Assert.Equal(datos.TemaRedes.Id, Assert.Single(artefacto.Temas).Id);
        Assert.Equal(datos.HerramientaWireshark.Id, Assert.Single(artefacto.Herramientas).Id);

        var certificacion = Assert.Single(lista.Items, i => i.TipoEvidence == TipoEvidenceV1.CertificacionObtenida);
        Assert.Equal(datos.CertificacionObtenida.Id, certificacion.Id);
        Assert.Equal("Security+ Evidence", certificacion.Titulo);
        Assert.Equal(EstadoMadurez.Documentado, certificacion.EstadoMadurez);
        Assert.Equal(new DateOnly(2026, 8, 30), certificacion.FechaReferencia);
        Assert.Equal(datos.TemaWeb.Id, Assert.Single(certificacion.Temas).Id);
        Assert.Empty(certificacion.Herramientas);
    }

    [Fact]
    public async Task EvidenceLista_FiltrosTipoMadurezTemaYCombinados_RetornanSoloCoincidencias()
    {
        await using var ambiente = await AmbientePersistenciaSqlServer.CrearAsync(CancellationToken);
        var datos = await CrearDatasetEvidenceAsync(ambiente);

        await using var contexto = ambiente.CrearNuevoContexto();
        var consulta = new ConsultaEvidenceListaV1(contexto);

        var soloWriteups = await consulta.ObtenerAsync(
            new ObtenerEvidenceListaV1Solicitud(datos.UsuarioA.Id, TipoEvidenceV1.Writeup),
            CancellationToken);
        var documentados = await consulta.ObtenerAsync(
            new ObtenerEvidenceListaV1Solicitud(datos.UsuarioA.Id, EstadoMadurez: EstadoMadurez.Documentado),
            CancellationToken);
        var porTemaRedes = await consulta.ObtenerAsync(
            new ObtenerEvidenceListaV1Solicitud(datos.UsuarioA.Id, TemaId: datos.TemaRedes.Id),
            CancellationToken);
        var certificacionDocumentadaPorTema = await consulta.ObtenerAsync(
            new ObtenerEvidenceListaV1Solicitud(
                datos.UsuarioA.Id,
                TipoEvidenceV1.CertificacionObtenida,
                EstadoMadurez.Documentado,
                datos.TemaWeb.Id),
            CancellationToken);

        Assert.Collection(
            soloWriteups.Items,
            item => Assert.Equal(TipoEvidenceV1.Writeup, item.TipoEvidence));
        Assert.Equal(2, documentados.Total);
        Assert.All(documentados.Items, item => Assert.Equal(EstadoMadurez.Documentado, item.EstadoMadurez));
        Assert.Equal(
            [TipoEvidenceV1.Proyecto, TipoEvidenceV1.Writeup, TipoEvidenceV1.ArtefactoTecnico],
            porTemaRedes.Items.Select(i => i.TipoEvidence).Order().ToArray());
        Assert.DoesNotContain(porTemaRedes.Items, i => i.Id == datos.CertificacionObtenida.Id);
        var certificacion = Assert.Single(certificacionDocumentadaPorTema.Items);
        Assert.Equal(datos.CertificacionObtenida.Id, certificacion.Id);
        Assert.Equal(TipoEvidenceV1.CertificacionObtenida, certificacion.TipoEvidence);
    }

    [Fact]
    public async Task EvidenceLista_SoftDeletedYOwnership_NoMezclaDatos()
    {
        await using var ambiente = await AmbientePersistenciaSqlServer.CrearAsync(CancellationToken);
        var datos = await CrearDatasetEvidenceAsync(ambiente);

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            var lista = await new ConsultaEvidenceListaV1(contexto).ObtenerAsync(
                new ObtenerEvidenceListaV1Solicitud(datos.UsuarioA.Id),
                CancellationToken);

            Assert.Equal(5, lista.Total);
            Assert.DoesNotContain(lista.Items, item => item.Id == datos.ProyectoOtroUsuario.Id);
            Assert.DoesNotContain(lista.Items, item => item.Id == datos.ProyectoEliminado.Id);
        }
    }

    [Fact]
    public async Task EvidenceLista_PortfolioMantieneFiltroPropioDeMadurez()
    {
        await using var ambiente = await AmbientePersistenciaSqlServer.CrearAsync(CancellationToken);
        var datos = await CrearDatasetEvidenceAsync(ambiente);

        await using var contexto = ambiente.CrearNuevoContexto();
        var evidence = await new ConsultaEvidenceListaV1(contexto).ObtenerAsync(
            new ObtenerEvidenceListaV1Solicitud(datos.UsuarioA.Id),
            CancellationToken);
        var portafolio = await new ConsultaPortafolio(contexto).ObtenerAsync(
            new ObtenerPortafolioSolicitud(datos.UsuarioA.Id),
            CancellationToken);

        Assert.Equal(5, evidence.Total);
        Assert.Contains(evidence.Items, i => i.EstadoMadurez == EstadoMadurez.Borrador);
        Assert.Contains(evidence.Items, i => i.EstadoMadurez == EstadoMadurez.Documentado);
        Assert.Equal(2, portafolio.Resumen.Total);
        Assert.Equal(1, portafolio.Resumen.Writeups);
        Assert.Equal(1, portafolio.Resumen.ArtefactosTecnicos);
        Assert.DoesNotContain(portafolio.Proyectos, p => p.ProyectoId == datos.Proyecto.Id);
        Assert.Empty(portafolio.Laboratorios);
        Assert.Empty(portafolio.CertificacionesObtenidas);
    }

    [Fact]
    public async Task EvidenceLista_ConsultaUsaNumeroConstanteDeComandos()
    {
        await using var ambiente = await AmbientePersistenciaSqlServer.CrearAsync(CancellationToken);
        var datos = await CrearDatasetEvidenceAsync(ambiente);
        var contador = new ContadorComandosInterceptor();

        await using (var contexto = CrearContextoContado(contador))
        {
            var lista = await new ConsultaEvidenceListaV1(contexto).ObtenerAsync(
                new ObtenerEvidenceListaV1Solicitud(datos.UsuarioA.Id),
                CancellationToken);

            Assert.Equal(5, lista.Total);
        }

        Assert.True(contador.ComandosLectura <= 13, $"Se esperaban como maximo 13 comandos; se ejecutaron {contador.ComandosLectura}.");
    }

    private static async Task<DatosEvidence> CrearDatasetEvidenceAsync(AmbientePersistenciaSqlServer ambiente)
    {
        var usuarioA = Usuario.Registrar("Usuario Evidence A", EmailUnico());
        var usuarioB = Usuario.Registrar("Usuario Evidence B", EmailUnico());
        var temaRedes = Tema.Crear(usuarioA.Id, "Redes", TipoConocimiento.Conceptual);
        var temaWeb = Tema.Crear(usuarioA.Id, "Web", TipoConocimiento.Procedimental);
        var temaAjeno = Tema.Crear(usuarioB.Id, "Ajeno", TipoConocimiento.Conceptual);
        var herramientaWireshark = Herramienta.Crear($"Wireshark Evidence {Guid.CreateVersion7():N}");
        var herramientaBurp = Herramienta.Crear($"Burp Evidence {Guid.CreateVersion7():N}");
        var certificacion = Certificacion.Crear("Security+ Evidence", TipoCosto.Pago);

        var proyecto = Proyecto.Crear(usuarioA.Id, "Proyecto Evidence");
        proyecto.ActualizarFechas(new DateOnly(2026, 8, 1), new DateOnly(2026, 8, 10));

        var laboratorio = Laboratorio.Crear(usuarioA.Id, "Laboratorio Evidence");
        laboratorio.ActualizarFecha(new DateOnly(2026, 8, 20));
        laboratorio.AvanzarMadurez(EstadoMadurez.Documentado);

        var writeup = Writeup.Crear(usuarioA.Id, "Writeup Evidence");
        writeup.ActualizarFecha(new DateOnly(2026, 8, 25));
        writeup.AvanzarMadurez(EstadoMadurez.ListoPortafolio);

        var artefacto = ArtefactoTecnico.Crear(usuarioA.Id, TipoArtefacto.Script, "Script Evidence");
        artefacto.AvanzarMadurez(EstadoMadurez.Publicado);

        var certificacionObtenida = CertificacionObtenida.Registrar(
            usuarioA.Id,
            certificacion.Id,
            new DateOnly(2026, 8, 30));
        certificacionObtenida.AvanzarMadurez(EstadoMadurez.Documentado);

        var proyectoOtroUsuario = Proyecto.Crear(usuarioB.Id, "Proyecto otro usuario");
        proyectoOtroUsuario.AvanzarMadurez(EstadoMadurez.Publicado);
        var proyectoEliminado = Proyecto.Crear(usuarioA.Id, "Proyecto eliminado");
        proyectoEliminado.AvanzarMadurez(EstadoMadurez.Publicado);
        proyectoEliminado.MarcarComoEliminado();

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            contexto.Usuarios.AddRange(usuarioA, usuarioB);
            contexto.Temas.AddRange(temaRedes, temaWeb, temaAjeno);
            contexto.Herramientas.AddRange(herramientaWireshark, herramientaBurp);
            contexto.Certificaciones.Add(certificacion);
            contexto.Proyectos.AddRange(proyecto, proyectoOtroUsuario, proyectoEliminado);
            contexto.Laboratorios.Add(laboratorio);
            contexto.Writeups.Add(writeup);
            contexto.ArtefactosTecnicos.Add(artefacto);
            contexto.CertificacionesObtenidas.Add(certificacionObtenida);
            await contexto.GuardarCambiosAsync(CancellationToken);

            await VincularProyectoTemaAsync(contexto, proyecto.Id, temaRedes.Id);
            await VincularProyectoHerramientaAsync(contexto, proyecto.Id, herramientaWireshark.Id);
            await VincularProyectoTemaAsync(contexto, proyectoOtroUsuario.Id, temaAjeno.Id);
            await VincularProyectoTemaAsync(contexto, proyectoEliminado.Id, temaRedes.Id);

            await VincularLaboratorioTemaAsync(contexto, laboratorio.Id, temaWeb.Id);
            await VincularLaboratorioHerramientaAsync(contexto, laboratorio.Id, herramientaBurp.Id);

            await VincularWriteupTemaAsync(contexto, writeup.Id, temaRedes.Id);

            await VincularArtefactoTemaAsync(contexto, artefacto.Id, temaRedes.Id);
            await VincularArtefactoHerramientaAsync(contexto, artefacto.Id, herramientaWireshark.Id);

            await VincularCertificacionTemaAsync(contexto, certificacion.Id, temaWeb.Id);
        }

        return new DatosEvidence(
            usuarioA,
            usuarioB,
            temaRedes,
            temaWeb,
            herramientaWireshark,
            herramientaBurp,
            certificacion,
            proyecto,
            laboratorio,
            writeup,
            artefacto,
            certificacionObtenida,
            proyectoOtroUsuario,
            proyectoEliminado);
    }

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

    private static async Task VincularProyectoHerramientaAsync(AprendizajeDbContext contexto, Guid proyectoId, Guid herramientaId) =>
        await contexto.Database.ExecuteSqlInterpolatedAsync(
            $"INSERT INTO [evidence].[ProyectoHerramienta] ([ProyectoId], [HerramientaId]) VALUES ({proyectoId}, {herramientaId})",
            CancellationToken);

    private static async Task VincularLaboratorioHerramientaAsync(AprendizajeDbContext contexto, Guid laboratorioId, Guid herramientaId) =>
        await contexto.Database.ExecuteSqlInterpolatedAsync(
            $"INSERT INTO [evidence].[LaboratorioHerramienta] ([LaboratorioId], [HerramientaId]) VALUES ({laboratorioId}, {herramientaId})",
            CancellationToken);

    private static async Task VincularArtefactoHerramientaAsync(AprendizajeDbContext contexto, Guid artefactoId, Guid herramientaId) =>
        await contexto.Database.ExecuteSqlInterpolatedAsync(
            $"INSERT INTO [evidence].[ArtefactoHerramienta] ([ArtefactoTecnicoId], [HerramientaId]) VALUES ({artefactoId}, {herramientaId})",
            CancellationToken);

    private static IEnumerable<EvidenceItemV1Dto> OrdenarComoReadModel(IEnumerable<EvidenceItemV1Dto> items) =>
        items.OrderByDescending(i => i.FechaActividadUtc)
            .ThenBy(i => i.TipoEvidence)
            .ThenBy(i => i.Titulo)
            .ThenBy(i => i.Id);

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

    private static string EmailUnico() => $"evidence-lista-sql-{Guid.CreateVersion7():N}@local.test";

    private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

    private sealed record DatosEvidence(
        Usuario UsuarioA,
        Usuario UsuarioB,
        Tema TemaRedes,
        Tema TemaWeb,
        Herramienta HerramientaWireshark,
        Herramienta HerramientaBurp,
        Certificacion Certificacion,
        Proyecto Proyecto,
        Laboratorio Laboratorio,
        Writeup Writeup,
        ArtefactoTecnico Artefacto,
        CertificacionObtenida CertificacionObtenida,
        Proyecto ProyectoOtroUsuario,
        Proyecto ProyectoEliminado);
}
