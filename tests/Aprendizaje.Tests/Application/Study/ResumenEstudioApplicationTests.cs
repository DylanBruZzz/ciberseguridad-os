using Aprendizaje.Aplicacion.Analytics.Estudio;
using Aprendizaje.Dominio.Study;
using Xunit;

namespace Aprendizaje.Tests.Application.Study;

public sealed class ResumenEstudioApplicationTests
{
    [Fact]
    public async Task ObtenerResumenEstudio_DebeRechazarUsuarioVacio()
    {
        var consulta = new FakeConsultaResumenEstudio();
        var casoUso = new ObtenerResumenEstudioCasoUso(consulta);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            casoUso.EjecutarAsync(Guid.Empty, CancellationToken));

        Assert.Equal(0, consulta.ObtenerLlamadas);
    }

    [Fact]
    public async Task ObtenerResumenEstudio_DebeRetornarResumenDeLaConsulta()
    {
        var usuarioId = Guid.CreateVersion7();
        var esperado = new ResumenEstudioDto(
            usuarioId,
            1,
            45,
            1,
            new DateOnly(2026, 8, 20),
            [new SesionPorTipoDto(TipoSesion.Teoria, 1, 45)]);
        var consulta = new FakeConsultaResumenEstudio { Resumen = esperado };
        var casoUso = new ObtenerResumenEstudioCasoUso(consulta);

        var resultado = await casoUso.EjecutarAsync(usuarioId, CancellationToken);

        Assert.Equal(esperado, resultado);
        Assert.Equal(1, consulta.ObtenerLlamadas);
        Assert.Equal(usuarioId, consulta.UsuarioIdRecibido);
    }

    private sealed class FakeConsultaResumenEstudio : IConsultaResumenEstudio
    {
        public int ObtenerLlamadas { get; private set; }
        public Guid UsuarioIdRecibido { get; private set; }
        public ResumenEstudioDto? Resumen { get; init; }

        public Task<ResumenEstudioDto> ObtenerAsync(
            Guid usuarioId,
            CancellationToken cancellationToken = default)
        {
            ObtenerLlamadas++;
            UsuarioIdRecibido = usuarioId;

            return Task.FromResult(Resumen ?? new ResumenEstudioDto(usuarioId, 0, 0, 0, null, []));
        }
    }

    private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;
}
