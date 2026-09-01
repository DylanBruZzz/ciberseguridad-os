using Aprendizaje.Aplicacion.Roadmap.Vistas.RoadmapVistaV1;
using Xunit;

namespace Aprendizaje.Tests.Application.Roadmap;

public sealed class RoadmapVistaV1ApplicationTests
{
    [Fact]
    public async Task ObtenerRoadmapVistaV1_DebeRechazarUsuarioVacio()
    {
        var consulta = new FakeConsultaRoadmapVistaV1();
        var casoUso = new ObtenerRoadmapVistaV1CasoUso(consulta);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            casoUso.EjecutarAsync(new ObtenerRoadmapVistaV1Solicitud(Guid.Empty), CancellationToken));

        Assert.Equal(0, consulta.ObtenerLlamadas);
    }

    [Fact]
    public async Task ObtenerRoadmapVistaV1_DebeDelegarUsuarioYAhoraUtcALaConsulta()
    {
        var usuarioId = Guid.CreateVersion7();
        var ahoraUtc = new DateTime(2026, 9, 1, 12, 0, 0, DateTimeKind.Utc);
        var vista = new RoadmapVistaV1Dto(null, 0, 0, 0, []);
        var consulta = new FakeConsultaRoadmapVistaV1 { Vista = vista };
        var casoUso = new ObtenerRoadmapVistaV1CasoUso(consulta);

        var resultado = await casoUso.EjecutarAsync(
            new ObtenerRoadmapVistaV1Solicitud(usuarioId, ahoraUtc),
            CancellationToken);

        Assert.Equal(vista, resultado);
        Assert.Equal(usuarioId, consulta.UsuarioIdRecibido);
        Assert.Equal(ahoraUtc, consulta.AhoraUtcRecibido);
        Assert.Equal(1, consulta.ObtenerLlamadas);
    }

    private sealed class FakeConsultaRoadmapVistaV1 : IConsultaRoadmapVistaV1
    {
        public int ObtenerLlamadas { get; private set; }
        public Guid UsuarioIdRecibido { get; private set; }
        public DateTime AhoraUtcRecibido { get; private set; }
        public RoadmapVistaV1Dto Vista { get; init; } = new(null, 0, 0, 0, []);

        public Task<RoadmapVistaV1Dto> ObtenerAsync(
            Guid usuarioId,
            DateTime ahoraUtc,
            CancellationToken cancellationToken = default)
        {
            ObtenerLlamadas++;
            UsuarioIdRecibido = usuarioId;
            AhoraUtcRecibido = ahoraUtc;

            return Task.FromResult(Vista);
        }
    }

    private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;
}
