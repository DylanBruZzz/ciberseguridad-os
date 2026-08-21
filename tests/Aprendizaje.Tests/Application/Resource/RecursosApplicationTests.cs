using Aprendizaje.Aplicacion.Resource.Recursos.CrearRecurso;
using Aprendizaje.Aplicacion.Resource.Recursos.ListarRecursos;
using Aprendizaje.Aplicacion.Resource.Recursos.ObtenerRecursoPorId;
using Aprendizaje.Aplicacion.Resource.Recursos.VincularRecursoATema;
using Aprendizaje.Dominio.Resource;
using Aprendizaje.Dominio.Roadmap;
using Aprendizaje.Tests.Soporte;
using Xunit;

namespace Aprendizaje.Tests.Application.Resource;

public sealed class RecursosApplicationTests
{
    [Fact]
    public async Task CrearRecurso_DebeAgregarRecursoYGuardar()
    {
        var recursos = new FakeRecursoRepository();
        var unitOfWork = new FakeUnitOfWork();
        var casoUso = new CrearRecursoCasoUso(recursos, unitOfWork);
        var usuarioId = Guid.CreateVersion7();

        var resultado = await casoUso.EjecutarAsync(new CrearRecursoSolicitud(
            usuarioId,
            TipoRecurso.Documentacion,
            " Documentación modelo OSI ",
            null), CancellationToken);

        Assert.NotEqual(Guid.Empty, resultado.Id);
        Assert.Equal(usuarioId, resultado.UsuarioId);
        Assert.Equal(TipoRecurso.Documentacion, resultado.Tipo);
        Assert.Equal("Documentación modelo OSI", resultado.Titulo);
        Assert.Null(resultado.Url);
        Assert.Equal(EstadoRecurso.PorClasificar, resultado.Estado);
        Assert.NotNull(await recursos.ObtenerPorIdAsync(resultado.Id, CancellationToken));
        Assert.Equal(1, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task ObtenerRecursoPorId_DebeRetornarRecursoExistente()
    {
        var recursos = new FakeRecursoRepository();
        var recurso = CrearRecurso();
        recursos.Agregar(recurso);
        var casoUso = new ObtenerRecursoPorIdCasoUso(recursos);

        var resultado = await casoUso.EjecutarAsync(recurso.Id, CancellationToken);

        Assert.True(resultado.Encontrado);
        Assert.NotNull(resultado.Recurso);
        Assert.Equal(recurso.Id, resultado.Recurso.Id);
        Assert.Equal(recurso.Titulo, resultado.Recurso.Titulo);
    }

    [Fact]
    public async Task ObtenerRecursoPorId_DebeRetornarNoEncontrado()
    {
        var recursos = new FakeRecursoRepository();
        var casoUso = new ObtenerRecursoPorIdCasoUso(recursos);

        var resultado = await casoUso.EjecutarAsync(Guid.CreateVersion7(), CancellationToken);

        Assert.False(resultado.Encontrado);
        Assert.Null(resultado.Recurso);
    }

    [Fact]
    public async Task ListarRecursos_DebeMapearColeccionDelRepository()
    {
        var recursos = new FakeRecursoRepository();
        var usuarioId = Guid.CreateVersion7();
        var recurso = CrearRecurso(usuarioId);
        recursos.Agregar(recurso);
        recursos.Agregar(CrearRecurso(Guid.CreateVersion7()));
        var casoUso = new ListarRecursosCasoUso(recursos);

        var resultado = await casoUso.EjecutarAsync(new ListarRecursosSolicitud(usuarioId), CancellationToken);

        var resumen = Assert.Single(resultado.Recursos);
        Assert.Equal(recurso.Id, resumen.Id);
        Assert.Equal(usuarioId, resumen.UsuarioId);
        Assert.Equal(recurso.Titulo, resumen.Titulo);
    }

    [Fact]
    public async Task VincularRecursoATema_DebeRetornarRecursoNoEncontradoSinGuardar()
    {
        var recursos = new FakeRecursoRepository();
        var temas = new FakeTemaRepository();
        var unitOfWork = new FakeUnitOfWork();
        var casoUso = new VincularRecursoATemaCasoUso(recursos, temas, unitOfWork);

        var resultado = await casoUso.EjecutarAsync(new VincularRecursoATemaSolicitud(
            Guid.CreateVersion7(),
            Guid.CreateVersion7()), CancellationToken);

        Assert.Equal(VincularRecursoATemaEstado.RecursoNoEncontrado, resultado.Estado);
        Assert.Equal(0, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task VincularRecursoATema_DebeRetornarTemaNoEncontradoSinGuardar()
    {
        var recursos = new FakeRecursoRepository();
        var temas = new FakeTemaRepository();
        var unitOfWork = new FakeUnitOfWork();
        var recurso = CrearRecurso();
        recursos.Agregar(recurso);
        var casoUso = new VincularRecursoATemaCasoUso(recursos, temas, unitOfWork);

        var resultado = await casoUso.EjecutarAsync(new VincularRecursoATemaSolicitud(
            recurso.Id,
            Guid.CreateVersion7()), CancellationToken);

        Assert.Equal(VincularRecursoATemaEstado.TemaNoEncontrado, resultado.Estado);
        Assert.Equal(0, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task VincularRecursoATema_DebeRetornarUsuarioNoCoincideSinGuardar()
    {
        var recursos = new FakeRecursoRepository();
        var temas = new FakeTemaRepository();
        var unitOfWork = new FakeUnitOfWork();
        var recurso = CrearRecurso();
        var tema = Tema.Crear(Guid.CreateVersion7(), "Modelo OSI", TipoConocimiento.Conceptual);
        recursos.Agregar(recurso);
        temas.Agregar(tema);
        var casoUso = new VincularRecursoATemaCasoUso(recursos, temas, unitOfWork);

        var resultado = await casoUso.EjecutarAsync(new VincularRecursoATemaSolicitud(recurso.Id, tema.Id), CancellationToken);

        Assert.Equal(VincularRecursoATemaEstado.UsuarioNoCoincide, resultado.Estado);
        Assert.Equal(0, recursos.VincularTemaLlamadas);
        Assert.Equal(0, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task VincularRecursoATema_DebeVincularYGuardar()
    {
        var recursos = new FakeRecursoRepository();
        var temas = new FakeTemaRepository();
        var unitOfWork = new FakeUnitOfWork();
        var usuarioId = Guid.CreateVersion7();
        var recurso = CrearRecurso(usuarioId);
        var tema = Tema.Crear(usuarioId, "Modelo OSI", TipoConocimiento.Conceptual);
        recursos.Agregar(recurso);
        temas.Agregar(tema);
        var casoUso = new VincularRecursoATemaCasoUso(recursos, temas, unitOfWork);

        var resultado = await casoUso.EjecutarAsync(new VincularRecursoATemaSolicitud(recurso.Id, tema.Id), CancellationToken);

        Assert.Equal(VincularRecursoATemaEstado.Actualizado, resultado.Estado);
        Assert.True(await recursos.ExisteVinculoTemaAsync(recurso.Id, tema.Id, CancellationToken));
        Assert.Equal(1, recursos.VincularTemaLlamadas);
        Assert.Equal(1, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task VincularRecursoATema_DebeSerIdempotenteSinGuardarDeNuevo()
    {
        var recursos = new FakeRecursoRepository();
        var temas = new FakeTemaRepository();
        var unitOfWork = new FakeUnitOfWork();
        var usuarioId = Guid.CreateVersion7();
        var recurso = CrearRecurso(usuarioId);
        var tema = Tema.Crear(usuarioId, "Modelo OSI", TipoConocimiento.Conceptual);
        recursos.Agregar(recurso);
        temas.Agregar(tema);
        var casoUso = new VincularRecursoATemaCasoUso(recursos, temas, unitOfWork);
        await casoUso.EjecutarAsync(new VincularRecursoATemaSolicitud(recurso.Id, tema.Id), CancellationToken);

        var resultado = await casoUso.EjecutarAsync(new VincularRecursoATemaSolicitud(recurso.Id, tema.Id), CancellationToken);

        Assert.Equal(VincularRecursoATemaEstado.Actualizado, resultado.Estado);
        Assert.Equal(1, recursos.VincularTemaLlamadas);
        Assert.Equal(1, unitOfWork.GuardarCambiosLlamadas);
    }

    private static Recurso CrearRecurso(Guid? usuarioId = null) =>
        Recurso.Guardar(usuarioId ?? Guid.CreateVersion7(), TipoRecurso.Documentacion, "Documentación modelo OSI");

    private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;
}
