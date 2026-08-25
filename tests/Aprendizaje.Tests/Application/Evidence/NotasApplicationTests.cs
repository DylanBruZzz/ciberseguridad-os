using Aprendizaje.Aplicacion.Evidence.Notas.Comun;
using Aprendizaje.Aplicacion.Evidence.Notas.CrearNotaParaArtefactoTecnico;
using Aprendizaje.Aplicacion.Evidence.Notas.CrearNotaParaLaboratorio;
using Aprendizaje.Aplicacion.Evidence.Notas.CrearNotaParaProyecto;
using Aprendizaje.Aplicacion.Evidence.Notas.CrearNotaParaTema;
using Aprendizaje.Aplicacion.Evidence.Notas.CrearNotaParaWriteup;
using Aprendizaje.Aplicacion.Evidence.Notas.ListarNotas;
using Aprendizaje.Aplicacion.Evidence.Notas.ObtenerNotaPorId;
using Aprendizaje.Dominio.Evidence;
using Aprendizaje.Dominio.Roadmap;
using Aprendizaje.Tests.Soporte;
using Xunit;

namespace Aprendizaje.Tests.Application.Evidence;

public sealed class NotasApplicationTests
{
    [Theory]
    [InlineData(PadreNota.Tema)]
    [InlineData(PadreNota.Proyecto)]
    [InlineData(PadreNota.Laboratorio)]
    [InlineData(PadreNota.Writeup)]
    [InlineData(PadreNota.ArtefactoTecnico)]
    public async Task CrearNota_DebeRetornarPadreNoEncontradoSinGuardar(PadreNota padre)
    {
        var contexto = CrearContexto(padre, agregarPadre: false);

        var resultado = await contexto.EjecutarAsync();

        Assert.Equal(CrearNotaEstado.PadreNoEncontrado, resultado.Estado);
        Assert.Equal(0, contexto.Notas.AgregarLlamadas);
        Assert.Equal(0, contexto.UnitOfWork.GuardarCambiosLlamadas);
    }

    [Theory]
    [InlineData(PadreNota.Tema)]
    [InlineData(PadreNota.Proyecto)]
    [InlineData(PadreNota.Laboratorio)]
    [InlineData(PadreNota.Writeup)]
    [InlineData(PadreNota.ArtefactoTecnico)]
    public async Task CrearNota_DebeRetornarUsuarioNoCoincideSinGuardar(PadreNota padre)
    {
        var contexto = CrearContexto(padre, usuarioPadreId: Guid.CreateVersion7());

        var resultado = await contexto.EjecutarAsync();

        Assert.Equal(CrearNotaEstado.UsuarioNoCoincide, resultado.Estado);
        Assert.Equal(0, contexto.Notas.AgregarLlamadas);
        Assert.Equal(0, contexto.UnitOfWork.GuardarCambiosLlamadas);
    }

    [Theory]
    [InlineData(PadreNota.Tema)]
    [InlineData(PadreNota.Proyecto)]
    [InlineData(PadreNota.Laboratorio)]
    [InlineData(PadreNota.Writeup)]
    [InlineData(PadreNota.ArtefactoTecnico)]
    public async Task CrearNota_DebeAgregarNotaCorrectaYGuardar(PadreNota padre)
    {
        var contexto = CrearContexto(padre);

        var resultado = await contexto.EjecutarAsync();

        Assert.Equal(CrearNotaEstado.Creada, resultado.Estado);
        Assert.NotEqual(Guid.Empty, resultado.Id);
        Assert.Equal(contexto.UsuarioId, resultado.UsuarioId);
        Assert.Equal("Observación de cierre", resultado.Texto);
        Assert.Equal(TipoNota.Autoexplicacion, resultado.Tipo);
        Assert.NotNull(contexto.Notas.UltimaNotaAgregada);
        AssertPadreUnico(contexto.Notas.UltimaNotaAgregada, padre, contexto.PadreId);
        AssertPadreUnico(resultado, padre, contexto.PadreId);
        Assert.Equal(1, contexto.Notas.AgregarLlamadas);
        Assert.Equal(1, contexto.UnitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task CrearNota_DebeRechazarUsuarioVacioSinGuardar()
    {
        var contexto = CrearContexto(PadreNota.Proyecto, agregarPadre: false, usuarioId: Guid.Empty);

        await Assert.ThrowsAsync<ArgumentException>(() => contexto.EjecutarAsync());

        Assert.Equal(0, contexto.Notas.AgregarLlamadas);
        Assert.Equal(0, contexto.UnitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task ObtenerNotaPorId_DebeRetornarNotaExistente()
    {
        var notas = new FakeNotaRepository();
        var nota = Nota.SobreProyecto(Guid.CreateVersion7(), Guid.CreateVersion7(), "Observación de cierre");
        notas.Agregar(nota);
        var casoUso = new ObtenerNotaPorIdCasoUso(notas);

        var resultado = await casoUso.EjecutarAsync(nota.Id, CancellationToken);

        Assert.True(resultado.Encontrada);
        Assert.NotNull(resultado.Nota);
        Assert.Equal(nota.Id, resultado.Nota.Id);
        Assert.Equal(nota.ProyectoId, resultado.Nota.ProyectoId);
        Assert.Equal(nota.Texto, resultado.Nota.Texto);
    }

    [Fact]
    public async Task ObtenerNotaPorId_DebeRetornarNoEncontrada()
    {
        var notas = new FakeNotaRepository();
        var casoUso = new ObtenerNotaPorIdCasoUso(notas);

        var resultado = await casoUso.EjecutarAsync(Guid.CreateVersion7(), CancellationToken);

        Assert.False(resultado.Encontrada);
        Assert.Null(resultado.Nota);
    }

    [Fact]
    public async Task ListarNotas_DebeMapearColeccionDelRepository()
    {
        var notas = new FakeNotaRepository();
        var usuarioId = Guid.CreateVersion7();
        var nota = Nota.SobreProyecto(usuarioId, Guid.CreateVersion7(), "Observación de cierre");
        notas.Agregar(nota);
        notas.Agregar(Nota.SobreProyecto(Guid.CreateVersion7(), Guid.CreateVersion7(), "Otra nota"));
        var casoUso = new ListarNotasCasoUso(notas);

        var resultado = await casoUso.EjecutarAsync(new ListarNotasSolicitud(usuarioId), CancellationToken);

        var resumen = Assert.Single(resultado.Notas);
        Assert.Equal(nota.Id, resumen.Id);
        Assert.Equal(usuarioId, resumen.UsuarioId);
        Assert.Equal(nota.ProyectoId, resumen.ProyectoId);
        Assert.Equal(nota.Texto, resumen.Texto);
        Assert.Equal(nota.Tipo, resumen.Tipo);
    }

    [Fact]
    public async Task ListarNotas_DebeRetornarListaVacia()
    {
        var notas = new FakeNotaRepository();
        var casoUso = new ListarNotasCasoUso(notas);

        var resultado = await casoUso.EjecutarAsync(
            new ListarNotasSolicitud(Guid.CreateVersion7()),
            CancellationToken);

        Assert.Empty(resultado.Notas);
    }

    private static ContextoCreacionNota CrearContexto(
        PadreNota padre,
        bool agregarPadre = true,
        Guid? usuarioId = null,
        Guid? usuarioPadreId = null)
    {
        var notas = new FakeNotaRepository();
        var temas = new FakeTemaRepository();
        var proyectos = new FakeProyectoRepository();
        var laboratorios = new FakeLaboratorioRepository();
        var writeups = new FakeWriteupRepository();
        var artefactosTecnicos = new FakeArtefactoTecnicoRepository();
        var unitOfWork = new FakeUnitOfWork();
        var usuarioNotaId = usuarioId ?? Guid.CreateVersion7();
        var propietarioPadreId = usuarioPadreId ?? usuarioNotaId;
        var padreId = agregarPadre
            ? AgregarPadre(
                padre,
                propietarioPadreId,
                temas,
                proyectos,
                laboratorios,
                writeups,
                artefactosTecnicos)
            : Guid.CreateVersion7();

        return new ContextoCreacionNota(
            padre,
            padreId,
            usuarioNotaId,
            notas,
            unitOfWork,
            () => EjecutarCreacionAsync(
                padre,
                padreId,
                usuarioNotaId,
                notas,
                temas,
                proyectos,
                laboratorios,
                writeups,
                artefactosTecnicos,
                unitOfWork));
    }

    private static Guid AgregarPadre(
        PadreNota padre,
        Guid usuarioPadreId,
        FakeTemaRepository temas,
        FakeProyectoRepository proyectos,
        FakeLaboratorioRepository laboratorios,
        FakeWriteupRepository writeups,
        FakeArtefactoTecnicoRepository artefactosTecnicos)
    {
        switch (padre)
        {
            case PadreNota.Tema:
                var tema = Tema.Crear(usuarioPadreId, "Modelo OSI", TipoConocimiento.Conceptual);
                temas.Agregar(tema);
                return tema.Id;
            case PadreNota.Proyecto:
                var proyecto = Proyecto.Crear(usuarioPadreId, "Analizador OSI");
                proyectos.Agregar(proyecto);
                return proyecto.Id;
            case PadreNota.Laboratorio:
                var laboratorio = Laboratorio.Crear(usuarioPadreId, "Laboratorio OSI");
                laboratorios.Agregar(laboratorio);
                return laboratorio.Id;
            case PadreNota.Writeup:
                var writeup = Writeup.Crear(usuarioPadreId, "Writeup OSI");
                writeups.Agregar(writeup);
                return writeup.Id;
            case PadreNota.ArtefactoTecnico:
                var artefactoTecnico = ArtefactoTecnico.Crear(
                    usuarioPadreId,
                    TipoArtefacto.Cheatsheet,
                    "Filtros OSI");
                artefactosTecnicos.Agregar(artefactoTecnico);
                return artefactoTecnico.Id;
            default:
                throw new ArgumentOutOfRangeException(nameof(padre));
        }
    }

    private static async Task<CrearNotaResultado> EjecutarCreacionAsync(
        PadreNota padre,
        Guid padreId,
        Guid usuarioId,
        FakeNotaRepository notas,
        FakeTemaRepository temas,
        FakeProyectoRepository proyectos,
        FakeLaboratorioRepository laboratorios,
        FakeWriteupRepository writeups,
        FakeArtefactoTecnicoRepository artefactosTecnicos,
        FakeUnitOfWork unitOfWork)
    {
        const string texto = " Observación de cierre ";
        const TipoNota tipo = TipoNota.Autoexplicacion;

        return padre switch
        {
            PadreNota.Tema => await new CrearNotaParaTemaCasoUso(notas, temas, unitOfWork).EjecutarAsync(
                new CrearNotaParaTemaSolicitud(usuarioId, padreId, texto, tipo),
                CancellationToken),
            PadreNota.Proyecto => await new CrearNotaParaProyectoCasoUso(notas, proyectos, unitOfWork).EjecutarAsync(
                new CrearNotaParaProyectoSolicitud(usuarioId, padreId, texto, tipo),
                CancellationToken),
            PadreNota.Laboratorio => await new CrearNotaParaLaboratorioCasoUso(notas, laboratorios, unitOfWork).EjecutarAsync(
                new CrearNotaParaLaboratorioSolicitud(usuarioId, padreId, texto, tipo),
                CancellationToken),
            PadreNota.Writeup => await new CrearNotaParaWriteupCasoUso(notas, writeups, unitOfWork).EjecutarAsync(
                new CrearNotaParaWriteupSolicitud(usuarioId, padreId, texto, tipo),
                CancellationToken),
            PadreNota.ArtefactoTecnico => await new CrearNotaParaArtefactoTecnicoCasoUso(
                    notas,
                    artefactosTecnicos,
                    unitOfWork)
                .EjecutarAsync(
                    new CrearNotaParaArtefactoTecnicoSolicitud(usuarioId, padreId, texto, tipo),
                    CancellationToken),
            _ => throw new ArgumentOutOfRangeException(nameof(padre))
        };
    }

    private static void AssertPadreUnico(Nota nota, PadreNota padre, Guid padreId)
    {
        Assert.Equal(padre == PadreNota.Tema ? padreId : null, nota.TemaId);
        Assert.Equal(padre == PadreNota.Proyecto ? padreId : null, nota.ProyectoId);
        Assert.Equal(padre == PadreNota.Laboratorio ? padreId : null, nota.LaboratorioId);
        Assert.Equal(padre == PadreNota.Writeup ? padreId : null, nota.WriteupId);
        Assert.Equal(padre == PadreNota.ArtefactoTecnico ? padreId : null, nota.ArtefactoTecnicoId);
    }

    private static void AssertPadreUnico(CrearNotaResultado resultado, PadreNota padre, Guid padreId)
    {
        Assert.Equal(padre == PadreNota.Tema ? padreId : null, resultado.TemaId);
        Assert.Equal(padre == PadreNota.Proyecto ? padreId : null, resultado.ProyectoId);
        Assert.Equal(padre == PadreNota.Laboratorio ? padreId : null, resultado.LaboratorioId);
        Assert.Equal(padre == PadreNota.Writeup ? padreId : null, resultado.WriteupId);
        Assert.Equal(padre == PadreNota.ArtefactoTecnico ? padreId : null, resultado.ArtefactoTecnicoId);
    }

    public enum PadreNota
    {
        Tema,
        Proyecto,
        Laboratorio,
        Writeup,
        ArtefactoTecnico
    }

    private sealed record ContextoCreacionNota(
        PadreNota Padre,
        Guid PadreId,
        Guid UsuarioId,
        FakeNotaRepository Notas,
        FakeUnitOfWork UnitOfWork,
        Func<Task<CrearNotaResultado>> EjecutarAsync);

    private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;
}
