using Aprendizaje.Aplicacion.Roadmap.Certificaciones.CrearCertificacion;
using Aprendizaje.Aplicacion.Roadmap.Certificaciones.ListarCertificaciones;
using Aprendizaje.Aplicacion.Roadmap.Certificaciones.ObtenerCertificacionPorId;
using Aprendizaje.Dominio.Roadmap;
using Aprendizaje.Tests.Soporte;
using Xunit;

namespace Aprendizaje.Tests.Application.Roadmap;

public sealed class CertificacionesApplicationTests
{
    [Fact]
    public async Task CrearCertificacion_DebeAgregarCertificacionYGuardar()
    {
        var certificaciones = new FakeCertificacionRepository();
        var unitOfWork = new FakeUnitOfWork();
        var casoUso = new CrearCertificacionCasoUso(certificaciones, unitOfWork);

        var resultado = await casoUso.EjecutarAsync(
            new CrearCertificacionSolicitud(" CompTIA Network+ ", TipoCosto.Pago),
            CancellationToken);

        Assert.NotEqual(Guid.Empty, resultado.Id);
        Assert.Equal("CompTIA Network+", resultado.Nombre);
        Assert.Equal(TipoCosto.Pago, resultado.TipoCosto);
        Assert.Null(resultado.Proveedor);
        Assert.Null(resultado.Url);
        Assert.NotNull(await certificaciones.ObtenerPorIdAsync(resultado.Id, CancellationToken));
        Assert.Equal(1, certificaciones.AgregarLlamadas);
        Assert.Equal(1, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task CrearCertificacion_DebeRechazarNombreVacioSinGuardar()
    {
        var certificaciones = new FakeCertificacionRepository();
        var unitOfWork = new FakeUnitOfWork();
        var casoUso = new CrearCertificacionCasoUso(certificaciones, unitOfWork);

        await Assert.ThrowsAsync<ArgumentException>(() => casoUso.EjecutarAsync(
            new CrearCertificacionSolicitud("   ", TipoCosto.Pago),
            CancellationToken));

        Assert.Equal(0, certificaciones.AgregarLlamadas);
        Assert.Equal(0, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task ObtenerCertificacionPorId_DebeRetornarCertificacionExistente()
    {
        var certificaciones = new FakeCertificacionRepository();
        var certificacion = Certificacion.Crear("CompTIA Network+", TipoCosto.Pago);
        certificaciones.Agregar(certificacion);
        var casoUso = new ObtenerCertificacionPorIdCasoUso(certificaciones);

        var resultado = await casoUso.EjecutarAsync(certificacion.Id, CancellationToken);

        Assert.True(resultado.Encontrado);
        Assert.NotNull(resultado.Certificacion);
        Assert.Equal(certificacion.Id, resultado.Certificacion.Id);
        Assert.Equal(certificacion.Nombre, resultado.Certificacion.Nombre);
    }

    [Fact]
    public async Task ObtenerCertificacionPorId_DebeRetornarNoEncontrada()
    {
        var certificaciones = new FakeCertificacionRepository();
        var casoUso = new ObtenerCertificacionPorIdCasoUso(certificaciones);

        var resultado = await casoUso.EjecutarAsync(Guid.CreateVersion7(), CancellationToken);

        Assert.False(resultado.Encontrado);
        Assert.Null(resultado.Certificacion);
    }

    [Fact]
    public async Task ListarCertificaciones_DebeMapearColeccionDelRepository()
    {
        var certificaciones = new FakeCertificacionRepository();
        var certificacion = Certificacion.Crear("CompTIA Network+", TipoCosto.Pago);
        certificaciones.Agregar(certificacion);
        var casoUso = new ListarCertificacionesCasoUso(certificaciones);

        var resultado = await casoUso.EjecutarAsync(CancellationToken);

        var resumen = Assert.Single(resultado.Certificaciones);
        Assert.Equal(certificacion.Id, resumen.Id);
        Assert.Equal(certificacion.Nombre, resumen.Nombre);
        Assert.Equal(certificacion.TipoCosto, resumen.TipoCosto);
    }

    private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;
}
