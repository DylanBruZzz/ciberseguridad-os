using Aprendizaje.Aplicacion.Resource.Recursos.ActualizarRecurso;
using Aprendizaje.Aplicacion.Resource.Recursos.EliminarRecurso;
using Aprendizaje.Aplicacion.Resource.Recursos.VincularRecursoATema;
using Aprendizaje.Dominio.Nucleo;
using Aprendizaje.Dominio.Resource;
using Aprendizaje.Dominio.Roadmap;
using Aprendizaje.Infraestructura.Persistencia;
using Aprendizaje.Infraestructura.Persistencia.Repositorios;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Aprendizaje.Tests.Integration.Persistencia;

[Collection(ColeccionPersistenciaSqlServer.Nombre)]
public sealed class RecursoEditableSqlServerTests
{
    [Fact]
    public async Task RecursoEditable_ActualizacionPersisteYPreservaVinculoTema()
    {
        await using var ambiente = await AmbientePersistenciaSqlServer.CrearAsync(CancellationToken);
        var usuario = Usuario.Registrar("Usuario Resource", EmailUnico());
        var tema = Tema.Crear(usuario.Id, "Modelo OSI / TCP-IP", TipoConocimiento.Conceptual);
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
            await new VincularRecursoATemaCasoUso(new RecursoRepository(contexto), new TemaRepository(contexto), contexto)
                .EjecutarAsync(new VincularRecursoATemaSolicitud(recurso.Id, tema.Id), CancellationToken);

            var casoUso = CrearCasoActualizar(contexto);

            var resultado = await casoUso.EjecutarAsync(new ActualizarRecursoSolicitud(
                recurso.Id,
                usuario.Id,
                " Curso redes actualizado ",
                "https://example.local/recurso",
                EstadoRecurso.Consultado,
                5,
                "Notas actualizadas",
                "ChatGPT",
                "Prompt documentado"), CancellationToken);

            Assert.Equal(ActualizarRecursoEstado.Actualizado, resultado.Estado);
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            var persistido = await contexto.Recursos
                .AsNoTracking()
                .SingleAsync(r => r.Id == recurso.Id, CancellationToken);
            var vinculos = await ContarVinculosTemaAsync(contexto, recurso.Id, tema.Id);

            Assert.Equal("Curso redes actualizado", persistido.Titulo);
            Assert.Equal(TipoRecurso.Documentacion, persistido.Tipo);
            Assert.Equal("https://example.local/recurso", persistido.Url);
            Assert.Equal(EstadoRecurso.Consultado, persistido.Estado);
            Assert.Equal(5, persistido.Rating?.Valor);
            Assert.Equal("Notas actualizadas", persistido.Notas);
            Assert.Equal("ChatGPT", persistido.HerramientaIA);
            Assert.Equal("Prompt documentado", persistido.PromptsUtilizados);
            Assert.Equal(1, vinculos);
        }
    }

    [Fact]
    public async Task RecursoEditable_SoftDeleteOcultaConsultaNormalYMantieneFilaFisica()
    {
        await using var ambiente = await AmbientePersistenciaSqlServer.CrearAsync(CancellationToken);
        var usuario = Usuario.Registrar("Usuario Resource", EmailUnico());
        var recurso = Recurso.Guardar(usuario.Id, TipoRecurso.Curso, "Curso para eliminar");

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            contexto.Usuarios.Add(usuario);
            contexto.Recursos.Add(recurso);
            await contexto.GuardarCambiosAsync(CancellationToken);
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            var resultado = await CrearCasoEliminar(contexto).EjecutarAsync(
                new EliminarRecursoSolicitud(recurso.Id, usuario.Id),
                CancellationToken);

            Assert.Equal(EliminarRecursoEstado.Eliminado, resultado.Estado);
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            var consultaNormal = await contexto.Recursos.SingleOrDefaultAsync(r => r.Id == recurso.Id, CancellationToken);
            var consultaSinFiltro = await contexto.Recursos
                .IgnoreQueryFilters()
                .SingleOrDefaultAsync(r => r.Id == recurso.Id, CancellationToken);

            Assert.Null(consultaNormal);
            Assert.NotNull(consultaSinFiltro);
            Assert.NotNull(consultaSinFiltro.FechaEliminacionUtc);
        }
    }

    [Fact]
    public async Task RecursoEditable_OwnershipIncorrectoNoActualizaNiElimina()
    {
        await using var ambiente = await AmbientePersistenciaSqlServer.CrearAsync(CancellationToken);
        var usuarioA = Usuario.Registrar("Usuario A", EmailUnico());
        var usuarioB = Usuario.Registrar("Usuario B", EmailUnico());
        var recurso = Recurso.Guardar(usuarioA.Id, TipoRecurso.Documentacion, "Recurso original");

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            contexto.Usuarios.AddRange(usuarioA, usuarioB);
            contexto.Recursos.Add(recurso);
            await contexto.GuardarCambiosAsync(CancellationToken);
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            var actualizar = await CrearCasoActualizar(contexto).EjecutarAsync(new ActualizarRecursoSolicitud(
                recurso.Id,
                usuarioB.Id,
                "Recurso alterado",
                "https://example.local/otro",
                EstadoRecurso.EnUso,
                3,
                "No debe persistir",
                null,
                null), CancellationToken);

            var eliminar = await CrearCasoEliminar(contexto).EjecutarAsync(
                new EliminarRecursoSolicitud(recurso.Id, usuarioB.Id),
                CancellationToken);

            Assert.Equal(ActualizarRecursoEstado.UsuarioNoCoincide, actualizar.Estado);
            Assert.Equal(EliminarRecursoEstado.UsuarioNoCoincide, eliminar.Estado);
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            var persistido = await contexto.Recursos
                .AsNoTracking()
                .SingleAsync(r => r.Id == recurso.Id, CancellationToken);

            Assert.Equal("Recurso original", persistido.Titulo);
            Assert.Null(persistido.Url);
            Assert.Equal(EstadoRecurso.PorClasificar, persistido.Estado);
            Assert.Null(persistido.Rating);
            Assert.Null(persistido.Notas);
            Assert.Null(persistido.FechaEliminacionUtc);
        }
    }

    private static ActualizarRecursoCasoUso CrearCasoActualizar(AprendizajeDbContext contexto) =>
        new(new RecursoRepository(contexto), new UsuarioRepository(contexto), contexto);

    private static EliminarRecursoCasoUso CrearCasoEliminar(AprendizajeDbContext contexto) =>
        new(new RecursoRepository(contexto), new UsuarioRepository(contexto), contexto);

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

    private static string EmailUnico() => $"resource-editable-{Guid.CreateVersion7():N}@local.test";

    private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;
}
