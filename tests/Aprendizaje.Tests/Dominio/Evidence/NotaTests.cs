using Aprendizaje.Dominio.Evidence;
using Xunit;

namespace Aprendizaje.Tests.Dominio.Evidence;

public sealed class NotaTests
{
    [Theory]
    [InlineData(PadreNota.Tema)]
    [InlineData(PadreNota.Proyecto)]
    [InlineData(PadreNota.Laboratorio)]
    [InlineData(PadreNota.Writeup)]
    [InlineData(PadreNota.ArtefactoTecnico)]
    public void CrearSobrePadre_DebeInformarSoloElPadreCorrespondiente(PadreNota padre)
    {
        var usuarioId = Guid.CreateVersion7();
        var padreId = Guid.CreateVersion7();

        var nota = CrearNota(padre, usuarioId, padreId, "Observación de cierre", TipoNota.Hallazgo);

        Assert.NotEqual(Guid.Empty, nota.Id);
        Assert.Equal(usuarioId, nota.UsuarioId);
        Assert.Equal("Observación de cierre", nota.Texto);
        Assert.Equal(TipoNota.Hallazgo, nota.Tipo);
        Assert.True(nota.Fecha <= DateTime.UtcNow);
        AssertPadreUnico(nota, padre, padreId);
        Assert.Null(nota.FechaEliminacionUtc);
    }

    [Theory]
    [InlineData(PadreNota.Tema)]
    [InlineData(PadreNota.Proyecto)]
    [InlineData(PadreNota.Laboratorio)]
    [InlineData(PadreNota.Writeup)]
    [InlineData(PadreNota.ArtefactoTecnico)]
    public void CrearSobrePadre_DebeRechazarPadreVacio(PadreNota padre)
    {
        Assert.Throws<ArgumentException>(() => CrearNota(
            padre,
            Guid.CreateVersion7(),
            Guid.Empty,
            "Observación de cierre",
            TipoNota.Nota));
    }

    [Fact]
    public void CrearSobrePadre_DebeRechazarUsuarioVacio()
    {
        Assert.Throws<ArgumentException>(() => Nota.SobreProyecto(
            Guid.Empty,
            Guid.CreateVersion7(),
            "Observación de cierre"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void CrearSobrePadre_DebeRechazarTextoVacio(string texto)
    {
        Assert.Throws<ArgumentException>(() => Nota.SobreProyecto(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            texto));
    }

    [Fact]
    public void CrearSobrePadre_DebeNormalizarTexto()
    {
        var nota = Nota.SobreProyecto(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            "  Observación de cierre  ");

        Assert.Equal("Observación de cierre", nota.Texto);
    }

    [Fact]
    public void MarcarComoEliminado_DebeRegistrarFechaEliminacion()
    {
        var nota = Nota.SobreProyecto(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            "Observación de cierre");

        nota.MarcarComoEliminado();

        Assert.NotNull(nota.FechaEliminacionUtc);
    }

    private static Nota CrearNota(
        PadreNota padre,
        Guid usuarioId,
        Guid padreId,
        string texto,
        TipoNota tipo) =>
        padre switch
        {
            PadreNota.Tema => Nota.SobreTema(usuarioId, padreId, texto, tipo),
            PadreNota.Proyecto => Nota.SobreProyecto(usuarioId, padreId, texto, tipo),
            PadreNota.Laboratorio => Nota.SobreLaboratorio(usuarioId, padreId, texto, tipo),
            PadreNota.Writeup => Nota.SobreWriteup(usuarioId, padreId, texto, tipo),
            PadreNota.ArtefactoTecnico => Nota.SobreArtefactoTecnico(usuarioId, padreId, texto, tipo),
            _ => throw new ArgumentOutOfRangeException(nameof(padre))
        };

    private static void AssertPadreUnico(Nota nota, PadreNota padre, Guid padreId)
    {
        Assert.Equal(padre == PadreNota.Tema ? padreId : null, nota.TemaId);
        Assert.Equal(padre == PadreNota.Proyecto ? padreId : null, nota.ProyectoId);
        Assert.Equal(padre == PadreNota.Laboratorio ? padreId : null, nota.LaboratorioId);
        Assert.Equal(padre == PadreNota.Writeup ? padreId : null, nota.WriteupId);
        Assert.Equal(padre == PadreNota.ArtefactoTecnico ? padreId : null, nota.ArtefactoTecnicoId);
    }

    public enum PadreNota
    {
        Tema,
        Proyecto,
        Laboratorio,
        Writeup,
        ArtefactoTecnico
    }
}
