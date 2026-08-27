using Aprendizaje.Aplicacion.Roadmap.Fases.ActualizarMetadataPedagogica;
using Aprendizaje.Aplicacion.Roadmap.Fases.CrearFase;
using Aprendizaje.Aplicacion.Roadmap.Fases.ListarFases;
using Aprendizaje.Dominio.Roadmap;
using Aprendizaje.Tests.Soporte;
using Xunit;

namespace Aprendizaje.Tests.Application.Roadmap;

public sealed class FasesApplicationTests
{
    [Fact]
    public async Task CrearFase_DebeAgregarFaseConMetadataYGuardar()
    {
        var fases = new FakeFaseRepository();
        var unitOfWork = new FakeUnitOfWork();
        var casoUso = new CrearFaseCasoUso(fases, unitOfWork);
        var usuarioId = Guid.CreateVersion7();

        var resultado = await casoUso.EjecutarAsync(
            new CrearFaseSolicitud(
                usuarioId,
                " Fundamentos ",
                1,
                ["Dominar redes"],
                ["Completar CCNA 1"],
                1,
                4,
                " ~10 hrs/semana "),
            CancellationToken);

        Assert.NotEqual(Guid.Empty, resultado.Id);
        Assert.Equal(usuarioId, resultado.UsuarioId);
        Assert.Equal("Fundamentos", resultado.Nombre);
        Assert.Equal(["Dominar redes"], resultado.Objetivos);
        Assert.Equal(["Completar CCNA 1"], resultado.CriteriosAvance);
        Assert.Equal(1, resultado.MesInicioRecomendado);
        Assert.Equal(4, resultado.MesFinRecomendado);
        Assert.Equal("~10 hrs/semana", resultado.CargaSemanalRecomendada);
        Assert.NotNull(await fases.ObtenerPorIdAsync(resultado.Id, CancellationToken));
        Assert.Equal(1, fases.AgregarLlamadas);
        Assert.Equal(1, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task CrearFase_DebeRechazarMetadataInvalidaSinGuardar()
    {
        var fases = new FakeFaseRepository();
        var unitOfWork = new FakeUnitOfWork();
        var casoUso = new CrearFaseCasoUso(fases, unitOfWork);

        await Assert.ThrowsAsync<ArgumentException>(() => casoUso.EjecutarAsync(
            new CrearFaseSolicitud(
                Guid.CreateVersion7(),
                "Fundamentos",
                1,
                [],
                [],
                5,
                4),
            CancellationToken));

        Assert.Equal(0, fases.AgregarLlamadas);
        Assert.Equal(0, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task ListarFases_DebeMapearMetadataYListasVacias()
    {
        var fases = new FakeFaseRepository();
        var usuarioId = Guid.CreateVersion7();
        var faseConMetadata = Fase.Crear(usuarioId, "Fundamentos", 1);
        faseConMetadata.ConfigurarMetadataPedagogica(
            ["Dominar redes"],
            ["Completar CCNA 1"],
            1,
            4,
            "~10 hrs/semana");
        var faseSinMetadata = Fase.Crear(usuarioId, "Blue Team", 2);
        fases.Agregar(faseConMetadata);
        fases.Agregar(faseSinMetadata);
        var casoUso = new ListarFasesCasoUso(fases);

        var resultado = await casoUso.EjecutarAsync(new ListarFasesSolicitud(usuarioId), CancellationToken);

        var conMetadata = resultado.Fases.Single(f => f.Id == faseConMetadata.Id);
        Assert.Equal(["Dominar redes"], conMetadata.Objetivos);
        Assert.Equal(["Completar CCNA 1"], conMetadata.CriteriosAvance);
        Assert.Equal(1, conMetadata.MesInicioRecomendado);
        Assert.Equal(4, conMetadata.MesFinRecomendado);
        Assert.Equal("~10 hrs/semana", conMetadata.CargaSemanalRecomendada);

        var sinMetadata = resultado.Fases.Single(f => f.Id == faseSinMetadata.Id);
        Assert.Empty(sinMetadata.Objetivos);
        Assert.Empty(sinMetadata.CriteriosAvance);
    }

    [Fact]
    public async Task ActualizarMetadataPedagogica_DebeActualizarFaseYGuardar()
    {
        var fases = new FakeFaseRepository();
        var unitOfWork = new FakeUnitOfWork();
        var fase = Fase.Crear(Guid.CreateVersion7(), "Fundamentos", 1);
        fases.Agregar(fase);
        var casoUso = new ActualizarMetadataPedagogicaFaseCasoUso(fases, unitOfWork);

        var resultado = await casoUso.EjecutarAsync(
            new ActualizarMetadataPedagogicaFaseSolicitud(
                fase.Id,
                fase.UsuarioId,
                ["Dominar redes"],
                ["Completar CCNA 1"],
                1,
                4,
                "~10 hrs/semana"),
            CancellationToken);

        Assert.Equal(ActualizarMetadataPedagogicaFaseEstado.Actualizada, resultado.Estado);
        Assert.Equal(["Dominar redes"], fase.Objetivos);
        Assert.Equal(["Completar CCNA 1"], fase.CriteriosAvance);
        Assert.Equal(1, fase.MesInicioRecomendado);
        Assert.Equal(4, fase.MesFinRecomendado);
        Assert.Equal("~10 hrs/semana", fase.CargaSemanalRecomendada);
        Assert.Equal(1, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task ActualizarMetadataPedagogica_DebeRetornarFaseNoEncontradaSinGuardar()
    {
        var fases = new FakeFaseRepository();
        var unitOfWork = new FakeUnitOfWork();
        var casoUso = new ActualizarMetadataPedagogicaFaseCasoUso(fases, unitOfWork);

        var resultado = await casoUso.EjecutarAsync(
            new ActualizarMetadataPedagogicaFaseSolicitud(
                Guid.CreateVersion7(),
                Guid.CreateVersion7(),
                [],
                [],
                null,
                null,
                null),
            CancellationToken);

        Assert.Equal(ActualizarMetadataPedagogicaFaseEstado.FaseNoEncontrada, resultado.Estado);
        Assert.Equal(0, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task ActualizarMetadataPedagogica_DebeRetornarUsuarioNoCoincideSinGuardar()
    {
        var fases = new FakeFaseRepository();
        var unitOfWork = new FakeUnitOfWork();
        var fase = Fase.Crear(Guid.CreateVersion7(), "Fundamentos", 1);
        fases.Agregar(fase);
        var casoUso = new ActualizarMetadataPedagogicaFaseCasoUso(fases, unitOfWork);

        var resultado = await casoUso.EjecutarAsync(
            new ActualizarMetadataPedagogicaFaseSolicitud(
                fase.Id,
                Guid.CreateVersion7(),
                [],
                [],
                null,
                null,
                null),
            CancellationToken);

        Assert.Equal(ActualizarMetadataPedagogicaFaseEstado.UsuarioNoCoincide, resultado.Estado);
        Assert.Equal(0, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task ActualizarMetadataPedagogica_DebeRechazarGuidVacioSinGuardar()
    {
        var fases = new FakeFaseRepository();
        var unitOfWork = new FakeUnitOfWork();
        var casoUso = new ActualizarMetadataPedagogicaFaseCasoUso(fases, unitOfWork);

        await Assert.ThrowsAsync<ArgumentException>(() => casoUso.EjecutarAsync(
            new ActualizarMetadataPedagogicaFaseSolicitud(
                Guid.Empty,
                Guid.CreateVersion7(),
                [],
                [],
                null,
                null,
                null),
            CancellationToken));

        Assert.Equal(0, unitOfWork.GuardarCambiosLlamadas);
    }

    private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;
}
