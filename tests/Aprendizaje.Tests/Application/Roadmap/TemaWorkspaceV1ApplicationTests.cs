using Aprendizaje.Aplicacion.Roadmap.Vistas.TemaWorkspaceV1;
using Aprendizaje.Dominio.Roadmap;
using Xunit;

namespace Aprendizaje.Tests.Application.Roadmap;

public sealed class TemaWorkspaceV1ApplicationTests
{
    [Fact]
    public async Task ObtenerTemaWorkspaceV1_DebeRechazarUsuarioVacio()
    {
        var consulta = new FakeConsultaTemaWorkspaceV1();
        var casoUso = new ObtenerTemaWorkspaceV1CasoUso(consulta);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            casoUso.EjecutarAsync(
                new ObtenerTemaWorkspaceV1Solicitud(Guid.Empty, Guid.CreateVersion7()),
                CancellationToken));

        Assert.Equal(0, consulta.ObtenerLlamadas);
    }

    [Fact]
    public async Task ObtenerTemaWorkspaceV1_DebeRechazarTemaVacio()
    {
        var consulta = new FakeConsultaTemaWorkspaceV1();
        var casoUso = new ObtenerTemaWorkspaceV1CasoUso(consulta);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            casoUso.EjecutarAsync(
                new ObtenerTemaWorkspaceV1Solicitud(Guid.CreateVersion7(), Guid.Empty),
                CancellationToken));

        Assert.Equal(0, consulta.ObtenerLlamadas);
    }

    [Fact]
    public async Task ObtenerTemaWorkspaceV1_DebeDelegarUsuarioTemaYAhoraUtcALaConsulta()
    {
        var usuarioId = Guid.CreateVersion7();
        var temaId = Guid.CreateVersion7();
        var ahoraUtc = new DateTime(2026, 9, 1, 12, 0, 0, DateTimeKind.Utc);
        var workspace = CrearWorkspace(temaId);
        var consulta = new FakeConsultaTemaWorkspaceV1 { Workspace = workspace };
        var casoUso = new ObtenerTemaWorkspaceV1CasoUso(consulta);

        var resultado = await casoUso.EjecutarAsync(
            new ObtenerTemaWorkspaceV1Solicitud(usuarioId, temaId, ahoraUtc),
            CancellationToken);

        Assert.True(resultado.Encontrado);
        Assert.Equal(workspace, resultado.Workspace);
        Assert.Equal(usuarioId, consulta.UsuarioIdRecibido);
        Assert.Equal(temaId, consulta.TemaIdRecibido);
        Assert.Equal(ahoraUtc, consulta.AhoraUtcRecibido);
        Assert.Equal(1, consulta.ObtenerLlamadas);
    }

    [Fact]
    public async Task ObtenerTemaWorkspaceV1_CuandoConsultaNoEncuentra_RetornaNoEncontrado()
    {
        var consulta = new FakeConsultaTemaWorkspaceV1();
        var casoUso = new ObtenerTemaWorkspaceV1CasoUso(consulta);

        var resultado = await casoUso.EjecutarAsync(
            new ObtenerTemaWorkspaceV1Solicitud(Guid.CreateVersion7(), Guid.CreateVersion7()),
            CancellationToken);

        Assert.False(resultado.Encontrado);
        Assert.Null(resultado.Workspace);
        Assert.Equal(1, consulta.ObtenerLlamadas);
    }

    private static TemaWorkspaceV1Dto CrearWorkspace(Guid temaId) =>
        new(
            new TemaWorkspaceTemaDto(
                temaId,
                null,
                "Tema",
                null,
                TipoConocimiento.Conceptual,
                EstadoTema.NoIniciado,
                null,
                null,
                30,
                0,
                0,
                0,
                [],
                []),
            null,
            new TemaWorkspaceApuntesDto(string.Empty, null),
            null,
            new TemaWorkspaceRepasoDto(null, false),
            new TemaWorkspaceResourcesResumenDto(0),
            new TemaWorkspaceSesionesResumenDto(0, 0),
            new TemaWorkspaceEvidenceResumenDto(0, 0, 0, 0, 0, 0));

    private sealed class FakeConsultaTemaWorkspaceV1 : IConsultaTemaWorkspaceV1
    {
        public int ObtenerLlamadas { get; private set; }
        public Guid UsuarioIdRecibido { get; private set; }
        public Guid TemaIdRecibido { get; private set; }
        public DateTime AhoraUtcRecibido { get; private set; }
        public TemaWorkspaceV1Dto? Workspace { get; init; }

        public Task<TemaWorkspaceV1Dto?> ObtenerAsync(
            Guid usuarioId,
            Guid temaId,
            DateTime ahoraUtc,
            CancellationToken cancellationToken = default)
        {
            ObtenerLlamadas++;
            UsuarioIdRecibido = usuarioId;
            TemaIdRecibido = temaId;
            AhoraUtcRecibido = ahoraUtc;

            return Task.FromResult(Workspace);
        }
    }

    private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;
}
