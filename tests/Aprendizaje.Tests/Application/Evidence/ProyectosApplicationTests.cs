using Aprendizaje.Aplicacion.Evidence.Proyectos.CrearProyecto;
using Aprendizaje.Aplicacion.Evidence.Proyectos.ListarProyectos;
using Aprendizaje.Aplicacion.Evidence.Proyectos.ObtenerProyectoPorId;
using Aprendizaje.Aplicacion.Evidence.Proyectos.VincularProyectoAHerramienta;
using Aprendizaje.Aplicacion.Evidence.Proyectos.VincularProyectoATema;
using Aprendizaje.Dominio.Evidence;
using Aprendizaje.Dominio.Roadmap;
using Aprendizaje.Dominio.Study;
using Aprendizaje.Tests.Soporte;
using Xunit;

namespace Aprendizaje.Tests.Application.Evidence;

public sealed class ProyectosApplicationTests
{
    [Fact]
    public async Task CrearProyecto_DebeAgregarProyectoYGuardar()
    {
        var proyectos = new FakeProyectoRepository();
        var unitOfWork = new FakeUnitOfWork();
        var casoUso = new CrearProyectoCasoUso(proyectos, unitOfWork);
        var usuarioId = Guid.CreateVersion7();

        var resultado = await casoUso.EjecutarAsync(
            new CrearProyectoSolicitud(usuarioId, " Analizador de tráfico OSI "),
            CancellationToken);

        Assert.NotEqual(Guid.Empty, resultado.Id);
        Assert.Equal(usuarioId, resultado.UsuarioId);
        Assert.Equal("Analizador de tráfico OSI", resultado.Nombre);
        Assert.Equal(EstadoProyecto.Idea, resultado.Estado);
        Assert.Equal(EstadoMadurez.Borrador, resultado.EstadoMadurez);
        Assert.Null(resultado.Descripcion);
        Assert.Null(resultado.RepositorioUrl);
        Assert.Null(resultado.FechaInicio);
        Assert.Null(resultado.FechaFin);
        Assert.NotNull(await proyectos.ObtenerPorIdAsync(resultado.Id, CancellationToken));
        Assert.Equal(1, proyectos.AgregarLlamadas);
        Assert.Equal(1, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task CrearProyecto_DebeRechazarUsuarioVacioSinGuardar()
    {
        var proyectos = new FakeProyectoRepository();
        var unitOfWork = new FakeUnitOfWork();
        var casoUso = new CrearProyectoCasoUso(proyectos, unitOfWork);

        await Assert.ThrowsAsync<ArgumentException>(() => casoUso.EjecutarAsync(
            new CrearProyectoSolicitud(Guid.Empty, "Analizador de tráfico OSI"),
            CancellationToken));

        Assert.Equal(0, proyectos.AgregarLlamadas);
        Assert.Equal(0, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task ObtenerProyectoPorId_DebeRetornarProyectoExistente()
    {
        var proyectos = new FakeProyectoRepository();
        var proyecto = CrearProyecto();
        proyectos.Agregar(proyecto);
        var casoUso = new ObtenerProyectoPorIdCasoUso(proyectos);

        var resultado = await casoUso.EjecutarAsync(proyecto.Id, CancellationToken);

        Assert.True(resultado.Encontrado);
        Assert.NotNull(resultado.Proyecto);
        Assert.Equal(proyecto.Id, resultado.Proyecto.Id);
        Assert.Equal(proyecto.Nombre, resultado.Proyecto.Nombre);
    }

    [Fact]
    public async Task ObtenerProyectoPorId_DebeRetornarNoEncontrado()
    {
        var proyectos = new FakeProyectoRepository();
        var casoUso = new ObtenerProyectoPorIdCasoUso(proyectos);

        var resultado = await casoUso.EjecutarAsync(Guid.CreateVersion7(), CancellationToken);

        Assert.False(resultado.Encontrado);
        Assert.Null(resultado.Proyecto);
    }

    [Fact]
    public async Task ListarProyectos_DebeMapearColeccionDelRepository()
    {
        var proyectos = new FakeProyectoRepository();
        var usuarioId = Guid.CreateVersion7();
        var proyecto = CrearProyecto(usuarioId);
        proyectos.Agregar(proyecto);
        proyectos.Agregar(CrearProyecto(Guid.CreateVersion7()));
        var casoUso = new ListarProyectosCasoUso(proyectos);

        var resultado = await casoUso.EjecutarAsync(new ListarProyectosSolicitud(usuarioId), CancellationToken);

        var resumen = Assert.Single(resultado.Proyectos);
        Assert.Equal(proyecto.Id, resumen.Id);
        Assert.Equal(usuarioId, resumen.UsuarioId);
        Assert.Equal(proyecto.Nombre, resumen.Nombre);
    }

    [Fact]
    public async Task VincularProyectoATema_DebeRetornarProyectoNoEncontradoSinGuardar()
    {
        var proyectos = new FakeProyectoRepository();
        var temas = new FakeTemaRepository();
        var unitOfWork = new FakeUnitOfWork();
        var casoUso = new VincularProyectoATemaCasoUso(proyectos, temas, unitOfWork);

        var resultado = await casoUso.EjecutarAsync(
            new VincularProyectoATemaSolicitud(Guid.CreateVersion7(), Guid.CreateVersion7()),
            CancellationToken);

        Assert.Equal(VincularProyectoATemaEstado.ProyectoNoEncontrado, resultado.Estado);
        Assert.Equal(0, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task VincularProyectoATema_DebeRetornarTemaNoEncontradoSinGuardar()
    {
        var proyectos = new FakeProyectoRepository();
        var temas = new FakeTemaRepository();
        var unitOfWork = new FakeUnitOfWork();
        var proyecto = CrearProyecto();
        proyectos.Agregar(proyecto);
        var casoUso = new VincularProyectoATemaCasoUso(proyectos, temas, unitOfWork);

        var resultado = await casoUso.EjecutarAsync(
            new VincularProyectoATemaSolicitud(proyecto.Id, Guid.CreateVersion7()),
            CancellationToken);

        Assert.Equal(VincularProyectoATemaEstado.TemaNoEncontrado, resultado.Estado);
        Assert.Equal(0, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task VincularProyectoATema_DebeRetornarUsuarioNoCoincideSinGuardar()
    {
        var proyectos = new FakeProyectoRepository();
        var temas = new FakeTemaRepository();
        var unitOfWork = new FakeUnitOfWork();
        var proyecto = CrearProyecto();
        var tema = Tema.Crear(Guid.CreateVersion7(), "Modelo OSI", TipoConocimiento.Conceptual);
        proyectos.Agregar(proyecto);
        temas.Agregar(tema);
        var casoUso = new VincularProyectoATemaCasoUso(proyectos, temas, unitOfWork);

        var resultado = await casoUso.EjecutarAsync(
            new VincularProyectoATemaSolicitud(proyecto.Id, tema.Id),
            CancellationToken);

        Assert.Equal(VincularProyectoATemaEstado.UsuarioNoCoincide, resultado.Estado);
        Assert.Equal(0, proyectos.VincularTemaLlamadas);
        Assert.Equal(0, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task VincularProyectoATema_DebeVincularYGuardar()
    {
        var proyectos = new FakeProyectoRepository();
        var temas = new FakeTemaRepository();
        var unitOfWork = new FakeUnitOfWork();
        var usuarioId = Guid.CreateVersion7();
        var proyecto = CrearProyecto(usuarioId);
        var tema = Tema.Crear(usuarioId, "Modelo OSI", TipoConocimiento.Conceptual);
        proyectos.Agregar(proyecto);
        temas.Agregar(tema);
        var casoUso = new VincularProyectoATemaCasoUso(proyectos, temas, unitOfWork);

        var resultado = await casoUso.EjecutarAsync(
            new VincularProyectoATemaSolicitud(proyecto.Id, tema.Id),
            CancellationToken);

        Assert.Equal(VincularProyectoATemaEstado.Actualizado, resultado.Estado);
        Assert.True(await proyectos.ExisteVinculoTemaAsync(proyecto.Id, tema.Id, CancellationToken));
        Assert.Equal(1, proyectos.VincularTemaLlamadas);
        Assert.Equal(1, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task VincularProyectoATema_DebeSerIdempotenteSinGuardarDeNuevo()
    {
        var proyectos = new FakeProyectoRepository();
        var temas = new FakeTemaRepository();
        var unitOfWork = new FakeUnitOfWork();
        var usuarioId = Guid.CreateVersion7();
        var proyecto = CrearProyecto(usuarioId);
        var tema = Tema.Crear(usuarioId, "Modelo OSI", TipoConocimiento.Conceptual);
        proyectos.Agregar(proyecto);
        temas.Agregar(tema);
        var casoUso = new VincularProyectoATemaCasoUso(proyectos, temas, unitOfWork);
        await casoUso.EjecutarAsync(new VincularProyectoATemaSolicitud(proyecto.Id, tema.Id), CancellationToken);

        var resultado = await casoUso.EjecutarAsync(
            new VincularProyectoATemaSolicitud(proyecto.Id, tema.Id),
            CancellationToken);

        Assert.Equal(VincularProyectoATemaEstado.Actualizado, resultado.Estado);
        Assert.Equal(1, proyectos.VincularTemaLlamadas);
        Assert.Equal(1, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task VincularProyectoAHerramienta_DebeRetornarProyectoNoEncontradoSinGuardar()
    {
        var proyectos = new FakeProyectoRepository();
        var herramientas = new FakeHerramientaRepository();
        var unitOfWork = new FakeUnitOfWork();
        var casoUso = new VincularProyectoAHerramientaCasoUso(proyectos, herramientas, unitOfWork);

        var resultado = await casoUso.EjecutarAsync(
            new VincularProyectoAHerramientaSolicitud(Guid.CreateVersion7(), Guid.CreateVersion7()),
            CancellationToken);

        Assert.Equal(VincularProyectoAHerramientaEstado.ProyectoNoEncontrado, resultado.Estado);
        Assert.Equal(0, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task VincularProyectoAHerramienta_DebeRetornarHerramientaNoEncontradaSinGuardar()
    {
        var proyectos = new FakeProyectoRepository();
        var herramientas = new FakeHerramientaRepository();
        var unitOfWork = new FakeUnitOfWork();
        var proyecto = CrearProyecto();
        proyectos.Agregar(proyecto);
        var casoUso = new VincularProyectoAHerramientaCasoUso(proyectos, herramientas, unitOfWork);

        var resultado = await casoUso.EjecutarAsync(
            new VincularProyectoAHerramientaSolicitud(proyecto.Id, Guid.CreateVersion7()),
            CancellationToken);

        Assert.Equal(VincularProyectoAHerramientaEstado.HerramientaNoEncontrada, resultado.Estado);
        Assert.Equal(0, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task VincularProyectoAHerramienta_DebeVincularYGuardar()
    {
        var proyectos = new FakeProyectoRepository();
        var herramientas = new FakeHerramientaRepository();
        var unitOfWork = new FakeUnitOfWork();
        var proyecto = CrearProyecto();
        var herramienta = Herramienta.Crear("Wireshark");
        proyectos.Agregar(proyecto);
        herramientas.Agregar(herramienta);
        var casoUso = new VincularProyectoAHerramientaCasoUso(proyectos, herramientas, unitOfWork);

        var resultado = await casoUso.EjecutarAsync(
            new VincularProyectoAHerramientaSolicitud(proyecto.Id, herramienta.Id),
            CancellationToken);

        Assert.Equal(VincularProyectoAHerramientaEstado.Actualizado, resultado.Estado);
        Assert.True(await proyectos.ExisteVinculoHerramientaAsync(proyecto.Id, herramienta.Id, CancellationToken));
        Assert.Equal(1, proyectos.VincularHerramientaLlamadas);
        Assert.Equal(1, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task VincularProyectoAHerramienta_DebeSerIdempotenteSinGuardarDeNuevo()
    {
        var proyectos = new FakeProyectoRepository();
        var herramientas = new FakeHerramientaRepository();
        var unitOfWork = new FakeUnitOfWork();
        var proyecto = CrearProyecto();
        var herramienta = Herramienta.Crear("Wireshark");
        proyectos.Agregar(proyecto);
        herramientas.Agregar(herramienta);
        var casoUso = new VincularProyectoAHerramientaCasoUso(proyectos, herramientas, unitOfWork);
        await casoUso.EjecutarAsync(
            new VincularProyectoAHerramientaSolicitud(proyecto.Id, herramienta.Id),
            CancellationToken);

        var resultado = await casoUso.EjecutarAsync(
            new VincularProyectoAHerramientaSolicitud(proyecto.Id, herramienta.Id),
            CancellationToken);

        Assert.Equal(VincularProyectoAHerramientaEstado.Actualizado, resultado.Estado);
        Assert.Equal(1, proyectos.VincularHerramientaLlamadas);
        Assert.Equal(1, unitOfWork.GuardarCambiosLlamadas);
    }

    private static Proyecto CrearProyecto(Guid? usuarioId = null) =>
        Proyecto.Crear(usuarioId ?? Guid.CreateVersion7(), "Analizador de tráfico OSI");

    private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;
}
