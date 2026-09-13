using Aprendizaje.Dominio.Roadmap;
using Aprendizaje.Dominio.Roadmap.Eventos;
using Aprendizaje.Dominio.Roadmap.ValueObjects;
using Xunit;

namespace Aprendizaje.Tests.Dominio.Roadmap;

public sealed class TemaTests
{
    [Fact]
    public void Crear_DebeExponerObjetivosComoColeccionVacia()
    {
        var tema = CrearTema();

        Assert.NotNull(tema.Objetivos);
        Assert.Empty(tema.Objetivos);
    }

    [Fact]
    public void EstablecerObjetivos_DebeGuardarObjetivosValidosNormalizados()
    {
        var tema = CrearTema();

        tema.EstablecerObjetivos([
            "  Comprender el modelo OSI  ",
            " ",
            "",
            "Diferenciar TCP y UDP",
            "\t"
        ]);

        Assert.Equal(
            ["Comprender el modelo OSI", "Diferenciar TCP y UDP"],
            tema.Objetivos);
    }

    [Fact]
    public void AsignarFase_DebeActualizarFaseId()
    {
        var tema = CrearTema();
        var faseId = Guid.CreateVersion7();

        tema.AsignarFase(faseId);

        Assert.Equal(faseId, tema.FaseId);
    }

    [Fact]
    public void AsignarFase_DebePermitirQuitarFase()
    {
        var tema = CrearTema();
        tema.AsignarFase(Guid.CreateVersion7());

        tema.AsignarFase(null);

        Assert.Null(tema.FaseId);
    }

    [Fact]
    public void AsignarTemaPadre_DebeActualizarTemaPadreId()
    {
        var tema = CrearTema();
        var temaPadreId = Guid.CreateVersion7();

        tema.AsignarTemaPadre(temaPadreId);

        Assert.Equal(temaPadreId, tema.TemaPadreId);
    }

    [Fact]
    public void AsignarTemaPadre_DebeRechazarSelfParent()
    {
        var tema = CrearTema();

        Assert.Throws<InvalidOperationException>(() => tema.AsignarTemaPadre(tema.Id));
    }

    [Fact]
    public void DefinirCriteriosRelevantes_DebeAceptarDosCriteriosDistintos()
    {
        var tema = CrearTema();

        tema.DefinirCriteriosRelevantes([Criterio(TipoCriterio.Teoria, "  Explicar el modelo OSI  "), Criterio(TipoCriterio.Practica, "Aplicar el modelo en diagnostico")]);

        Assert.Equal(2, tema.Criterios.Count);
        Assert.Contains(tema.Criterios, criterio => criterio.Tipo == TipoCriterio.Teoria);
        Assert.Contains(tema.Criterios, criterio => criterio.Tipo == TipoCriterio.Practica);
        Assert.Contains(tema.Criterios, criterio => criterio.Descripcion == "Explicar el modelo OSI");
        Assert.All(tema.Criterios, criterio =>
        {
            Assert.False(criterio.Cumplido);
            Assert.Null(criterio.FechaCumplido);
        });
    }

    [Fact]
    public void DefinirCriteriosRelevantes_DebeAceptarHastaCincoCriterios()
    {
        var tema = CrearTema();

        tema.DefinirCriteriosRelevantes([
            Criterio(TipoCriterio.Teoria),
            Criterio(TipoCriterio.Practica),
            Criterio(TipoCriterio.Explicacion),
            Criterio(TipoCriterio.Ejercicios),
            Criterio(TipoCriterio.Laboratorio)
        ]);

        Assert.Equal(5, tema.Criterios.Count);
    }

    [Fact]
    public void DefinirCriteriosRelevantes_DebeNormalizarDuplicados()
    {
        var tema = CrearTema();

        tema.DefinirCriteriosRelevantes([
            Criterio(TipoCriterio.Teoria, "Primera descripcion"),
            Criterio(TipoCriterio.Teoria, "Descripcion duplicada ignorada"),
            Criterio(TipoCriterio.Practica),
            Criterio(TipoCriterio.Practica)
        ]);

        Assert.Equal([TipoCriterio.Teoria, TipoCriterio.Practica], tema.Criterios.Select(c => c.Tipo));
        Assert.Equal("Primera descripcion", ObtenerCriterio(tema, TipoCriterio.Teoria).Descripcion);
    }

    [Fact]
    public void DefinirCriteriosRelevantes_DebeRechazarMenosDeDosCriteriosDistintos()
    {
        var tema = CrearTema();

        Assert.Throws<ArgumentException>(() =>
            tema.DefinirCriteriosRelevantes([Criterio(TipoCriterio.Teoria), Criterio(TipoCriterio.Teoria)]));
    }

    [Fact]
    public void DefinirCriteriosRelevantes_DebeRechazarDescripcionVacia()
    {
        var tema = CrearTema();

        Assert.Throws<ArgumentException>(() =>
            tema.DefinirCriteriosRelevantes([Criterio(TipoCriterio.Teoria, " "), Criterio(TipoCriterio.Practica)]));
    }

    [Fact]
    public void DefinirCriteriosRelevantes_DebeRechazarDescripcionDemasiadoLarga()
    {
        var tema = CrearTema();

        Assert.Throws<ArgumentException>(() =>
            tema.DefinirCriteriosRelevantes([
                Criterio(TipoCriterio.Teoria, new string('a', CriterioTema.DescripcionMaxLength + 1)),
                Criterio(TipoCriterio.Practica)
            ]));
    }

    [Fact]
    public void MarcarCriterio_DebeMarcarCriterioExistente()
    {
        var tema = CrearTemaConCriterios();
        var antes = DateTime.UtcNow;

        tema.MarcarCriterio(TipoCriterio.Teoria);

        var criterio = ObtenerCriterio(tema, TipoCriterio.Teoria);
        Assert.True(criterio.Cumplido);
        Assert.Equal("Descripcion Teoria", criterio.Descripcion);
        Assert.NotNull(criterio.FechaCumplido);
        Assert.True(criterio.FechaCumplido >= antes);
        Assert.False(ObtenerCriterio(tema, TipoCriterio.Practica).Cumplido);
    }

    [Fact]
    public void MarcarCriterio_DebeSerIdempotenteYSinReemplazarFechaOriginal()
    {
        var tema = CrearTemaConCriterios();
        tema.MarcarCriterio(TipoCriterio.Teoria);
        var fechaOriginal = ObtenerCriterio(tema, TipoCriterio.Teoria).FechaCumplido;

        tema.MarcarCriterio(TipoCriterio.Teoria);

        Assert.Equal(fechaOriginal, ObtenerCriterio(tema, TipoCriterio.Teoria).FechaCumplido);
    }

    [Fact]
    public void MarcarCriterio_DebeRechazarCriterioNoDefinido()
    {
        var tema = CrearTemaConCriterios();

        Assert.Throws<InvalidOperationException>(() => tema.MarcarCriterio(TipoCriterio.Laboratorio));
    }

    [Fact]
    public void DesmarcarCriterio_DebeQuitarCumplimientoYFecha()
    {
        var tema = CrearTemaConCriterios();
        tema.MarcarCriterio(TipoCriterio.Teoria);

        tema.DesmarcarCriterio(TipoCriterio.Teoria);

        var criterio = ObtenerCriterio(tema, TipoCriterio.Teoria);
        Assert.False(criterio.Cumplido);
        Assert.Equal("Descripcion Teoria", criterio.Descripcion);
        Assert.Null(criterio.FechaCumplido);
    }

    [Fact]
    public void DesmarcarCriterio_DebeRechazarCriterioNoDefinido()
    {
        var tema = CrearTemaConCriterios();

        Assert.Throws<InvalidOperationException>(() => tema.DesmarcarCriterio(TipoCriterio.Laboratorio));
    }

    [Fact]
    public void DefinirCriteriosRelevantes_DebePermitirRedefinirSinProgreso()
    {
        var tema = CrearTemaConCriterios();

        tema.DefinirCriteriosRelevantes([Criterio(TipoCriterio.Explicacion), Criterio(TipoCriterio.Ejercicios)]);

        Assert.Equal([TipoCriterio.Explicacion, TipoCriterio.Ejercicios], tema.Criterios.Select(c => c.Tipo));
    }

    [Fact]
    public void DefinirCriteriosRelevantes_DebeRechazarRedefinirConProgreso()
    {
        var tema = CrearTemaConCriterios();
        tema.MarcarCriterio(TipoCriterio.Teoria);

        Assert.Throws<InvalidOperationException>(() =>
            tema.DefinirCriteriosRelevantes([Criterio(TipoCriterio.Explicacion), Criterio(TipoCriterio.Ejercicios)]));
    }

    [Fact]
    public void ActualizarDificultadPercibida_DebeAceptarNivelValido()
    {
        var tema = CrearTema();

        tema.ActualizarDificultadPercibida(NivelPercepcion.Crear(4));

        Assert.NotNull(tema.DificultadPercibida);
        Assert.Equal(4, tema.DificultadPercibida.Valor);
    }

    [Fact]
    public void ActualizarConfianza_DebePermitirQuitarValor()
    {
        var tema = CrearTema();
        tema.ActualizarConfianza(NivelPercepcion.Crear(3));

        tema.ActualizarConfianza(null);

        Assert.Null(tema.Confianza);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    public void NivelPercepcion_Crear_DebeRechazarValoresFueraDeRango(int valor)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => NivelPercepcion.Crear(valor));
    }

    [Fact]
    public void IniciarYFinalizarEstudio_DebeRegistrarFechasValidas()
    {
        var tema = CrearTema();

        tema.IniciarEstudio(new DateOnly(2026, 8, 25));
        tema.FinalizarEstudio(new DateOnly(2026, 9, 10));

        Assert.Equal(new DateOnly(2026, 8, 25), tema.FechaInicio);
        Assert.Equal(new DateOnly(2026, 9, 10), tema.FechaFin);
    }

    [Fact]
    public void IniciarEstudio_DebeRechazarInicioPosteriorAFinRegistrado()
    {
        var tema = CrearTema();
        tema.FinalizarEstudio(new DateOnly(2026, 9, 10));

        Assert.Throws<InvalidOperationException>(() => tema.IniciarEstudio(new DateOnly(2026, 9, 11)));
    }

    [Fact]
    public void FinalizarEstudio_DebeRechazarFinAnteriorAInicioRegistrado()
    {
        var tema = CrearTema();
        tema.IniciarEstudio(new DateOnly(2026, 8, 25));

        Assert.Throws<InvalidOperationException>(() => tema.FinalizarEstudio(new DateOnly(2026, 8, 24)));
    }

    [Fact]
    public void ConfigurarIntervaloRepaso_DebeAceptarIntervaloValidoYNull()
    {
        var tema = CrearTema();

        tema.ConfigurarIntervaloRepaso(IntervaloRepaso.Crear(21));
        Assert.NotNull(tema.IntervaloRepaso);
        Assert.Equal(21, tema.IntervaloRepaso.Dias);

        tema.ConfigurarIntervaloRepaso(null);
        Assert.Null(tema.IntervaloRepaso);
    }

    [Fact]
    public void IntervaloRepaso_Crear_DebeRechazarDiasNoPositivos()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => IntervaloRepaso.Crear(0));
    }

    [Fact]
    public void EstaDominado_DebeSerFalseSinCriteriosMarcados()
    {
        var tema = CrearTemaConCriterios();

        Assert.False(tema.EstaDominado());
    }

    [Fact]
    public void EstaDominado_DebeSerFalseConUnCriterioMarcado()
    {
        var tema = CrearTemaConCriterios();

        tema.MarcarCriterio(TipoCriterio.Teoria);

        Assert.False(tema.EstaDominado());
    }

    [Fact]
    public void EstaDominado_DebeSerTrueConTodosLosCriteriosMarcados()
    {
        var tema = CrearTemaConCriterios();

        tema.MarcarCriterio(TipoCriterio.Teoria);
        tema.MarcarCriterio(TipoCriterio.Practica);

        Assert.True(tema.EstaDominado());
    }

    [Fact]
    public void MarcarCriterio_NoDebeGenerarTemaDominadoEventoAntesDeCompletarTodos()
    {
        var tema = CrearTemaConCriterios();

        tema.MarcarCriterio(TipoCriterio.Teoria);

        Assert.Empty(tema.EventosDominio);
    }

    [Fact]
    public void MarcarCriterio_DebeGenerarTemaDominadoEventoAlCompletarUltimoCriterio()
    {
        var tema = CrearTemaConCriterios();
        tema.MarcarCriterio(TipoCriterio.Teoria);
        var antes = DateTime.UtcNow;

        tema.MarcarCriterio(TipoCriterio.Practica);

        var evento = Assert.IsType<TemaDominadoEvento>(Assert.Single(tema.EventosDominio));
        Assert.Equal(tema.Id, evento.TemaId);
        Assert.True(evento.OcurrioEnUtc >= antes);
    }

    [Fact]
    public void MarcarCriterio_NoDebeGenerarOtroTemaDominadoEventoSiYaEstabaDominado()
    {
        var tema = CrearTemaConCriterios();
        tema.MarcarCriterio(TipoCriterio.Teoria);
        tema.MarcarCriterio(TipoCriterio.Practica);

        tema.MarcarCriterio(TipoCriterio.Practica);

        Assert.Single(tema.EventosDominio);
    }

    private static Tema CrearTema() =>
        Tema.Crear(Guid.CreateVersion7(), "Modelo OSI", TipoConocimiento.Conceptual);

    private static Tema CrearTemaConCriterios()
    {
        var tema = CrearTema();
        tema.DefinirCriteriosRelevantes([Criterio(TipoCriterio.Teoria), Criterio(TipoCriterio.Practica)]);

        return tema;
    }

    private static DefinicionCriterioTema Criterio(TipoCriterio tipo, string? descripcion = null) =>
        new(tipo, descripcion ?? $"Descripcion {tipo}");

    private static CriterioTema ObtenerCriterio(Tema tema, TipoCriterio tipo) =>
        tema.Criterios.Single(criterio => criterio.Tipo == tipo);
}
