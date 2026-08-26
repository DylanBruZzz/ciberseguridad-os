using Aprendizaje.Aplicacion.Analytics.Certificaciones;
using Aprendizaje.Aplicacion.Analytics.Competencias;
using Aprendizaje.Dominio.Roadmap;
using Xunit;

namespace Aprendizaje.Tests.Application.Study;

public sealed class ResumenCompetenciaCertificacionApplicationTests
{
    [Fact]
    public async Task ObtenerResumenCompetencias_DebeRechazarUsuarioVacio()
    {
        var consulta = new FakeConsultaResumenCompetencias();
        var casoUso = new ObtenerResumenCompetenciasCasoUso(consulta);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            casoUso.EjecutarAsync(Guid.Empty, CancellationToken));

        Assert.Equal(0, consulta.ListarLlamadas);
    }

    [Fact]
    public async Task ObtenerResumenCompetencias_DebeRetornarListaVacia()
    {
        var usuarioId = Guid.CreateVersion7();
        var consulta = new FakeConsultaResumenCompetencias();
        var casoUso = new ObtenerResumenCompetenciasCasoUso(consulta);

        var resultado = await casoUso.EjecutarAsync(usuarioId, CancellationToken);

        Assert.Empty(resultado);
        Assert.Equal(usuarioId, consulta.UsuarioIdRecibido);
        Assert.Equal(1, consulta.ListarLlamadas);
    }

    [Fact]
    public async Task ObtenerResumenCompetencias_DebeRetornarResumenDeLaConsulta()
    {
        var usuarioId = Guid.CreateVersion7();
        var resumen = new ResumenCompetenciaDto(
            Guid.CreateVersion7(),
            "Comprensión de fundamentos de redes",
            2);
        var consulta = new FakeConsultaResumenCompetencias { Resumen = [resumen] };
        var casoUso = new ObtenerResumenCompetenciasCasoUso(consulta);

        var resultado = await casoUso.EjecutarAsync(usuarioId, CancellationToken);

        var item = Assert.Single(resultado);
        Assert.Equal(resumen, item);
        Assert.Equal(usuarioId, consulta.UsuarioIdRecibido);
    }

    [Fact]
    public async Task ObtenerResumenCertificaciones_DebeRechazarUsuarioVacio()
    {
        var consulta = new FakeConsultaResumenCertificaciones();
        var casoUso = new ObtenerResumenCertificacionesCasoUso(consulta);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            casoUso.EjecutarAsync(Guid.Empty, CancellationToken));

        Assert.Equal(0, consulta.ListarLlamadas);
    }

    [Fact]
    public async Task ObtenerResumenCertificaciones_DebeRetornarListaVacia()
    {
        var usuarioId = Guid.CreateVersion7();
        var consulta = new FakeConsultaResumenCertificaciones();
        var casoUso = new ObtenerResumenCertificacionesCasoUso(consulta);

        var resultado = await casoUso.EjecutarAsync(usuarioId, CancellationToken);

        Assert.Empty(resultado);
        Assert.Equal(usuarioId, consulta.UsuarioIdRecibido);
        Assert.Equal(1, consulta.ListarLlamadas);
    }

    [Fact]
    public async Task ObtenerResumenCertificaciones_DebeRetornarResumenDeLaConsulta()
    {
        var usuarioId = Guid.CreateVersion7();
        var resumen = new ResumenCertificacionDto(
            Guid.CreateVersion7(),
            "CompTIA Network+",
            "CompTIA",
            TipoCosto.Pago,
            1,
            2);
        var consulta = new FakeConsultaResumenCertificaciones { Resumen = [resumen] };
        var casoUso = new ObtenerResumenCertificacionesCasoUso(consulta);

        var resultado = await casoUso.EjecutarAsync(usuarioId, CancellationToken);

        var item = Assert.Single(resultado);
        Assert.Equal(resumen, item);
        Assert.Equal(usuarioId, consulta.UsuarioIdRecibido);
    }

    private sealed class FakeConsultaResumenCompetencias : IConsultaResumenCompetencias
    {
        public int ListarLlamadas { get; private set; }
        public Guid UsuarioIdRecibido { get; private set; }
        public IReadOnlyCollection<ResumenCompetenciaDto> Resumen { get; init; } = [];

        public Task<IReadOnlyCollection<ResumenCompetenciaDto>> ListarAsync(
            Guid usuarioId,
            CancellationToken cancellationToken = default)
        {
            ListarLlamadas++;
            UsuarioIdRecibido = usuarioId;

            return Task.FromResult(Resumen);
        }
    }

    private sealed class FakeConsultaResumenCertificaciones : IConsultaResumenCertificaciones
    {
        public int ListarLlamadas { get; private set; }
        public Guid UsuarioIdRecibido { get; private set; }
        public IReadOnlyCollection<ResumenCertificacionDto> Resumen { get; init; } = [];

        public Task<IReadOnlyCollection<ResumenCertificacionDto>> ListarAsync(
            Guid usuarioId,
            CancellationToken cancellationToken = default)
        {
            ListarLlamadas++;
            UsuarioIdRecibido = usuarioId;

            return Task.FromResult(Resumen);
        }
    }

    private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;
}
