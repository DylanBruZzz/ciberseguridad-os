using Aprendizaje.Aplicacion.Roadmap.Certificaciones.CrearCertificacion;
using Aprendizaje.Aplicacion.Roadmap.Certificaciones.ListarCertificaciones;
using Aprendizaje.Aplicacion.Roadmap.Certificaciones.ObtenerCertificacionPorId;
using Aprendizaje.Aplicacion.Roadmap.Certificaciones.VincularCertificacionATema;
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

    [Fact]
    public async Task VincularCertificacionATema_DebeRetornarCertificacionNoEncontradaSinGuardar()
    {
        var certificaciones = new FakeCertificacionRepository();
        var temas = new FakeTemaRepository();
        var unitOfWork = new FakeUnitOfWork();
        var casoUso = new VincularCertificacionATemaCasoUso(certificaciones, temas, unitOfWork);

        var resultado = await casoUso.EjecutarAsync(
            new VincularCertificacionATemaSolicitud(Guid.CreateVersion7(), Guid.CreateVersion7()),
            CancellationToken);

        Assert.Equal(VincularCertificacionATemaEstado.CertificacionNoEncontrada, resultado.Estado);
        Assert.Equal(0, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task VincularCertificacionATema_DebeRetornarTemaNoEncontradoSinGuardar()
    {
        var certificaciones = new FakeCertificacionRepository();
        var temas = new FakeTemaRepository();
        var unitOfWork = new FakeUnitOfWork();
        var certificacion = Certificacion.Crear("CompTIA Network+", TipoCosto.Pago);
        certificaciones.Agregar(certificacion);
        var casoUso = new VincularCertificacionATemaCasoUso(certificaciones, temas, unitOfWork);

        var resultado = await casoUso.EjecutarAsync(
            new VincularCertificacionATemaSolicitud(certificacion.Id, Guid.CreateVersion7()),
            CancellationToken);

        Assert.Equal(VincularCertificacionATemaEstado.TemaNoEncontrado, resultado.Estado);
        Assert.Equal(0, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task VincularCertificacionATema_DebeVincularYGuardar()
    {
        var certificaciones = new FakeCertificacionRepository();
        var temas = new FakeTemaRepository();
        var unitOfWork = new FakeUnitOfWork();
        var usuarioId = Guid.CreateVersion7();
        var certificacion = Certificacion.Crear("CompTIA Network+", TipoCosto.Pago);
        var tema = Tema.Crear(usuarioId, "Modelo OSI", TipoConocimiento.Conceptual);
        certificaciones.Agregar(certificacion);
        temas.Agregar(tema);
        var casoUso = new VincularCertificacionATemaCasoUso(certificaciones, temas, unitOfWork);

        var resultado = await casoUso.EjecutarAsync(
            new VincularCertificacionATemaSolicitud(certificacion.Id, tema.Id),
            CancellationToken);

        Assert.Equal(VincularCertificacionATemaEstado.Actualizado, resultado.Estado);
        Assert.True(await certificaciones.ExisteVinculoTemaAsync(certificacion.Id, tema.Id, CancellationToken));
        Assert.Equal(1, certificaciones.VincularTemaLlamadas);
        Assert.Equal(1, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task VincularCertificacionATema_NoDebeValidarOwnershipArtificial()
    {
        var certificaciones = new FakeCertificacionRepository();
        var temas = new FakeTemaRepository();
        var unitOfWork = new FakeUnitOfWork();
        var certificacion = Certificacion.Crear("CompTIA Network+", TipoCosto.Pago);
        var tema = Tema.Crear(Guid.CreateVersion7(), "Modelo OSI", TipoConocimiento.Conceptual);
        certificaciones.Agregar(certificacion);
        temas.Agregar(tema);
        var casoUso = new VincularCertificacionATemaCasoUso(certificaciones, temas, unitOfWork);

        var resultado = await casoUso.EjecutarAsync(
            new VincularCertificacionATemaSolicitud(certificacion.Id, tema.Id),
            CancellationToken);

        Assert.Equal(VincularCertificacionATemaEstado.Actualizado, resultado.Estado);
        Assert.Equal(1, certificaciones.VincularTemaLlamadas);
        Assert.Equal(1, unitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task VincularCertificacionATema_DebeSerIdempotenteSinGuardarDeNuevo()
    {
        var certificaciones = new FakeCertificacionRepository();
        var temas = new FakeTemaRepository();
        var unitOfWork = new FakeUnitOfWork();
        var usuarioId = Guid.CreateVersion7();
        var certificacion = Certificacion.Crear("CompTIA Network+", TipoCosto.Pago);
        var tema = Tema.Crear(usuarioId, "Modelo OSI", TipoConocimiento.Conceptual);
        certificaciones.Agregar(certificacion);
        temas.Agregar(tema);
        var casoUso = new VincularCertificacionATemaCasoUso(certificaciones, temas, unitOfWork);
        await casoUso.EjecutarAsync(new VincularCertificacionATemaSolicitud(certificacion.Id, tema.Id), CancellationToken);

        var resultado = await casoUso.EjecutarAsync(
            new VincularCertificacionATemaSolicitud(certificacion.Id, tema.Id),
            CancellationToken);

        Assert.Equal(VincularCertificacionATemaEstado.Actualizado, resultado.Estado);
        Assert.Equal(1, certificaciones.VincularTemaLlamadas);
        Assert.Equal(1, unitOfWork.GuardarCambiosLlamadas);
    }

    private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;
}
