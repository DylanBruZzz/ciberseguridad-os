using Aprendizaje.Dominio.Roadmap;
using Xunit;

namespace Aprendizaje.Tests.Dominio.Roadmap;

public sealed class FaseTests
{
    [Fact]
    public void Crear_DebeCrearFaseValidaConMetadataVacia()
    {
        var usuarioId = Guid.CreateVersion7();

        var fase = Fase.Crear(usuarioId, "Fundamentos", 1);

        Assert.NotEqual(Guid.Empty, fase.Id);
        Assert.Equal(usuarioId, fase.UsuarioId);
        Assert.Equal("Fundamentos", fase.Nombre);
        Assert.Equal(1, fase.Orden);
        Assert.Empty(fase.Objetivos);
        Assert.Empty(fase.CriteriosAvance);
        Assert.Null(fase.MesInicioRecomendado);
        Assert.Null(fase.MesFinRecomendado);
        Assert.Null(fase.CargaSemanalRecomendada);
        Assert.Empty(fase.EventosDominio);
    }

    [Fact]
    public void ConfigurarMetadataPedagogica_DebeConservarValoresNormalizados()
    {
        var fase = Fase.Crear(Guid.CreateVersion7(), "Fundamentos", 1);

        fase.ConfigurarMetadataPedagogica(
            [" Dominar redes ", "", "Administrar Linux"],
            [" Configurar subnetting ", " ", "Completar CCNA 1"],
            1,
            4,
            "  ~10 hrs/semana  ");

        Assert.Equal(["Dominar redes", "Administrar Linux"], fase.Objetivos);
        Assert.Equal(["Configurar subnetting", "Completar CCNA 1"], fase.CriteriosAvance);
        Assert.Equal(1, fase.MesInicioRecomendado);
        Assert.Equal(4, fase.MesFinRecomendado);
        Assert.Equal("~10 hrs/semana", fase.CargaSemanalRecomendada);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ConfigurarMetadataPedagogica_DebeRechazarMesInicioNoPositivo(int mesInicio)
    {
        var fase = Fase.Crear(Guid.CreateVersion7(), "Fundamentos", 1);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            fase.ConfigurarMetadataPedagogica([], [], mesInicio, null, null));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ConfigurarMetadataPedagogica_DebeRechazarMesFinNoPositivo(int mesFin)
    {
        var fase = Fase.Crear(Guid.CreateVersion7(), "Fundamentos", 1);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            fase.ConfigurarMetadataPedagogica([], [], null, mesFin, null));
    }

    [Fact]
    public void ConfigurarMetadataPedagogica_DebeRechazarMesFinAnteriorAInicio()
    {
        var fase = Fase.Crear(Guid.CreateVersion7(), "Fundamentos", 1);

        Assert.Throws<ArgumentException>(() =>
            fase.ConfigurarMetadataPedagogica([], [], 5, 4, null));
    }

    [Fact]
    public void ConfigurarMetadataPedagogica_DebePermitirListasVaciasYCargaVacia()
    {
        var fase = Fase.Crear(Guid.CreateVersion7(), "Fundamentos", 1);

        fase.ConfigurarMetadataPedagogica([], [], null, null, " ");

        Assert.Empty(fase.Objetivos);
        Assert.Empty(fase.CriteriosAvance);
        Assert.Null(fase.CargaSemanalRecomendada);
    }

    [Fact]
    public void ObjetivosYCriteriosAvance_NoDebenExponerColeccionesMutables()
    {
        var fase = Fase.Crear(Guid.CreateVersion7(), "Fundamentos", 1);

        var objetivos = Assert.IsAssignableFrom<ICollection<string>>(fase.Objetivos);
        var criterios = Assert.IsAssignableFrom<ICollection<string>>(fase.CriteriosAvance);

        Assert.True(objetivos.IsReadOnly);
        Assert.True(criterios.IsReadOnly);
    }
}
