using Aprendizaje.Aplicacion.Roadmap.Temas.ActualizarPercepcion;
using Aprendizaje.Aplicacion.Roadmap.Temas.ActualizarPlanificacion;
using Aprendizaje.Aplicacion.Roadmap.Temas.AsignarTemaAFase;
using Aprendizaje.Aplicacion.Roadmap.Temas.AsignarTemaPadre;
using Aprendizaje.Aplicacion.Roadmap.Temas.ConfigurarIntervaloRepaso;
using Aprendizaje.Aplicacion.Roadmap.Temas.DefinirCriteriosRelevantes;
using Aprendizaje.Aplicacion.Roadmap.Temas.DesmarcarCriterio;
using Aprendizaje.Aplicacion.Roadmap.Temas.MarcarCriterio;
using Aprendizaje.Dominio.Roadmap;
using Aprendizaje.Tests.Soporte;
using Xunit;

namespace Aprendizaje.Tests.Application.Roadmap;

public sealed class TemasApplicationTests
{
    [Fact]
    public async Task ActualizarPercepcion_DebeRetornarTemaNoEncontradoSinGuardar()
    {
        var temas = new FakeTemaRepository();
        var unitOfWork = new FakeUnitOfWork();
        var casoUso = new ActualizarPercepcionTemaCasoUso(temas, unitOfWork);

        var resultado = await casoUso.EjecutarAsync(
            new ActualizarPercepcionTemaSolicitud(Guid.CreateVersion7(), 4, 3),
            CancellationToken);

        Assert.False(resultado.Encontrado);
        Assert.Equal(0, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task ActualizarPercepcion_DebeActualizarValoresYGuardar()
    {
        var temas = new FakeTemaRepository();
        var unitOfWork = new FakeUnitOfWork();
        var tema = CrearTema();
        temas.Agregar(tema);
        var casoUso = new ActualizarPercepcionTemaCasoUso(temas, unitOfWork);

        var resultado = await casoUso.EjecutarAsync(
            new ActualizarPercepcionTemaSolicitud(tema.Id, 4, 2),
            CancellationToken);

        Assert.True(resultado.Encontrado);
        Assert.Equal(4, tema.DificultadPercibida?.Valor);
        Assert.Equal(2, tema.Confianza?.Valor);
        Assert.Equal(1, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task ActualizarPercepcion_DebeRechazarNivelInvalidoSinGuardar()
    {
        var temas = new FakeTemaRepository();
        var unitOfWork = new FakeUnitOfWork();
        var tema = CrearTema();
        temas.Agregar(tema);
        var casoUso = new ActualizarPercepcionTemaCasoUso(temas, unitOfWork);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => casoUso.EjecutarAsync(
            new ActualizarPercepcionTemaSolicitud(tema.Id, 6, 2),
            CancellationToken));

        Assert.Equal(0, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task ActualizarPlanificacion_DebeRetornarTemaNoEncontradoSinGuardar()
    {
        var temas = new FakeTemaRepository();
        var unitOfWork = new FakeUnitOfWork();
        var casoUso = new ActualizarPlanificacionTemaCasoUso(temas, unitOfWork);

        var resultado = await casoUso.EjecutarAsync(
            new ActualizarPlanificacionTemaSolicitud(
                Guid.CreateVersion7(),
                new DateOnly(2026, 8, 25),
                new DateOnly(2026, 9, 10)),
            CancellationToken);

        Assert.False(resultado.Encontrado);
        Assert.Equal(0, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task ActualizarPlanificacion_DebeActualizarFechasYGuardar()
    {
        var temas = new FakeTemaRepository();
        var unitOfWork = new FakeUnitOfWork();
        var tema = CrearTema();
        temas.Agregar(tema);
        var casoUso = new ActualizarPlanificacionTemaCasoUso(temas, unitOfWork);

        var resultado = await casoUso.EjecutarAsync(
            new ActualizarPlanificacionTemaSolicitud(
                tema.Id,
                new DateOnly(2026, 8, 25),
                new DateOnly(2026, 9, 10)),
            CancellationToken);

        Assert.True(resultado.Encontrado);
        Assert.Equal(new DateOnly(2026, 8, 25), tema.FechaInicio);
        Assert.Equal(new DateOnly(2026, 9, 10), tema.FechaFin);
        Assert.Equal(1, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task ActualizarPlanificacion_DebeRechazarFechasVaciasSinGuardar()
    {
        var temas = new FakeTemaRepository();
        var unitOfWork = new FakeUnitOfWork();
        var casoUso = new ActualizarPlanificacionTemaCasoUso(temas, unitOfWork);

        await Assert.ThrowsAsync<ArgumentException>(() => casoUso.EjecutarAsync(
            new ActualizarPlanificacionTemaSolicitud(Guid.CreateVersion7(), null, null),
            CancellationToken));

        Assert.Equal(0, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task ActualizarPlanificacion_DebePropagarInvarianteTemporalSinGuardar()
    {
        var temas = new FakeTemaRepository();
        var unitOfWork = new FakeUnitOfWork();
        var tema = CrearTema();
        temas.Agregar(tema);
        var casoUso = new ActualizarPlanificacionTemaCasoUso(temas, unitOfWork);

        await Assert.ThrowsAsync<InvalidOperationException>(() => casoUso.EjecutarAsync(
            new ActualizarPlanificacionTemaSolicitud(
                tema.Id,
                new DateOnly(2026, 9, 10),
                new DateOnly(2026, 8, 25)),
            CancellationToken));

        Assert.Equal(0, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task ConfigurarIntervaloRepaso_DebeRetornarTemaNoEncontradoSinGuardar()
    {
        var temas = new FakeTemaRepository();
        var unitOfWork = new FakeUnitOfWork();
        var casoUso = new ConfigurarIntervaloRepasoTemaCasoUso(temas, unitOfWork);

        var resultado = await casoUso.EjecutarAsync(
            new ConfigurarIntervaloRepasoTemaSolicitud(Guid.CreateVersion7(), 21),
            CancellationToken);

        Assert.False(resultado.Encontrado);
        Assert.Equal(0, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task ConfigurarIntervaloRepaso_DebeActualizarIntervaloYGuardar()
    {
        var temas = new FakeTemaRepository();
        var unitOfWork = new FakeUnitOfWork();
        var tema = CrearTema();
        temas.Agregar(tema);
        var casoUso = new ConfigurarIntervaloRepasoTemaCasoUso(temas, unitOfWork);

        var resultado = await casoUso.EjecutarAsync(
            new ConfigurarIntervaloRepasoTemaSolicitud(tema.Id, 21),
            CancellationToken);

        Assert.True(resultado.Encontrado);
        Assert.Equal(21, tema.IntervaloRepaso?.Dias);
        Assert.Equal(1, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task ConfigurarIntervaloRepaso_DebePermitirQuitarIntervaloPropio()
    {
        var temas = new FakeTemaRepository();
        var unitOfWork = new FakeUnitOfWork();
        var tema = CrearTema();
        temas.Agregar(tema);
        tema.ConfigurarIntervaloRepaso(Aprendizaje.Dominio.Roadmap.ValueObjects.IntervaloRepaso.Crear(21));
        var casoUso = new ConfigurarIntervaloRepasoTemaCasoUso(temas, unitOfWork);

        var resultado = await casoUso.EjecutarAsync(
            new ConfigurarIntervaloRepasoTemaSolicitud(tema.Id, null),
            CancellationToken);

        Assert.True(resultado.Encontrado);
        Assert.Null(tema.IntervaloRepaso);
        Assert.Equal(1, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task ConfigurarIntervaloRepaso_DebeRechazarDiasInvalidosSinGuardar()
    {
        var temas = new FakeTemaRepository();
        var unitOfWork = new FakeUnitOfWork();
        var tema = CrearTema();
        temas.Agregar(tema);
        var casoUso = new ConfigurarIntervaloRepasoTemaCasoUso(temas, unitOfWork);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => casoUso.EjecutarAsync(
            new ConfigurarIntervaloRepasoTemaSolicitud(tema.Id, 0),
            CancellationToken));

        Assert.Equal(0, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task AsignarTemaAFase_DebeRetornarTemaNoEncontradoSinGuardar()
    {
        var temas = new FakeTemaRepository();
        var fases = new FakeFaseRepository();
        var unitOfWork = new FakeUnitOfWork();
        var casoUso = new AsignarTemaAFaseCasoUso(temas, fases, unitOfWork);

        var resultado = await casoUso.EjecutarAsync(new AsignarTemaAFaseSolicitud(
            Guid.CreateVersion7(),
            Guid.CreateVersion7()), CancellationToken);

        Assert.Equal(AsignarTemaAFaseEstado.TemaNoEncontrado, resultado.Estado);
        Assert.Equal(0, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task AsignarTemaAFase_DebeRetornarFaseNoEncontradaSinGuardar()
    {
        var temas = new FakeTemaRepository();
        var fases = new FakeFaseRepository();
        var unitOfWork = new FakeUnitOfWork();
        var tema = CrearTema();
        temas.Agregar(tema);
        var casoUso = new AsignarTemaAFaseCasoUso(temas, fases, unitOfWork);

        var resultado = await casoUso.EjecutarAsync(new AsignarTemaAFaseSolicitud(
            tema.Id,
            Guid.CreateVersion7()), CancellationToken);

        Assert.Equal(AsignarTemaAFaseEstado.FaseNoEncontrada, resultado.Estado);
        Assert.Equal(0, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task AsignarTemaAFase_DebeRetornarUsuarioNoCoincideSinGuardar()
    {
        var temas = new FakeTemaRepository();
        var fases = new FakeFaseRepository();
        var unitOfWork = new FakeUnitOfWork();
        var tema = CrearTema();
        var fase = Fase.Crear(Guid.CreateVersion7(), "Otra fase", 1);
        temas.Agregar(tema);
        fases.Agregar(fase);
        var casoUso = new AsignarTemaAFaseCasoUso(temas, fases, unitOfWork);

        var resultado = await casoUso.EjecutarAsync(
            new AsignarTemaAFaseSolicitud(tema.Id, fase.Id),
            CancellationToken);

        Assert.Equal(AsignarTemaAFaseEstado.UsuarioNoCoincide, resultado.Estado);
        Assert.Equal(0, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task AsignarTemaAFase_DebeAsignarFaseYGuardar()
    {
        var temas = new FakeTemaRepository();
        var fases = new FakeFaseRepository();
        var unitOfWork = new FakeUnitOfWork();
        var tema = CrearTema();
        var fase = Fase.Crear(tema.UsuarioId, "Fundamentos", 1);
        temas.Agregar(tema);
        fases.Agregar(fase);
        var casoUso = new AsignarTemaAFaseCasoUso(temas, fases, unitOfWork);

        var resultado = await casoUso.EjecutarAsync(
            new AsignarTemaAFaseSolicitud(tema.Id, fase.Id),
            CancellationToken);

        Assert.Equal(AsignarTemaAFaseEstado.Actualizado, resultado.Estado);
        Assert.Equal(fase.Id, tema.FaseId);
        Assert.Equal(1, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task AsignarTemaPadre_DebeRetornarTemaNoEncontradoSinGuardar()
    {
        var temas = new FakeTemaRepository();
        var unitOfWork = new FakeUnitOfWork();
        var casoUso = new AsignarTemaPadreCasoUso(temas, unitOfWork);

        var resultado = await casoUso.EjecutarAsync(new AsignarTemaPadreSolicitud(
            Guid.CreateVersion7(),
            Guid.CreateVersion7()), CancellationToken);

        Assert.Equal(AsignarTemaPadreEstado.TemaNoEncontrado, resultado.Estado);
        Assert.Equal(0, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task AsignarTemaPadre_DebeRetornarTemaPadreNoEncontradoSinGuardar()
    {
        var temas = new FakeTemaRepository();
        var unitOfWork = new FakeUnitOfWork();
        var hijo = CrearTema();
        temas.Agregar(hijo);
        var casoUso = new AsignarTemaPadreCasoUso(temas, unitOfWork);

        var resultado = await casoUso.EjecutarAsync(new AsignarTemaPadreSolicitud(
            hijo.Id,
            Guid.CreateVersion7()), CancellationToken);

        Assert.Equal(AsignarTemaPadreEstado.TemaPadreNoEncontrado, resultado.Estado);
        Assert.Equal(0, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task AsignarTemaPadre_DebeRetornarUsuarioNoCoincideSinGuardar()
    {
        var temas = new FakeTemaRepository();
        var unitOfWork = new FakeUnitOfWork();
        var hijo = CrearTema();
        var padre = CrearTema();
        temas.Agregar(hijo);
        temas.Agregar(padre);
        var casoUso = new AsignarTemaPadreCasoUso(temas, unitOfWork);

        var resultado = await casoUso.EjecutarAsync(
            new AsignarTemaPadreSolicitud(hijo.Id, padre.Id),
            CancellationToken);

        Assert.Equal(AsignarTemaPadreEstado.UsuarioNoCoincide, resultado.Estado);
        Assert.Equal(0, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task AsignarTemaPadre_DebeRechazarSelfParentAntesDeConsultar()
    {
        var temas = new FakeTemaRepository();
        var unitOfWork = new FakeUnitOfWork();
        var temaId = Guid.CreateVersion7();
        var casoUso = new AsignarTemaPadreCasoUso(temas, unitOfWork);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            casoUso.EjecutarAsync(new AsignarTemaPadreSolicitud(temaId, temaId), CancellationToken));

        Assert.Equal(0, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task AsignarTemaPadre_DebeDetectarCicloDirectoSinGuardar()
    {
        var temas = new FakeTemaRepository();
        var unitOfWork = new FakeUnitOfWork();
        var usuarioId = Guid.CreateVersion7();
        var hijo = CrearTema(usuarioId);
        var padre = CrearTema(usuarioId);
        padre.AsignarTemaPadre(hijo.Id);
        temas.Agregar(hijo);
        temas.Agregar(padre);
        var casoUso = new AsignarTemaPadreCasoUso(temas, unitOfWork);

        var resultado = await casoUso.EjecutarAsync(
            new AsignarTemaPadreSolicitud(hijo.Id, padre.Id),
            CancellationToken);

        Assert.Equal(AsignarTemaPadreEstado.ConflictoJerarquia, resultado.Estado);
        Assert.Equal(0, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task AsignarTemaPadre_DebeAsignarPadreYGuardar()
    {
        var temas = new FakeTemaRepository();
        var unitOfWork = new FakeUnitOfWork();
        var usuarioId = Guid.CreateVersion7();
        var hijo = CrearTema(usuarioId);
        var padre = CrearTema(usuarioId);
        temas.Agregar(hijo);
        temas.Agregar(padre);
        var casoUso = new AsignarTemaPadreCasoUso(temas, unitOfWork);

        var resultado = await casoUso.EjecutarAsync(
            new AsignarTemaPadreSolicitud(hijo.Id, padre.Id),
            CancellationToken);

        Assert.Equal(AsignarTemaPadreEstado.Actualizado, resultado.Estado);
        Assert.Equal(padre.Id, hijo.TemaPadreId);
        Assert.Equal(1, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task DefinirCriteriosRelevantes_DebeRetornarTemaNoEncontradoSinGuardar()
    {
        var temas = new FakeTemaRepository();
        var unitOfWork = new FakeUnitOfWork();
        var casoUso = new DefinirCriteriosRelevantesTemaCasoUso(temas, unitOfWork);

        var resultado = await casoUso.EjecutarAsync(new DefinirCriteriosRelevantesTemaSolicitud(
            Guid.CreateVersion7(),
            [TipoCriterio.Teoria, TipoCriterio.Practica]), CancellationToken);

        Assert.Equal(DefinirCriteriosRelevantesTemaEstado.TemaNoEncontrado, resultado.Estado);
        Assert.Equal(0, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task DefinirCriteriosRelevantes_DebeDefinirYGuardar()
    {
        var temas = new FakeTemaRepository();
        var unitOfWork = new FakeUnitOfWork();
        var tema = CrearTema();
        temas.Agregar(tema);
        var casoUso = new DefinirCriteriosRelevantesTemaCasoUso(temas, unitOfWork);

        var resultado = await casoUso.EjecutarAsync(new DefinirCriteriosRelevantesTemaSolicitud(
            tema.Id,
            [TipoCriterio.Teoria, TipoCriterio.Practica]), CancellationToken);

        Assert.Equal(DefinirCriteriosRelevantesTemaEstado.Actualizado, resultado.Estado);
        Assert.Equal(2, tema.Criterios.Count);
        Assert.Equal(1, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task DefinirCriteriosRelevantes_DebeRetornarProgresoRegistradoSinGuardar()
    {
        var temas = new FakeTemaRepository();
        var unitOfWork = new FakeUnitOfWork();
        var tema = CrearTemaConCriterios();
        tema.MarcarCriterio(TipoCriterio.Teoria);
        temas.Agregar(tema);
        var casoUso = new DefinirCriteriosRelevantesTemaCasoUso(temas, unitOfWork);

        var resultado = await casoUso.EjecutarAsync(new DefinirCriteriosRelevantesTemaSolicitud(
            tema.Id,
            [TipoCriterio.Explicacion, TipoCriterio.Ejercicios]), CancellationToken);

        Assert.Equal(DefinirCriteriosRelevantesTemaEstado.ProgresoRegistrado, resultado.Estado);
        Assert.Equal(0, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task MarcarCriterio_DebeRetornarCriterioNoDefinidoSinGuardar()
    {
        var temas = new FakeTemaRepository();
        var unitOfWork = new FakeUnitOfWork();
        var tema = CrearTemaConCriterios();
        temas.Agregar(tema);
        var casoUso = new MarcarCriterioTemaCasoUso(temas, unitOfWork);

        var resultado = await casoUso.EjecutarAsync(new MarcarCriterioTemaSolicitud(
            tema.Id,
            TipoCriterio.Laboratorio), CancellationToken);

        Assert.Equal(MarcarCriterioTemaEstado.CriterioNoDefinido, resultado.Estado);
        Assert.Equal(0, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task MarcarCriterio_DebeMarcarYGuardar()
    {
        var temas = new FakeTemaRepository();
        var unitOfWork = new FakeUnitOfWork();
        var tema = CrearTemaConCriterios();
        temas.Agregar(tema);
        var casoUso = new MarcarCriterioTemaCasoUso(temas, unitOfWork);

        var resultado = await casoUso.EjecutarAsync(new MarcarCriterioTemaSolicitud(
            tema.Id,
            TipoCriterio.Teoria), CancellationToken);

        Assert.Equal(MarcarCriterioTemaEstado.Actualizado, resultado.Estado);
        Assert.True(tema.Criterios.Single(c => c.Tipo == TipoCriterio.Teoria).Cumplido);
        Assert.Equal(1, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task DesmarcarCriterio_DebeRetornarCriterioNoDefinidoSinGuardar()
    {
        var temas = new FakeTemaRepository();
        var unitOfWork = new FakeUnitOfWork();
        var tema = CrearTemaConCriterios();
        temas.Agregar(tema);
        var casoUso = new DesmarcarCriterioTemaCasoUso(temas, unitOfWork);

        var resultado = await casoUso.EjecutarAsync(new DesmarcarCriterioTemaSolicitud(
            tema.Id,
            TipoCriterio.Laboratorio), CancellationToken);

        Assert.Equal(DesmarcarCriterioTemaEstado.CriterioNoDefinido, resultado.Estado);
        Assert.Equal(0, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task DesmarcarCriterio_DebeDesmarcarYGuardar()
    {
        var temas = new FakeTemaRepository();
        var unitOfWork = new FakeUnitOfWork();
        var tema = CrearTemaConCriterios();
        tema.MarcarCriterio(TipoCriterio.Teoria);
        temas.Agregar(tema);
        var casoUso = new DesmarcarCriterioTemaCasoUso(temas, unitOfWork);

        var resultado = await casoUso.EjecutarAsync(new DesmarcarCriterioTemaSolicitud(
            tema.Id,
            TipoCriterio.Teoria), CancellationToken);

        Assert.Equal(DesmarcarCriterioTemaEstado.Actualizado, resultado.Estado);
        Assert.False(tema.Criterios.Single(c => c.Tipo == TipoCriterio.Teoria).Cumplido);
        Assert.Equal(1, unitOfWork.GuardarCambiosLlamadas);
    }

    private static Tema CrearTema(Guid? usuarioId = null) =>
        Tema.Crear(usuarioId ?? Guid.CreateVersion7(), "Modelo OSI", TipoConocimiento.Conceptual);

    private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

    private static Tema CrearTemaConCriterios()
    {
        var tema = CrearTema();
        tema.DefinirCriteriosRelevantes([TipoCriterio.Teoria, TipoCriterio.Practica]);

        return tema;
    }
}
