using Aprendizaje.Aplicacion.Portafolio;
using Aprendizaje.Dominio.Evidence;
using Aprendizaje.Dominio.Nucleo;
using Aprendizaje.Tests.Soporte;
using Xunit;

namespace Aprendizaje.Tests.Application.Portafolio;

public sealed class PortafolioApplicationTests
{
    [Fact]
    public async Task ObtenerPortafolio_DebeRechazarUsuarioVacio()
    {
        var usuarios = new FakeUsuarioRepository();
        var consulta = new FakeConsultaPortafolio();
        var casoUso = new ObtenerPortafolioCasoUso(usuarios, consulta);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            casoUso.EjecutarAsync(new ObtenerPortafolioSolicitud(Guid.Empty), CancellationToken));

        Assert.Equal(0, consulta.Llamadas);
    }

    [Fact]
    public async Task ObtenerPortafolio_DebeRechazarFiltroMadurezNoElegible()
    {
        var usuario = Usuario.Registrar("Dylan", EmailUnico());
        var usuarios = new FakeUsuarioRepository();
        usuarios.Agregar(usuario);
        var consulta = new FakeConsultaPortafolio();
        var casoUso = new ObtenerPortafolioCasoUso(usuarios, consulta);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            casoUso.EjecutarAsync(
                new ObtenerPortafolioSolicitud(usuario.Id, EstadoMadurez: EstadoMadurez.Documentado),
                CancellationToken));

        Assert.Equal(0, consulta.Llamadas);
    }

    [Fact]
    public async Task ObtenerPortafolio_DebeRetornarNoEncontradoSiUsuarioNoExiste()
    {
        var consulta = new FakeConsultaPortafolio();
        var casoUso = new ObtenerPortafolioCasoUso(new FakeUsuarioRepository(), consulta);

        var resultado = await casoUso.EjecutarAsync(
            new ObtenerPortafolioSolicitud(Guid.CreateVersion7()),
            CancellationToken);

        Assert.False(resultado.Encontrado);
        Assert.Null(resultado.Portafolio);
        Assert.Equal(0, consulta.Llamadas);
    }

    [Fact]
    public async Task ObtenerPortafolio_DebeRetornarPortafolioVacioParaUsuarioSinEvidence()
    {
        var usuario = Usuario.Registrar("Dylan", EmailUnico());
        var usuarios = new FakeUsuarioRepository();
        usuarios.Agregar(usuario);
        var portafolio = CrearPortafolioVacio(usuario.Id);
        var consulta = new FakeConsultaPortafolio { Portafolio = portafolio };
        var casoUso = new ObtenerPortafolioCasoUso(usuarios, consulta);

        var resultado = await casoUso.EjecutarAsync(new ObtenerPortafolioSolicitud(usuario.Id), CancellationToken);

        Assert.True(resultado.Encontrado);
        Assert.Same(portafolio, resultado.Portafolio);
        Assert.Equal(usuario.Id, consulta.SolicitudRecibida?.UsuarioId);
        Assert.Null(consulta.SolicitudRecibida?.TipoEvidence);
        Assert.Null(consulta.SolicitudRecibida?.EstadoMadurez);
    }

    [Fact]
    public async Task ObtenerPortafolio_DebePasarFiltrosValidosALaConsulta()
    {
        var usuario = Usuario.Registrar("Dylan", EmailUnico());
        var usuarios = new FakeUsuarioRepository();
        usuarios.Agregar(usuario);
        var consulta = new FakeConsultaPortafolio { Portafolio = CrearPortafolioVacio(usuario.Id) };
        var casoUso = new ObtenerPortafolioCasoUso(usuarios, consulta);

        var resultado = await casoUso.EjecutarAsync(
            new ObtenerPortafolioSolicitud(
                usuario.Id,
                TipoEvidencePortafolio.Writeup,
                EstadoMadurez.Publicado),
            CancellationToken);

        Assert.True(resultado.Encontrado);
        Assert.Equal(TipoEvidencePortafolio.Writeup, consulta.SolicitudRecibida?.TipoEvidence);
        Assert.Equal(EstadoMadurez.Publicado, consulta.SolicitudRecibida?.EstadoMadurez);
        Assert.Equal(1, consulta.Llamadas);
    }

    private static PortafolioDto CrearPortafolioVacio(Guid usuarioId) =>
        new(
            usuarioId,
            new PortafolioResumenDto(0, 0, 0, 0, 0, 0, 0, 0),
            [],
            [],
            [],
            [],
            []);

    private static string EmailUnico() => $"portafolio-app-{Guid.CreateVersion7():N}@local.test";

    private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

    private sealed class FakeConsultaPortafolio : IConsultaPortafolio
    {
        public int Llamadas { get; private set; }
        public ObtenerPortafolioSolicitud? SolicitudRecibida { get; private set; }
        public PortafolioDto? Portafolio { get; init; }

        public Task<PortafolioDto> ObtenerAsync(
            ObtenerPortafolioSolicitud solicitud,
            CancellationToken cancellationToken = default)
        {
            Llamadas++;
            SolicitudRecibida = solicitud;

            return Task.FromResult(Portafolio ?? CrearPortafolioVacio(solicitud.UsuarioId));
        }
    }
}
