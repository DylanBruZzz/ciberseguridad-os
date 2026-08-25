using Aprendizaje.Aplicacion.Roadmap.Competencias.CrearCompetencia;
using Aprendizaje.Aplicacion.Roadmap.Competencias.ListarCompetencias;
using Aprendizaje.Aplicacion.Roadmap.Competencias.ObtenerCompetenciaPorId;
using Aprendizaje.Aplicacion.Roadmap.Competencias.VincularCompetenciaATema;
using Aprendizaje.Dominio.Roadmap;
using Aprendizaje.Tests.Soporte;
using Xunit;

namespace Aprendizaje.Tests.Application.Roadmap;

public sealed class CompetenciasApplicationTests
{
    [Fact]
    public async Task CrearCompetencia_DebeAgregarCompetenciaYGuardar()
    {
        var competencias = new FakeCompetenciaRepository();
        var unitOfWork = new FakeUnitOfWork();
        var casoUso = new CrearCompetenciaCasoUso(competencias, unitOfWork);
        var usuarioId = Guid.CreateVersion7();

        var resultado = await casoUso.EjecutarAsync(
            new CrearCompetenciaSolicitud(usuarioId, " Comprensión de fundamentos de redes "),
            CancellationToken);

        Assert.NotEqual(Guid.Empty, resultado.Id);
        Assert.Equal(usuarioId, resultado.UsuarioId);
        Assert.Equal("Comprensión de fundamentos de redes", resultado.Nombre);
        Assert.Null(resultado.Descripcion);
        Assert.NotNull(await competencias.ObtenerPorIdAsync(resultado.Id, CancellationToken));
        Assert.Equal(1, competencias.AgregarLlamadas);
        Assert.Equal(1, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task CrearCompetencia_DebeRechazarUsuarioVacioSinGuardar()
    {
        var competencias = new FakeCompetenciaRepository();
        var unitOfWork = new FakeUnitOfWork();
        var casoUso = new CrearCompetenciaCasoUso(competencias, unitOfWork);

        await Assert.ThrowsAsync<ArgumentException>(() => casoUso.EjecutarAsync(
            new CrearCompetenciaSolicitud(Guid.Empty, "Comprensión de fundamentos de redes"),
            CancellationToken));

        Assert.Equal(0, competencias.AgregarLlamadas);
        Assert.Equal(0, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task ObtenerCompetenciaPorId_DebeRetornarCompetenciaExistente()
    {
        var competencias = new FakeCompetenciaRepository();
        var competencia = CrearCompetencia();
        competencias.Agregar(competencia);
        var casoUso = new ObtenerCompetenciaPorIdCasoUso(competencias);

        var resultado = await casoUso.EjecutarAsync(competencia.Id, CancellationToken);

        Assert.True(resultado.Encontrada);
        Assert.NotNull(resultado.Competencia);
        Assert.Equal(competencia.Id, resultado.Competencia.Id);
        Assert.Equal(competencia.Nombre, resultado.Competencia.Nombre);
    }

    [Fact]
    public async Task ObtenerCompetenciaPorId_DebeRetornarNoEncontrada()
    {
        var competencias = new FakeCompetenciaRepository();
        var casoUso = new ObtenerCompetenciaPorIdCasoUso(competencias);

        var resultado = await casoUso.EjecutarAsync(Guid.CreateVersion7(), CancellationToken);

        Assert.False(resultado.Encontrada);
        Assert.Null(resultado.Competencia);
    }

    [Fact]
    public async Task ListarCompetencias_DebeMapearColeccionDelRepository()
    {
        var competencias = new FakeCompetenciaRepository();
        var usuarioId = Guid.CreateVersion7();
        var competencia = CrearCompetencia(usuarioId);
        competencias.Agregar(competencia);
        competencias.Agregar(CrearCompetencia(Guid.CreateVersion7()));
        var casoUso = new ListarCompetenciasCasoUso(competencias);

        var resultado = await casoUso.EjecutarAsync(
            new ListarCompetenciasSolicitud(usuarioId),
            CancellationToken);

        var resumen = Assert.Single(resultado.Competencias);
        Assert.Equal(competencia.Id, resumen.Id);
        Assert.Equal(usuarioId, resumen.UsuarioId);
        Assert.Equal(competencia.Nombre, resumen.Nombre);
        Assert.Null(resumen.Descripcion);
    }

    [Fact]
    public async Task ListarCompetencias_DebeRetornarListaVacia()
    {
        var competencias = new FakeCompetenciaRepository();
        var casoUso = new ListarCompetenciasCasoUso(competencias);

        var resultado = await casoUso.EjecutarAsync(
            new ListarCompetenciasSolicitud(Guid.CreateVersion7()),
            CancellationToken);

        Assert.Empty(resultado.Competencias);
    }

    [Fact]
    public async Task VincularCompetenciaATema_DebeRetornarCompetenciaNoEncontradaSinGuardar()
    {
        var competencias = new FakeCompetenciaRepository();
        var temas = new FakeTemaRepository();
        var unitOfWork = new FakeUnitOfWork();
        var casoUso = new VincularCompetenciaATemaCasoUso(competencias, temas, unitOfWork);

        var resultado = await casoUso.EjecutarAsync(
            new VincularCompetenciaATemaSolicitud(Guid.CreateVersion7(), Guid.CreateVersion7()),
            CancellationToken);

        Assert.Equal(VincularCompetenciaATemaEstado.CompetenciaNoEncontrada, resultado.Estado);
        Assert.Equal(0, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task VincularCompetenciaATema_DebeRetornarTemaNoEncontradoSinGuardar()
    {
        var competencias = new FakeCompetenciaRepository();
        var temas = new FakeTemaRepository();
        var unitOfWork = new FakeUnitOfWork();
        var competencia = CrearCompetencia();
        competencias.Agregar(competencia);
        var casoUso = new VincularCompetenciaATemaCasoUso(competencias, temas, unitOfWork);

        var resultado = await casoUso.EjecutarAsync(
            new VincularCompetenciaATemaSolicitud(competencia.Id, Guid.CreateVersion7()),
            CancellationToken);

        Assert.Equal(VincularCompetenciaATemaEstado.TemaNoEncontrado, resultado.Estado);
        Assert.Equal(0, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task VincularCompetenciaATema_DebeRetornarUsuarioNoCoincideSinGuardar()
    {
        var competencias = new FakeCompetenciaRepository();
        var temas = new FakeTemaRepository();
        var unitOfWork = new FakeUnitOfWork();
        var competencia = CrearCompetencia();
        var tema = Tema.Crear(Guid.CreateVersion7(), "Modelo OSI", TipoConocimiento.Conceptual);
        competencias.Agregar(competencia);
        temas.Agregar(tema);
        var casoUso = new VincularCompetenciaATemaCasoUso(competencias, temas, unitOfWork);

        var resultado = await casoUso.EjecutarAsync(
            new VincularCompetenciaATemaSolicitud(competencia.Id, tema.Id),
            CancellationToken);

        Assert.Equal(VincularCompetenciaATemaEstado.UsuarioNoCoincide, resultado.Estado);
        Assert.Equal(0, competencias.VincularTemaLlamadas);
        Assert.Equal(0, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task VincularCompetenciaATema_DebeVincularYGuardar()
    {
        var competencias = new FakeCompetenciaRepository();
        var temas = new FakeTemaRepository();
        var unitOfWork = new FakeUnitOfWork();
        var usuarioId = Guid.CreateVersion7();
        var competencia = CrearCompetencia(usuarioId);
        var tema = Tema.Crear(usuarioId, "Modelo OSI", TipoConocimiento.Conceptual);
        competencias.Agregar(competencia);
        temas.Agregar(tema);
        var casoUso = new VincularCompetenciaATemaCasoUso(competencias, temas, unitOfWork);

        var resultado = await casoUso.EjecutarAsync(
            new VincularCompetenciaATemaSolicitud(competencia.Id, tema.Id),
            CancellationToken);

        Assert.Equal(VincularCompetenciaATemaEstado.Actualizado, resultado.Estado);
        Assert.True(await competencias.ExisteVinculoTemaAsync(competencia.Id, tema.Id, CancellationToken));
        Assert.Equal(1, competencias.VincularTemaLlamadas);
        Assert.Equal(1, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task VincularCompetenciaATema_DebeSerIdempotenteSinGuardarDeNuevo()
    {
        var competencias = new FakeCompetenciaRepository();
        var temas = new FakeTemaRepository();
        var unitOfWork = new FakeUnitOfWork();
        var usuarioId = Guid.CreateVersion7();
        var competencia = CrearCompetencia(usuarioId);
        var tema = Tema.Crear(usuarioId, "Modelo OSI", TipoConocimiento.Conceptual);
        competencias.Agregar(competencia);
        temas.Agregar(tema);
        var casoUso = new VincularCompetenciaATemaCasoUso(competencias, temas, unitOfWork);
        await casoUso.EjecutarAsync(new VincularCompetenciaATemaSolicitud(competencia.Id, tema.Id), CancellationToken);

        var resultado = await casoUso.EjecutarAsync(
            new VincularCompetenciaATemaSolicitud(competencia.Id, tema.Id),
            CancellationToken);

        Assert.Equal(VincularCompetenciaATemaEstado.Actualizado, resultado.Estado);
        Assert.Equal(1, competencias.VincularTemaLlamadas);
        Assert.Equal(1, unitOfWork.GuardarCambiosLlamadas);
    }

    private static Competencia CrearCompetencia(Guid? usuarioId = null) =>
        Competencia.Crear(usuarioId ?? Guid.CreateVersion7(), "Comprensión de fundamentos de redes");

    private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;
}
