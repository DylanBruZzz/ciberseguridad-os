using Aprendizaje.Aplicacion.Evidence.CertificacionesObtenidas.CrearCertificacionObtenida;
using Aprendizaje.Aplicacion.Evidence.CertificacionesObtenidas.ListarCertificacionesObtenidas;
using Aprendizaje.Aplicacion.Evidence.CertificacionesObtenidas.ObtenerCertificacionObtenidaPorId;
using Aprendizaje.Dominio.Evidence;
using Aprendizaje.Dominio.Roadmap;
using Aprendizaje.Tests.Soporte;
using Xunit;

namespace Aprendizaje.Tests.Application.Evidence;

public sealed class CertificacionesObtenidasApplicationTests
{
    [Fact]
    public async Task CrearCertificacionObtenida_DebeRetornarCertificacionNoEncontradaSinGuardar()
    {
        var certificacionesObtenidas = new FakeCertificacionObtenidaRepository();
        var certificaciones = new FakeCertificacionRepository();
        var unitOfWork = new FakeUnitOfWork();
        var casoUso = new CrearCertificacionObtenidaCasoUso(certificacionesObtenidas, certificaciones, unitOfWork);

        var resultado = await casoUso.EjecutarAsync(
            new CrearCertificacionObtenidaSolicitud(
                Guid.CreateVersion7(),
                Guid.CreateVersion7(),
                new DateOnly(2026, 8, 25)),
            CancellationToken);

        Assert.Equal(CrearCertificacionObtenidaEstado.CertificacionNoEncontrada, resultado.Estado);
        Assert.Equal(0, certificacionesObtenidas.AgregarLlamadas);
        Assert.Equal(0, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task CrearCertificacionObtenida_DebeAgregarCertificacionObtenidaYGuardar()
    {
        var certificacionesObtenidas = new FakeCertificacionObtenidaRepository();
        var certificaciones = new FakeCertificacionRepository();
        var unitOfWork = new FakeUnitOfWork();
        var certificacion = Certificacion.Crear("CompTIA Network+", TipoCosto.Pago);
        certificaciones.Agregar(certificacion);
        var casoUso = new CrearCertificacionObtenidaCasoUso(certificacionesObtenidas, certificaciones, unitOfWork);
        var usuarioId = Guid.CreateVersion7();
        var fechaObtencion = new DateOnly(2026, 8, 25);

        var resultado = await casoUso.EjecutarAsync(
            new CrearCertificacionObtenidaSolicitud(usuarioId, certificacion.Id, fechaObtencion),
            CancellationToken);

        Assert.Equal(CrearCertificacionObtenidaEstado.Creada, resultado.Estado);
        Assert.NotNull(resultado.Id);
        Assert.Equal(usuarioId, resultado.UsuarioId);
        Assert.Equal(certificacion.Id, resultado.CertificacionId);
        Assert.Equal(fechaObtencion, resultado.FechaObtencion);
        Assert.Null(resultado.EvidenciaUrl);
        Assert.Equal(EstadoMadurez.Documentado, resultado.EstadoMadurez);
        Assert.NotNull(await certificacionesObtenidas.ObtenerPorIdAsync(resultado.Id.Value, CancellationToken));
        Assert.Equal(1, certificacionesObtenidas.AgregarLlamadas);
        Assert.Equal(1, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task CrearCertificacionObtenida_DebeRechazarUsuarioVacioSinGuardar()
    {
        var certificacionesObtenidas = new FakeCertificacionObtenidaRepository();
        var certificaciones = new FakeCertificacionRepository();
        var unitOfWork = new FakeUnitOfWork();
        var certificacion = Certificacion.Crear("CompTIA Network+", TipoCosto.Pago);
        certificaciones.Agregar(certificacion);
        var casoUso = new CrearCertificacionObtenidaCasoUso(certificacionesObtenidas, certificaciones, unitOfWork);

        await Assert.ThrowsAsync<ArgumentException>(() => casoUso.EjecutarAsync(
            new CrearCertificacionObtenidaSolicitud(
                Guid.Empty,
                certificacion.Id,
                new DateOnly(2026, 8, 25)),
            CancellationToken));

        Assert.Equal(0, certificacionesObtenidas.AgregarLlamadas);
        Assert.Equal(0, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task CrearCertificacionObtenida_DebeRechazarCertificacionVaciaSinGuardar()
    {
        var certificacionesObtenidas = new FakeCertificacionObtenidaRepository();
        var certificaciones = new FakeCertificacionRepository();
        var unitOfWork = new FakeUnitOfWork();
        var casoUso = new CrearCertificacionObtenidaCasoUso(certificacionesObtenidas, certificaciones, unitOfWork);

        await Assert.ThrowsAsync<ArgumentException>(() => casoUso.EjecutarAsync(
            new CrearCertificacionObtenidaSolicitud(
                Guid.CreateVersion7(),
                Guid.Empty,
                new DateOnly(2026, 8, 25)),
            CancellationToken));

        Assert.Equal(0, certificacionesObtenidas.AgregarLlamadas);
        Assert.Equal(0, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task ObtenerCertificacionObtenidaPorId_DebeRetornarCertificacionObtenidaExistente()
    {
        var certificacionesObtenidas = new FakeCertificacionObtenidaRepository();
        var certificacionObtenida = CrearCertificacionObtenida();
        certificacionesObtenidas.Agregar(certificacionObtenida);
        var casoUso = new ObtenerCertificacionObtenidaPorIdCasoUso(certificacionesObtenidas);

        var resultado = await casoUso.EjecutarAsync(certificacionObtenida.Id, CancellationToken);

        Assert.True(resultado.Encontrada);
        Assert.NotNull(resultado.CertificacionObtenida);
        Assert.Equal(certificacionObtenida.Id, resultado.CertificacionObtenida.Id);
        Assert.Equal(certificacionObtenida.CertificacionId, resultado.CertificacionObtenida.CertificacionId);
    }

    [Fact]
    public async Task ObtenerCertificacionObtenidaPorId_DebeRetornarNoEncontrada()
    {
        var certificacionesObtenidas = new FakeCertificacionObtenidaRepository();
        var casoUso = new ObtenerCertificacionObtenidaPorIdCasoUso(certificacionesObtenidas);

        var resultado = await casoUso.EjecutarAsync(Guid.CreateVersion7(), CancellationToken);

        Assert.False(resultado.Encontrada);
        Assert.Null(resultado.CertificacionObtenida);
    }

    [Fact]
    public async Task ListarCertificacionesObtenidas_DebeMapearColeccionDelRepository()
    {
        var certificacionesObtenidas = new FakeCertificacionObtenidaRepository();
        var usuarioId = Guid.CreateVersion7();
        var certificacionObtenida = CrearCertificacionObtenida(usuarioId);
        certificacionesObtenidas.Agregar(certificacionObtenida);
        certificacionesObtenidas.Agregar(CrearCertificacionObtenida(Guid.CreateVersion7()));
        var casoUso = new ListarCertificacionesObtenidasCasoUso(certificacionesObtenidas);

        var resultado = await casoUso.EjecutarAsync(
            new ListarCertificacionesObtenidasSolicitud(usuarioId),
            CancellationToken);

        var resumen = Assert.Single(resultado.CertificacionesObtenidas);
        Assert.Equal(certificacionObtenida.Id, resumen.Id);
        Assert.Equal(usuarioId, resumen.UsuarioId);
        Assert.Equal(certificacionObtenida.CertificacionId, resumen.CertificacionId);
        Assert.Equal(certificacionObtenida.FechaObtencion, resumen.FechaObtencion);
        Assert.Equal(certificacionObtenida.EstadoMadurez, resumen.EstadoMadurez);
    }

    private static CertificacionObtenida CrearCertificacionObtenida(Guid? usuarioId = null) =>
        CertificacionObtenida.Registrar(
            usuarioId ?? Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            new DateOnly(2026, 8, 25));

    private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;
}
