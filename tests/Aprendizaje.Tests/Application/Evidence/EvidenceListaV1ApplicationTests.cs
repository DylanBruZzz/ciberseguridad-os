using Aprendizaje.Aplicacion.Evidence.Vistas.EvidenceListaV1;
using Aprendizaje.Dominio.Evidence;
using Aprendizaje.Dominio.Nucleo;
using Aprendizaje.Tests.Soporte;
using Xunit;

namespace Aprendizaje.Tests.Application.Evidence;

public sealed class EvidenceListaV1ApplicationTests
{
    [Fact]
    public async Task ObtenerEvidenceLista_DebeRechazarUsuarioVacio()
    {
        var consulta = new FakeConsultaEvidenceListaV1();
        var casoUso = new ObtenerEvidenceListaV1CasoUso(new FakeUsuarioRepository(), consulta);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            casoUso.EjecutarAsync(new ObtenerEvidenceListaV1Solicitud(Guid.Empty), CancellationToken));

        Assert.Equal(0, consulta.Llamadas);
    }

    [Fact]
    public async Task ObtenerEvidenceLista_DebeRechazarTemaVacio()
    {
        var usuario = Usuario.Registrar("Dylan", EmailUnico());
        var usuarios = new FakeUsuarioRepository();
        usuarios.Agregar(usuario);
        var consulta = new FakeConsultaEvidenceListaV1();
        var casoUso = new ObtenerEvidenceListaV1CasoUso(usuarios, consulta);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            casoUso.EjecutarAsync(
                new ObtenerEvidenceListaV1Solicitud(usuario.Id, TemaId: Guid.Empty),
                CancellationToken));

        Assert.Equal(0, consulta.Llamadas);
    }

    [Fact]
    public async Task ObtenerEvidenceLista_DebeRetornarNoEncontradoSiUsuarioNoExiste()
    {
        var consulta = new FakeConsultaEvidenceListaV1();
        var casoUso = new ObtenerEvidenceListaV1CasoUso(new FakeUsuarioRepository(), consulta);

        var resultado = await casoUso.EjecutarAsync(
            new ObtenerEvidenceListaV1Solicitud(Guid.CreateVersion7()),
            CancellationToken);

        Assert.False(resultado.Encontrado);
        Assert.Null(resultado.Evidence);
        Assert.Equal(0, consulta.Llamadas);
    }

    [Fact]
    public async Task ObtenerEvidenceLista_DebePasarFiltrosALaConsulta()
    {
        var usuario = Usuario.Registrar("Dylan", EmailUnico());
        var usuarios = new FakeUsuarioRepository();
        usuarios.Agregar(usuario);
        var temaId = Guid.CreateVersion7();
        var consulta = new FakeConsultaEvidenceListaV1();
        var casoUso = new ObtenerEvidenceListaV1CasoUso(usuarios, consulta);

        var resultado = await casoUso.EjecutarAsync(
            new ObtenerEvidenceListaV1Solicitud(
                usuario.Id,
                TipoEvidenceV1.Writeup,
                EstadoMadurez.Documentado,
                temaId),
            CancellationToken);

        Assert.True(resultado.Encontrado);
        Assert.NotNull(resultado.Evidence);
        Assert.Equal(1, consulta.Llamadas);
        Assert.Equal(usuario.Id, consulta.SolicitudRecibida?.UsuarioId);
        Assert.Equal(TipoEvidenceV1.Writeup, consulta.SolicitudRecibida?.TipoEvidence);
        Assert.Equal(EstadoMadurez.Documentado, consulta.SolicitudRecibida?.EstadoMadurez);
        Assert.Equal(temaId, consulta.SolicitudRecibida?.TemaId);
    }

    private static string EmailUnico() => $"evidence-lista-app-{Guid.CreateVersion7():N}@local.test";

    private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

    private sealed class FakeConsultaEvidenceListaV1 : IConsultaEvidenceListaV1
    {
        public int Llamadas { get; private set; }
        public ObtenerEvidenceListaV1Solicitud? SolicitudRecibida { get; private set; }

        public Task<EvidenceListaV1Dto> ObtenerAsync(
            ObtenerEvidenceListaV1Solicitud solicitud,
            CancellationToken cancellationToken = default)
        {
            Llamadas++;
            SolicitudRecibida = solicitud;

            return Task.FromResult(new EvidenceListaV1Dto(0, []));
        }
    }
}
