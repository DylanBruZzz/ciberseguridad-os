using Aprendizaje.Aplicacion.Evidence.Laboratorios.CrearLaboratorio;
using Aprendizaje.Aplicacion.Evidence.Laboratorios.ListarLaboratorios;
using Aprendizaje.Aplicacion.Evidence.Laboratorios.ObtenerLaboratorioPorId;
using Aprendizaje.Aplicacion.Evidence.Laboratorios.VincularLaboratorioAHerramienta;
using Aprendizaje.Aplicacion.Evidence.Laboratorios.VincularLaboratorioATema;
using Aprendizaje.Dominio.Evidence;
using Aprendizaje.Dominio.Roadmap;
using Aprendizaje.Dominio.Study;
using Aprendizaje.Tests.Soporte;
using Xunit;

namespace Aprendizaje.Tests.Application.Evidence;

public sealed class LaboratoriosApplicationTests
{
    [Fact]
    public async Task CrearLaboratorio_DebeAgregarLaboratorioYGuardar()
    {
        var laboratorios = new FakeLaboratorioRepository();
        var unitOfWork = new FakeUnitOfWork();
        var casoUso = new CrearLaboratorioCasoUso(laboratorios, unitOfWork);
        var usuarioId = Guid.CreateVersion7();

        var resultado = await casoUso.EjecutarAsync(new CrearLaboratorioSolicitud(
            usuarioId,
            " Análisis de tráfico OSI ",
            "Identificar capas",
            "Wireshark",
            "Tráfico capturado",
            45,
            new DateOnly(2026, 8, 24)), CancellationToken);

        Assert.NotEqual(Guid.Empty, resultado.Id);
        Assert.Equal(usuarioId, resultado.UsuarioId);
        Assert.Equal("Análisis de tráfico OSI", resultado.Nombre);
        Assert.Equal("Identificar capas", resultado.Objetivo);
        Assert.Equal("Wireshark", resultado.EntornoVms);
        Assert.Equal("Tráfico capturado", resultado.Hallazgos);
        Assert.Equal(45, resultado.TiempoInvertidoMinutos);
        Assert.Equal(EstadoMadurez.Borrador, resultado.EstadoMadurez);
        Assert.Equal(new DateOnly(2026, 8, 24), resultado.Fecha);
        Assert.NotNull(await laboratorios.ObtenerPorIdAsync(resultado.Id, CancellationToken));
        Assert.Equal(1, laboratorios.AgregarLlamadas);
        Assert.Equal(1, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task CrearLaboratorio_DebeRechazarUsuarioVacioSinGuardar()
    {
        var laboratorios = new FakeLaboratorioRepository();
        var unitOfWork = new FakeUnitOfWork();
        var casoUso = new CrearLaboratorioCasoUso(laboratorios, unitOfWork);

        await Assert.ThrowsAsync<ArgumentException>(() => casoUso.EjecutarAsync(
            new CrearLaboratorioSolicitud(Guid.Empty, "Análisis de tráfico OSI", null, null, null, null, null),
            CancellationToken));

        Assert.Equal(0, laboratorios.AgregarLlamadas);
        Assert.Equal(0, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task ObtenerLaboratorioPorId_DebeRetornarLaboratorioExistente()
    {
        var laboratorios = new FakeLaboratorioRepository();
        var laboratorio = CrearLaboratorio();
        laboratorios.Agregar(laboratorio);
        var casoUso = new ObtenerLaboratorioPorIdCasoUso(laboratorios);

        var resultado = await casoUso.EjecutarAsync(laboratorio.Id, CancellationToken);

        Assert.True(resultado.Encontrado);
        Assert.NotNull(resultado.Laboratorio);
        Assert.Equal(laboratorio.Id, resultado.Laboratorio.Id);
        Assert.Equal(laboratorio.Nombre, resultado.Laboratorio.Nombre);
    }

    [Fact]
    public async Task ObtenerLaboratorioPorId_DebeRetornarNoEncontrado()
    {
        var laboratorios = new FakeLaboratorioRepository();
        var casoUso = new ObtenerLaboratorioPorIdCasoUso(laboratorios);

        var resultado = await casoUso.EjecutarAsync(Guid.CreateVersion7(), CancellationToken);

        Assert.False(resultado.Encontrado);
        Assert.Null(resultado.Laboratorio);
    }

    [Fact]
    public async Task ListarLaboratorios_DebeMapearColeccionDelRepository()
    {
        var laboratorios = new FakeLaboratorioRepository();
        var usuarioId = Guid.CreateVersion7();
        var laboratorio = CrearLaboratorio(usuarioId);
        laboratorios.Agregar(laboratorio);
        laboratorios.Agregar(CrearLaboratorio(Guid.CreateVersion7()));
        var casoUso = new ListarLaboratoriosCasoUso(laboratorios);

        var resultado = await casoUso.EjecutarAsync(new ListarLaboratoriosSolicitud(usuarioId), CancellationToken);

        var resumen = Assert.Single(resultado.Laboratorios);
        Assert.Equal(laboratorio.Id, resumen.Id);
        Assert.Equal(usuarioId, resumen.UsuarioId);
        Assert.Equal(laboratorio.Nombre, resumen.Nombre);
    }

    [Fact]
    public async Task VincularLaboratorioATema_DebeRetornarLaboratorioNoEncontradoSinGuardar()
    {
        var laboratorios = new FakeLaboratorioRepository();
        var temas = new FakeTemaRepository();
        var unitOfWork = new FakeUnitOfWork();
        var casoUso = new VincularLaboratorioATemaCasoUso(laboratorios, temas, unitOfWork);

        var resultado = await casoUso.EjecutarAsync(new VincularLaboratorioATemaSolicitud(
            Guid.CreateVersion7(),
            Guid.CreateVersion7()), CancellationToken);

        Assert.Equal(VincularLaboratorioATemaEstado.LaboratorioNoEncontrado, resultado.Estado);
        Assert.Equal(0, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task VincularLaboratorioATema_DebeRetornarTemaNoEncontradoSinGuardar()
    {
        var laboratorios = new FakeLaboratorioRepository();
        var temas = new FakeTemaRepository();
        var unitOfWork = new FakeUnitOfWork();
        var laboratorio = CrearLaboratorio();
        laboratorios.Agregar(laboratorio);
        var casoUso = new VincularLaboratorioATemaCasoUso(laboratorios, temas, unitOfWork);

        var resultado = await casoUso.EjecutarAsync(
            new VincularLaboratorioATemaSolicitud(laboratorio.Id, Guid.CreateVersion7()),
            CancellationToken);

        Assert.Equal(VincularLaboratorioATemaEstado.TemaNoEncontrado, resultado.Estado);
        Assert.Equal(0, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task VincularLaboratorioATema_DebeRetornarUsuarioNoCoincideSinGuardar()
    {
        var laboratorios = new FakeLaboratorioRepository();
        var temas = new FakeTemaRepository();
        var unitOfWork = new FakeUnitOfWork();
        var laboratorio = CrearLaboratorio();
        var tema = Tema.Crear(Guid.CreateVersion7(), "Modelo OSI", TipoConocimiento.Conceptual);
        laboratorios.Agregar(laboratorio);
        temas.Agregar(tema);
        var casoUso = new VincularLaboratorioATemaCasoUso(laboratorios, temas, unitOfWork);

        var resultado = await casoUso.EjecutarAsync(
            new VincularLaboratorioATemaSolicitud(laboratorio.Id, tema.Id),
            CancellationToken);

        Assert.Equal(VincularLaboratorioATemaEstado.UsuarioNoCoincide, resultado.Estado);
        Assert.Equal(0, laboratorios.VincularTemaLlamadas);
        Assert.Equal(0, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task VincularLaboratorioATema_DebeVincularYGuardar()
    {
        var laboratorios = new FakeLaboratorioRepository();
        var temas = new FakeTemaRepository();
        var unitOfWork = new FakeUnitOfWork();
        var usuarioId = Guid.CreateVersion7();
        var laboratorio = CrearLaboratorio(usuarioId);
        var tema = Tema.Crear(usuarioId, "Modelo OSI", TipoConocimiento.Conceptual);
        laboratorios.Agregar(laboratorio);
        temas.Agregar(tema);
        var casoUso = new VincularLaboratorioATemaCasoUso(laboratorios, temas, unitOfWork);

        var resultado = await casoUso.EjecutarAsync(
            new VincularLaboratorioATemaSolicitud(laboratorio.Id, tema.Id),
            CancellationToken);

        Assert.Equal(VincularLaboratorioATemaEstado.Actualizado, resultado.Estado);
        Assert.True(await laboratorios.ExisteVinculoTemaAsync(laboratorio.Id, tema.Id, CancellationToken));
        Assert.Equal(1, laboratorios.VincularTemaLlamadas);
        Assert.Equal(1, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task VincularLaboratorioATema_DebeSerIdempotenteSinGuardarDeNuevo()
    {
        var laboratorios = new FakeLaboratorioRepository();
        var temas = new FakeTemaRepository();
        var unitOfWork = new FakeUnitOfWork();
        var usuarioId = Guid.CreateVersion7();
        var laboratorio = CrearLaboratorio(usuarioId);
        var tema = Tema.Crear(usuarioId, "Modelo OSI", TipoConocimiento.Conceptual);
        laboratorios.Agregar(laboratorio);
        temas.Agregar(tema);
        var casoUso = new VincularLaboratorioATemaCasoUso(laboratorios, temas, unitOfWork);
        await casoUso.EjecutarAsync(new VincularLaboratorioATemaSolicitud(laboratorio.Id, tema.Id), CancellationToken);

        var resultado = await casoUso.EjecutarAsync(
            new VincularLaboratorioATemaSolicitud(laboratorio.Id, tema.Id),
            CancellationToken);

        Assert.Equal(VincularLaboratorioATemaEstado.Actualizado, resultado.Estado);
        Assert.Equal(1, laboratorios.VincularTemaLlamadas);
        Assert.Equal(1, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task VincularLaboratorioAHerramienta_DebeRetornarLaboratorioNoEncontradoSinGuardar()
    {
        var laboratorios = new FakeLaboratorioRepository();
        var herramientas = new FakeHerramientaRepository();
        var unitOfWork = new FakeUnitOfWork();
        var casoUso = new VincularLaboratorioAHerramientaCasoUso(laboratorios, herramientas, unitOfWork);

        var resultado = await casoUso.EjecutarAsync(new VincularLaboratorioAHerramientaSolicitud(
            Guid.CreateVersion7(),
            Guid.CreateVersion7()), CancellationToken);

        Assert.Equal(VincularLaboratorioAHerramientaEstado.LaboratorioNoEncontrado, resultado.Estado);
        Assert.Equal(0, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task VincularLaboratorioAHerramienta_DebeRetornarHerramientaNoEncontradaSinGuardar()
    {
        var laboratorios = new FakeLaboratorioRepository();
        var herramientas = new FakeHerramientaRepository();
        var unitOfWork = new FakeUnitOfWork();
        var laboratorio = CrearLaboratorio();
        laboratorios.Agregar(laboratorio);
        var casoUso = new VincularLaboratorioAHerramientaCasoUso(laboratorios, herramientas, unitOfWork);

        var resultado = await casoUso.EjecutarAsync(
            new VincularLaboratorioAHerramientaSolicitud(laboratorio.Id, Guid.CreateVersion7()),
            CancellationToken);

        Assert.Equal(VincularLaboratorioAHerramientaEstado.HerramientaNoEncontrada, resultado.Estado);
        Assert.Equal(0, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task VincularLaboratorioAHerramienta_DebeVincularYGuardar()
    {
        var laboratorios = new FakeLaboratorioRepository();
        var herramientas = new FakeHerramientaRepository();
        var unitOfWork = new FakeUnitOfWork();
        var laboratorio = CrearLaboratorio();
        var herramienta = Herramienta.Crear("Wireshark");
        laboratorios.Agregar(laboratorio);
        herramientas.Agregar(herramienta);
        var casoUso = new VincularLaboratorioAHerramientaCasoUso(laboratorios, herramientas, unitOfWork);

        var resultado = await casoUso.EjecutarAsync(
            new VincularLaboratorioAHerramientaSolicitud(laboratorio.Id, herramienta.Id),
            CancellationToken);

        Assert.Equal(VincularLaboratorioAHerramientaEstado.Actualizado, resultado.Estado);
        Assert.True(await laboratorios.ExisteVinculoHerramientaAsync(laboratorio.Id, herramienta.Id, CancellationToken));
        Assert.Equal(1, laboratorios.VincularHerramientaLlamadas);
        Assert.Equal(1, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task VincularLaboratorioAHerramienta_DebeSerIdempotenteSinGuardarDeNuevo()
    {
        var laboratorios = new FakeLaboratorioRepository();
        var herramientas = new FakeHerramientaRepository();
        var unitOfWork = new FakeUnitOfWork();
        var laboratorio = CrearLaboratorio();
        var herramienta = Herramienta.Crear("Wireshark");
        laboratorios.Agregar(laboratorio);
        herramientas.Agregar(herramienta);
        var casoUso = new VincularLaboratorioAHerramientaCasoUso(laboratorios, herramientas, unitOfWork);
        await casoUso.EjecutarAsync(
            new VincularLaboratorioAHerramientaSolicitud(laboratorio.Id, herramienta.Id),
            CancellationToken);

        var resultado = await casoUso.EjecutarAsync(
            new VincularLaboratorioAHerramientaSolicitud(laboratorio.Id, herramienta.Id),
            CancellationToken);

        Assert.Equal(VincularLaboratorioAHerramientaEstado.Actualizado, resultado.Estado);
        Assert.Equal(1, laboratorios.VincularHerramientaLlamadas);
        Assert.Equal(1, unitOfWork.GuardarCambiosLlamadas);
    }

    private static Laboratorio CrearLaboratorio(Guid? usuarioId = null) =>
        Laboratorio.Crear(usuarioId ?? Guid.CreateVersion7(), "Análisis de tráfico OSI");

    private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;
}
