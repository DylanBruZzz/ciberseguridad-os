using Aprendizaje.Aplicacion.Resource.Recursos.ActualizarRecurso;
using Aprendizaje.Aplicacion.Resource.Recursos.CrearRecurso;
using Aprendizaje.Aplicacion.Resource.Recursos.EliminarRecurso;
using Aprendizaje.Aplicacion.Resource.Recursos;
using Aprendizaje.Aplicacion.Resource.Recursos.ListarRecursos;
using Aprendizaje.Aplicacion.Resource.Recursos.ObtenerRecursoPorId;
using Aprendizaje.Aplicacion.Resource.Recursos.VincularRecursoATema;
using Aprendizaje.Dominio.Nucleo;
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
        var recurso = CrearRecurso();
        var consulta = new FakeConsultaRecursosV1
        {
            Detalle = CrearDetalle(recurso, [new RecursoTemaResumen(Guid.CreateVersion7(), "Modelo OSI")])
        };
        var casoUso = new ObtenerRecursoPorIdCasoUso(consulta);

        var resultado = await casoUso.EjecutarAsync(recurso.Id, CancellationToken);

        Assert.True(resultado.Encontrado);
        Assert.NotNull(resultado.Recurso);
        Assert.Equal(recurso.Id, resultado.Recurso.Id);
        Assert.Equal(recurso.Titulo, resultado.Recurso.Titulo);
        Assert.Equal("Modelo OSI", Assert.Single(resultado.Recurso.Temas).Nombre);
    }

    [Fact]
    public async Task ObtenerRecursoPorId_DebeRetornarNoEncontrado()
    {
        var consulta = new FakeConsultaRecursosV1();
        var casoUso = new ObtenerRecursoPorIdCasoUso(consulta);

        var resultado = await casoUso.EjecutarAsync(Guid.CreateVersion7(), CancellationToken);

        Assert.False(resultado.Encontrado);
        Assert.Null(resultado.Recurso);
    }

    [Fact]
    public async Task ListarRecursos_DebeMapearColeccionDelRepository()
    {
        var usuarioId = Guid.CreateVersion7();
        var recurso = CrearRecurso(usuarioId);
        var consulta = new FakeConsultaRecursosV1
        {
            Recursos =
            [
                new RecursoResumen(
                    recurso.Id,
                    usuarioId,
                    recurso.Tipo,
                    recurso.Titulo,
                    recurso.Url,
                    recurso.Estado,
                    [new RecursoTemaResumen(Guid.CreateVersion7(), "Modelo OSI")])
            ]
        };
        var casoUso = new ListarRecursosCasoUso(consulta);

        var resultado = await casoUso.EjecutarAsync(new ListarRecursosSolicitud(usuarioId), CancellationToken);

        var resumen = Assert.Single(resultado.Recursos);
        Assert.Equal(recurso.Id, resumen.Id);
        Assert.Equal(usuarioId, resumen.UsuarioId);
        Assert.Equal(recurso.Titulo, resumen.Titulo);
        Assert.Equal("Modelo OSI", Assert.Single(resumen.Temas).Nombre);
    }

    [Fact]
    public async Task ListarRecursos_DebePasarTemaIdOpcionalALaConsulta()
    {
        var usuarioId = Guid.CreateVersion7();
        var temaId = Guid.CreateVersion7();
        var consulta = new FakeConsultaRecursosV1();
        var casoUso = new ListarRecursosCasoUso(consulta);

        _ = await casoUso.EjecutarAsync(new ListarRecursosSolicitud(usuarioId, temaId), CancellationToken);

        Assert.Equal(usuarioId, consulta.SolicitudRecibida?.UsuarioId);
        Assert.Equal(temaId, consulta.SolicitudRecibida?.TemaId);
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

    [Fact]
    public async Task ActualizarRecurso_DebeRetornarUsuarioNoEncontradoSinGuardar()
    {
        var recursos = new FakeRecursoRepository();
        var usuarios = new FakeUsuarioRepository();
        var unitOfWork = new FakeUnitOfWork();
        var casoUso = new ActualizarRecursoCasoUso(recursos, usuarios, unitOfWork);

        var resultado = await casoUso.EjecutarAsync(SolicitudActualizar(
            Guid.CreateVersion7(),
            Guid.CreateVersion7()), CancellationToken);

        Assert.Equal(ActualizarRecursoEstado.UsuarioNoEncontrado, resultado.Estado);
        Assert.Equal(0, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task ActualizarRecurso_DebeRetornarRecursoNoEncontradoSinGuardar()
    {
        var recursos = new FakeRecursoRepository();
        var usuarios = new FakeUsuarioRepository();
        var unitOfWork = new FakeUnitOfWork();
        var usuario = Usuario.Registrar("Dylan", EmailUnico());
        usuarios.Agregar(usuario);
        var casoUso = new ActualizarRecursoCasoUso(recursos, usuarios, unitOfWork);

        var resultado = await casoUso.EjecutarAsync(SolicitudActualizar(
            Guid.CreateVersion7(),
            usuario.Id), CancellationToken);

        Assert.Equal(ActualizarRecursoEstado.RecursoNoEncontrado, resultado.Estado);
        Assert.Equal(0, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task ActualizarRecurso_DebeRetornarUsuarioNoCoincideSinGuardar()
    {
        var recursos = new FakeRecursoRepository();
        var usuarios = new FakeUsuarioRepository();
        var unitOfWork = new FakeUnitOfWork();
        var usuarioA = Usuario.Registrar("Usuario A", EmailUnico());
        var usuarioB = Usuario.Registrar("Usuario B", EmailUnico());
        var recurso = CrearRecurso(usuarioA.Id);
        usuarios.Agregar(usuarioA);
        usuarios.Agregar(usuarioB);
        recursos.Agregar(recurso);
        var casoUso = new ActualizarRecursoCasoUso(recursos, usuarios, unitOfWork);

        var resultado = await casoUso.EjecutarAsync(SolicitudActualizar(recurso.Id, usuarioB.Id), CancellationToken);

        Assert.Equal(ActualizarRecursoEstado.UsuarioNoCoincide, resultado.Estado);
        Assert.Equal("Documentación modelo OSI", recurso.Titulo);
        Assert.Equal(0, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task ActualizarRecurso_DebeActualizarCamposYGuardar()
    {
        var recursos = new FakeRecursoRepository();
        var usuarios = new FakeUsuarioRepository();
        var unitOfWork = new FakeUnitOfWork();
        var usuario = Usuario.Registrar("Dylan", EmailUnico());
        var recurso = CrearRecurso(usuario.Id);
        usuarios.Agregar(usuario);
        recursos.Agregar(recurso);
        var casoUso = new ActualizarRecursoCasoUso(recursos, usuarios, unitOfWork);

        var resultado = await casoUso.EjecutarAsync(new ActualizarRecursoSolicitud(
            recurso.Id,
            usuario.Id,
            " Curso actualizado ",
            "https://example.local/recurso",
            EstadoRecurso.Consultado,
            5,
            "Notas nuevas",
            "ChatGPT",
            "Prompt utilizado"), CancellationToken);

        Assert.Equal(ActualizarRecursoEstado.Actualizado, resultado.Estado);
        Assert.Equal("Curso actualizado", recurso.Titulo);
        Assert.Equal("https://example.local/recurso", recurso.Url);
        Assert.Equal(EstadoRecurso.Consultado, recurso.Estado);
        Assert.Equal(5, recurso.Rating?.Valor);
        Assert.Equal("Notas nuevas", recurso.Notas);
        Assert.Equal("ChatGPT", recurso.HerramientaIA);
        Assert.Equal("Prompt utilizado", recurso.PromptsUtilizados);
        Assert.Equal(1, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task ActualizarRecurso_DebePermitirCamposOpcionalesNulos()
    {
        var recursos = new FakeRecursoRepository();
        var usuarios = new FakeUsuarioRepository();
        var unitOfWork = new FakeUnitOfWork();
        var usuario = Usuario.Registrar("Dylan", EmailUnico());
        var recurso = CrearRecurso(usuario.Id);
        recurso.ActualizarUrl("https://example.local/recurso");
        usuarios.Agregar(usuario);
        recursos.Agregar(recurso);
        var casoUso = new ActualizarRecursoCasoUso(recursos, usuarios, unitOfWork);

        var resultado = await casoUso.EjecutarAsync(new ActualizarRecursoSolicitud(
            recurso.Id,
            usuario.Id,
            "Documentación actualizada",
            null,
            EstadoRecurso.PorRevisar,
            null,
            null,
            null,
            null), CancellationToken);

        Assert.Equal(ActualizarRecursoEstado.Actualizado, resultado.Estado);
        Assert.Null(recurso.Url);
        Assert.Null(recurso.Rating);
        Assert.Null(recurso.Notas);
        Assert.Null(recurso.HerramientaIA);
        Assert.Null(recurso.PromptsUtilizados);
        Assert.Equal(1, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task ActualizarRecurso_DebePropagarErrorDeDominioSinGuardar()
    {
        var recursos = new FakeRecursoRepository();
        var usuarios = new FakeUsuarioRepository();
        var unitOfWork = new FakeUnitOfWork();
        var usuario = Usuario.Registrar("Dylan", EmailUnico());
        var recurso = CrearRecurso(usuario.Id);
        usuarios.Agregar(usuario);
        recursos.Agregar(recurso);
        var casoUso = new ActualizarRecursoCasoUso(recursos, usuarios, unitOfWork);

        await Assert.ThrowsAsync<ArgumentException>(() => casoUso.EjecutarAsync(
            SolicitudActualizar(recurso.Id, usuario.Id, titulo: " "),
            CancellationToken));

        Assert.Equal(0, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task EliminarRecurso_DebeRetornarRecursoNoEncontradoSinGuardar()
    {
        var recursos = new FakeRecursoRepository();
        var usuarios = new FakeUsuarioRepository();
        var unitOfWork = new FakeUnitOfWork();
        var usuario = Usuario.Registrar("Dylan", EmailUnico());
        usuarios.Agregar(usuario);
        var casoUso = new EliminarRecursoCasoUso(recursos, usuarios, unitOfWork);

        var resultado = await casoUso.EjecutarAsync(
            new EliminarRecursoSolicitud(Guid.CreateVersion7(), usuario.Id),
            CancellationToken);

        Assert.Equal(EliminarRecursoEstado.RecursoNoEncontrado, resultado.Estado);
        Assert.Equal(0, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task EliminarRecurso_DebeRetornarUsuarioNoCoincideSinGuardar()
    {
        var recursos = new FakeRecursoRepository();
        var usuarios = new FakeUsuarioRepository();
        var unitOfWork = new FakeUnitOfWork();
        var usuarioA = Usuario.Registrar("Usuario A", EmailUnico());
        var usuarioB = Usuario.Registrar("Usuario B", EmailUnico());
        var recurso = CrearRecurso(usuarioA.Id);
        usuarios.Agregar(usuarioA);
        usuarios.Agregar(usuarioB);
        recursos.Agregar(recurso);
        var casoUso = new EliminarRecursoCasoUso(recursos, usuarios, unitOfWork);

        var resultado = await casoUso.EjecutarAsync(
            new EliminarRecursoSolicitud(recurso.Id, usuarioB.Id),
            CancellationToken);

        Assert.Equal(EliminarRecursoEstado.UsuarioNoCoincide, resultado.Estado);
        Assert.Null(recurso.FechaEliminacionUtc);
        Assert.Equal(0, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task EliminarRecurso_DebeMarcarEliminadoYGuardar()
    {
        var recursos = new FakeRecursoRepository();
        var usuarios = new FakeUsuarioRepository();
        var unitOfWork = new FakeUnitOfWork();
        var usuario = Usuario.Registrar("Dylan", EmailUnico());
        var recurso = CrearRecurso(usuario.Id);
        usuarios.Agregar(usuario);
        recursos.Agregar(recurso);
        var casoUso = new EliminarRecursoCasoUso(recursos, usuarios, unitOfWork);

        var resultado = await casoUso.EjecutarAsync(
            new EliminarRecursoSolicitud(recurso.Id, usuario.Id),
            CancellationToken);

        Assert.Equal(EliminarRecursoEstado.Eliminado, resultado.Estado);
        Assert.NotNull(recurso.FechaEliminacionUtc);
        Assert.Equal(1, unitOfWork.GuardarCambiosLlamadas);
    }

    private static Recurso CrearRecurso(Guid? usuarioId = null) =>
        Recurso.Guardar(usuarioId ?? Guid.CreateVersion7(), TipoRecurso.Documentacion, "Documentación modelo OSI");

    private static RecursoDetalle CrearDetalle(
        Recurso recurso,
        IReadOnlyCollection<RecursoTemaResumen>? temas = null) =>
        new(
            recurso.Id,
            recurso.UsuarioId,
            recurso.Tipo,
            recurso.Titulo,
            recurso.Url,
            recurso.Estado,
            recurso.Rating?.Valor,
            recurso.Notas,
            recurso.HerramientaIA,
            recurso.PromptsUtilizados,
            temas ?? []);

    private static ActualizarRecursoSolicitud SolicitudActualizar(
        Guid recursoId,
        Guid usuarioId,
        string titulo = "Recurso actualizado") =>
        new(
            recursoId,
            usuarioId,
            titulo,
            "https://example.local/recurso",
            EstadoRecurso.EnUso,
            4,
            "Notas",
            "ChatGPT",
            "Prompt");

    private static string EmailUnico() => $"resource-{Guid.CreateVersion7():N}@local.test";

    private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

    private sealed class FakeConsultaRecursosV1 : IConsultaRecursosV1
    {
        public ListarRecursosSolicitud? SolicitudRecibida { get; private set; }

        public IReadOnlyCollection<RecursoResumen> Recursos { get; init; } = [];

        public RecursoDetalle? Detalle { get; init; }

        public Task<IReadOnlyCollection<RecursoResumen>> ListarAsync(
            ListarRecursosSolicitud solicitud,
            CancellationToken cancellationToken = default)
        {
            SolicitudRecibida = solicitud;

            return Task.FromResult(Recursos);
        }

        public Task<RecursoDetalle?> ObtenerDetalleAsync(
            Guid id,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(Detalle?.Id == id ? Detalle : null);
    }
}
