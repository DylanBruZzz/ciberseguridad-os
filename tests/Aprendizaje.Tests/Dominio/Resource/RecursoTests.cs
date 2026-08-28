using Aprendizaje.Dominio.Resource;
using Aprendizaje.Dominio.Resource.ValueObjects;
using Xunit;

namespace Aprendizaje.Tests.Dominio.Resource;

public sealed class RecursoTests
{
    [Fact]
    public void Recurso_DebeActualizarPropiedadesEditables()
    {
        var recurso = Recurso.Guardar(Guid.CreateVersion7(), TipoRecurso.Documentacion, " Documentación OSI ");

        recurso.CambiarTitulo(" Curso redes ");
        recurso.ActualizarUrl("https://example.local/recurso");
        recurso.CambiarEstado(EstadoRecurso.EnUso);
        recurso.CalificarCon(RatingRecurso.Crear(4));
        recurso.ActualizarNotas("Notas de revisión");
        recurso.RegistrarUsoDeIA("ChatGPT", "Prompt de resumen");

        Assert.Equal("Curso redes", recurso.Titulo);
        Assert.Equal("https://example.local/recurso", recurso.Url);
        Assert.Equal(EstadoRecurso.EnUso, recurso.Estado);
        Assert.Equal(4, recurso.Rating?.Valor);
        Assert.Equal("Notas de revisión", recurso.Notas);
        Assert.Equal("ChatGPT", recurso.HerramientaIA);
        Assert.Equal("Prompt de resumen", recurso.PromptsUtilizados);
    }

    [Fact]
    public void Recurso_DebePermitirLimpiarCamposOpcionales()
    {
        var recurso = Recurso.Guardar(Guid.CreateVersion7(), TipoRecurso.Curso, "Curso redes");
        recurso.ActualizarUrl("https://example.local/recurso");
        recurso.CalificarCon(RatingRecurso.Crear(5));
        recurso.ActualizarNotas("Notas");
        recurso.RegistrarUsoDeIA("ChatGPT", "Prompt");

        recurso.ActualizarUrl(null);
        recurso.CalificarCon(null);
        recurso.ActualizarNotas(null);
        recurso.RegistrarUsoDeIA(null, null);

        Assert.Null(recurso.Url);
        Assert.Null(recurso.Rating);
        Assert.Null(recurso.Notas);
        Assert.Null(recurso.HerramientaIA);
        Assert.Null(recurso.PromptsUtilizados);
    }

    [Fact]
    public void Recurso_DebeRechazarTituloVacioAlActualizar()
    {
        var recurso = Recurso.Guardar(Guid.CreateVersion7(), TipoRecurso.Documentacion, "Documentación OSI");

        Assert.Throws<ArgumentException>(() => recurso.CambiarTitulo(" "));
    }

    [Fact]
    public void Recurso_DebeRechazarRatingFueraDeRango()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => RatingRecurso.Crear(0));
        Assert.Throws<ArgumentOutOfRangeException>(() => RatingRecurso.Crear(6));
    }

    [Fact]
    public void Recurso_DebeMarcarseComoEliminadoLogicamente()
    {
        var recurso = Recurso.Guardar(Guid.CreateVersion7(), TipoRecurso.Documentacion, "Documentación OSI");

        recurso.MarcarComoEliminado();

        Assert.NotNull(recurso.FechaEliminacionUtc);
    }
}
