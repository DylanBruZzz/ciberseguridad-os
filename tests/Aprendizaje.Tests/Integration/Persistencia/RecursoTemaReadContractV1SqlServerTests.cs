using System.Data.Common;
using Aprendizaje.Aplicacion.Resource.Recursos.ListarRecursos;
using Aprendizaje.Aplicacion.Resource.Recursos.VincularRecursoATema;
using Aprendizaje.Dominio.Nucleo;
using Aprendizaje.Dominio.Resource;
using Aprendizaje.Dominio.Roadmap;
using Aprendizaje.Infraestructura.Persistencia;
using Aprendizaje.Infraestructura.Persistencia.Consultas.Resource;
using Aprendizaje.Infraestructura.Persistencia.Repositorios;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Xunit;

namespace Aprendizaje.Tests.Integration.Persistencia;

[Collection(ColeccionPersistenciaSqlServer.Nombre)]
public sealed class RecursoTemaReadContractV1SqlServerTests
{
    [Fact]
    public async Task RecursosReadContract_ListGlobalExponeTemasYRespetaOwnershipYSoftDelete()
    {
        await using var ambiente = await AmbientePersistenciaSqlServer.CrearAsync(CancellationToken);
        var datos = await CrearDatasetRecursosAsync(ambiente);

        await using var contexto = ambiente.CrearNuevoContexto();
        var lista = await new ConsultaRecursosV1(contexto).ListarAsync(
            new ListarRecursosSolicitud(datos.UsuarioA.Id),
            CancellationToken);

        Assert.Equal(
            ["A recurso sin temas", "B recurso con un tema", "C recurso con varios temas"],
            lista.Select(r => r.Titulo).ToArray());
        Assert.Empty(lista.Single(r => r.Id == datos.RecursoSinTemas.Id).Temas);
        Assert.Equal(datos.TemaRedes.Id, Assert.Single(lista.Single(r => r.Id == datos.RecursoUnTema.Id).Temas).Id);

        var temasMultiples = lista.Single(r => r.Id == datos.RecursoVariosTemas.Id).Temas;
        Assert.Equal(
            new[] { datos.TemaRedes.Id, datos.TemaWeb.Id }.OrderBy(id => id).ToArray(),
            temasMultiples.Select(t => t.Id).OrderBy(id => id).ToArray());
        Assert.DoesNotContain(temasMultiples, t => t.Id == datos.TemaEliminado.Id);
        Assert.DoesNotContain(lista, r => r.Id == datos.RecursoOtroUsuario.Id);
        Assert.DoesNotContain(lista, r => r.Id == datos.RecursoEliminado.Id);
    }

    [Fact]
    public async Task RecursosReadContract_FiltroTemaIdDevuelveSoloRecursosRelacionadosDelUsuario()
    {
        await using var ambiente = await AmbientePersistenciaSqlServer.CrearAsync(CancellationToken);
        var datos = await CrearDatasetRecursosAsync(ambiente);

        await using var contexto = ambiente.CrearNuevoContexto();
        var consulta = new ConsultaRecursosV1(contexto);

        var porRedes = await consulta.ListarAsync(
            new ListarRecursosSolicitud(datos.UsuarioA.Id, datos.TemaRedes.Id),
            CancellationToken);
        var porWeb = await consulta.ListarAsync(
            new ListarRecursosSolicitud(datos.UsuarioA.Id, datos.TemaWeb.Id),
            CancellationToken);
        var porTemaAjeno = await consulta.ListarAsync(
            new ListarRecursosSolicitud(datos.UsuarioA.Id, datos.TemaAjeno.Id),
            CancellationToken);
        var porTemaInexistente = await consulta.ListarAsync(
            new ListarRecursosSolicitud(datos.UsuarioA.Id, Guid.CreateVersion7()),
            CancellationToken);

        Assert.Equal(
            new[] { datos.RecursoUnTema.Id, datos.RecursoVariosTemas.Id }.OrderBy(id => id).ToArray(),
            porRedes.Select(r => r.Id).OrderBy(id => id).ToArray());
        Assert.Equal(datos.RecursoVariosTemas.Id, Assert.Single(porWeb).Id);
        Assert.Empty(porTemaAjeno);
        Assert.Empty(porTemaInexistente);
    }

    [Fact]
    public async Task RecursosReadContract_DetailExponeTemasRelacionadosSinNMasUno()
    {
        await using var ambiente = await AmbientePersistenciaSqlServer.CrearAsync(CancellationToken);
        var datos = await CrearDatasetRecursosAsync(ambiente);

        await using var contexto = ambiente.CrearNuevoContexto();
        var detalle = await new ConsultaRecursosV1(contexto).ObtenerDetalleAsync(
            datos.RecursoVariosTemas.Id,
            CancellationToken);

        Assert.NotNull(detalle);
        Assert.Equal(datos.RecursoVariosTemas.Id, detalle.Id);
        Assert.Equal(
            new[] { datos.TemaRedes.Id, datos.TemaWeb.Id }.OrderBy(id => id).ToArray(),
            detalle.Temas.Select(t => t.Id).OrderBy(id => id).ToArray());
        Assert.DoesNotContain(detalle.Temas, t => t.Id == datos.TemaEliminado.Id);
    }

    [Fact]
    public async Task RecursosReadContract_LinkWriteExistenteSigueIdempotenteYVisibleEnLectura()
    {
        await using var ambiente = await AmbientePersistenciaSqlServer.CrearAsync(CancellationToken);
        var usuario = Usuario.Registrar("Usuario link recurso", EmailUnico());
        var tema = Tema.Crear(usuario.Id, "Tema link", TipoConocimiento.Conceptual);
        var recurso = Recurso.Guardar(usuario.Id, TipoRecurso.Documentacion, "Recurso link");

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

            var primero = await casoUso.EjecutarAsync(
                new VincularRecursoATemaSolicitud(recurso.Id, tema.Id),
                CancellationToken);
            var segundo = await casoUso.EjecutarAsync(
                new VincularRecursoATemaSolicitud(recurso.Id, tema.Id),
                CancellationToken);

            Assert.Equal(VincularRecursoATemaEstado.Actualizado, primero.Estado);
            Assert.Equal(VincularRecursoATemaEstado.Actualizado, segundo.Estado);
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            var vinculos = await ContarVinculosTemaAsync(contexto, recurso.Id, tema.Id);
            var detalle = await new ConsultaRecursosV1(contexto).ObtenerDetalleAsync(recurso.Id, CancellationToken);

            Assert.Equal(1, vinculos);
            Assert.NotNull(detalle);
            Assert.Equal(tema.Id, Assert.Single(detalle.Temas).Id);
        }
    }

    [Fact]
    public async Task RecursosReadContract_ListUsaNumeroConstanteDeComandos()
    {
        await using var ambiente = await AmbientePersistenciaSqlServer.CrearAsync(CancellationToken);
        var datos = await CrearDatasetRecursosAsync(ambiente);
        var contador = new ContadorComandosInterceptor();

        await using (var contexto = CrearContextoContado(contador))
        {
            var lista = await new ConsultaRecursosV1(contexto).ListarAsync(
                new ListarRecursosSolicitud(datos.UsuarioA.Id),
                CancellationToken);

            Assert.Equal(3, lista.Count);
        }

        Assert.True(contador.ComandosLectura <= 2, $"Se esperaban como maximo 2 comandos; se ejecutaron {contador.ComandosLectura}.");
    }

    private static async Task<DatosRecursos> CrearDatasetRecursosAsync(AmbientePersistenciaSqlServer ambiente)
    {
        var usuarioA = Usuario.Registrar("Usuario Recursos A", EmailUnico());
        var usuarioB = Usuario.Registrar("Usuario Recursos B", EmailUnico());
        var temaRedes = Tema.Crear(usuarioA.Id, "Redes", TipoConocimiento.Conceptual);
        var temaWeb = Tema.Crear(usuarioA.Id, "Web", TipoConocimiento.Procedimental);
        var temaEliminado = Tema.Crear(usuarioA.Id, "Tema eliminado", TipoConocimiento.Conceptual);
        temaEliminado.MarcarComoEliminado();
        var temaAjeno = Tema.Crear(usuarioB.Id, "Tema ajeno", TipoConocimiento.Conceptual);

        var recursoSinTemas = Recurso.Guardar(usuarioA.Id, TipoRecurso.Documentacion, "A recurso sin temas");
        var recursoUnTema = Recurso.Guardar(usuarioA.Id, TipoRecurso.Video, "B recurso con un tema");
        var recursoVariosTemas = Recurso.Guardar(usuarioA.Id, TipoRecurso.Curso, "C recurso con varios temas");
        var recursoOtroUsuario = Recurso.Guardar(usuarioB.Id, TipoRecurso.Documentacion, "D recurso otro usuario");
        var recursoEliminado = Recurso.Guardar(usuarioA.Id, TipoRecurso.Libro, "E recurso eliminado");
        recursoEliminado.MarcarComoEliminado();

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            contexto.Usuarios.AddRange(usuarioA, usuarioB);
            contexto.Temas.AddRange(temaRedes, temaWeb, temaEliminado, temaAjeno);
            contexto.Recursos.AddRange(
                recursoSinTemas,
                recursoUnTema,
                recursoVariosTemas,
                recursoOtroUsuario,
                recursoEliminado);
            await contexto.GuardarCambiosAsync(CancellationToken);

            await VincularRecursoTemaAsync(contexto, recursoUnTema.Id, temaRedes.Id);
            await VincularRecursoTemaAsync(contexto, recursoVariosTemas.Id, temaRedes.Id);
            await VincularRecursoTemaAsync(contexto, recursoVariosTemas.Id, temaWeb.Id);
            await VincularRecursoTemaAsync(contexto, recursoVariosTemas.Id, temaEliminado.Id);
            await VincularRecursoTemaAsync(contexto, recursoOtroUsuario.Id, temaAjeno.Id);
            await VincularRecursoTemaAsync(contexto, recursoEliminado.Id, temaRedes.Id);
        }

        return new DatosRecursos(
            usuarioA,
            usuarioB,
            temaRedes,
            temaWeb,
            temaEliminado,
            temaAjeno,
            recursoSinTemas,
            recursoUnTema,
            recursoVariosTemas,
            recursoOtroUsuario,
            recursoEliminado);
    }

    private static async Task VincularRecursoTemaAsync(AprendizajeDbContext contexto, Guid recursoId, Guid temaId) =>
        await contexto.Database.ExecuteSqlInterpolatedAsync(
            $"INSERT INTO [resource].[RecursoTema] ([RecursoId], [TemaId]) VALUES ({recursoId}, {temaId})",
            CancellationToken);

    private static async Task<int> ContarVinculosTemaAsync(
        AprendizajeDbContext contexto,
        Guid recursoId,
        Guid temaId) =>
        await contexto.Database
            .SqlQueryRaw<int>(
                "SELECT COUNT(*) AS [Value] FROM [resource].[RecursoTema] WHERE [RecursoId] = {0} AND [TemaId] = {1}",
                recursoId,
                temaId)
            .SingleAsync(CancellationToken);

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

    private static string EmailUnico() => $"recurso-read-contract-{Guid.CreateVersion7():N}@local.test";

    private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

    private sealed record DatosRecursos(
        Usuario UsuarioA,
        Usuario UsuarioB,
        Tema TemaRedes,
        Tema TemaWeb,
        Tema TemaEliminado,
        Tema TemaAjeno,
        Recurso RecursoSinTemas,
        Recurso RecursoUnTema,
        Recurso RecursoVariosTemas,
        Recurso RecursoOtroUsuario,
        Recurso RecursoEliminado);
}
