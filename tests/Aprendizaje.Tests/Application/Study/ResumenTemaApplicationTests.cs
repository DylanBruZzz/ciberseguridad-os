using Aprendizaje.Aplicacion.Analytics.Temas;
using Xunit;

namespace Aprendizaje.Tests.Application.Study;

public sealed class ResumenTemaApplicationTests
{
    [Fact]
    public async Task ObtenerResumenTema_DebeRechazarUsuarioVacio()
    {
        var consulta = new FakeConsultaResumenTema();
        var casoUso = new ObtenerResumenTemaCasoUso(consulta);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            casoUso.EjecutarAsync(
                new ObtenerResumenTemaSolicitud(Guid.Empty, Guid.CreateVersion7()),
                CancellationToken));

        Assert.Equal(0, consulta.ObtenerLlamadas);
    }

    [Fact]
    public async Task ObtenerResumenTema_DebeRechazarTemaVacio()
    {
        var consulta = new FakeConsultaResumenTema();
        var casoUso = new ObtenerResumenTemaCasoUso(consulta);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            casoUso.EjecutarAsync(
                new ObtenerResumenTemaSolicitud(Guid.CreateVersion7(), Guid.Empty),
                CancellationToken));

        Assert.Equal(0, consulta.ObtenerLlamadas);
    }

    [Fact]
    public async Task ObtenerResumenTema_DebeRetornarNoEncontradoCuandoConsultaNoDevuelveResumen()
    {
        var consulta = new FakeConsultaResumenTema();
        var casoUso = new ObtenerResumenTemaCasoUso(consulta);

        var resultado = await casoUso.EjecutarAsync(
            new ObtenerResumenTemaSolicitud(Guid.CreateVersion7(), Guid.CreateVersion7()),
            CancellationToken);

        Assert.False(resultado.Encontrado);
        Assert.Null(resultado.Resumen);
        Assert.Equal(1, consulta.ObtenerLlamadas);
    }

    [Fact]
    public async Task ObtenerResumenTema_DebeRetornarResumenDeLaConsulta()
    {
        var usuarioId = Guid.CreateVersion7();
        var temaId = Guid.CreateVersion7();
        var resumen = new ResumenTemaDto(
            temaId,
            "Modelo OSI",
            4,
            5,
            new DateOnly(2026, 8, 25),
            new DateOnly(2026, 10, 15),
            21,
            1,
            45,
            new DateOnly(2026, 8, 20),
            2,
            2,
            1,
            1,
            1,
            1,
            1,
            1);
        var consulta = new FakeConsultaResumenTema { Resumen = resumen };
        var casoUso = new ObtenerResumenTemaCasoUso(consulta);

        var resultado = await casoUso.EjecutarAsync(new ObtenerResumenTemaSolicitud(usuarioId, temaId), CancellationToken);

        Assert.True(resultado.Encontrado);
        Assert.Equal(resumen, resultado.Resumen);
        Assert.Equal(usuarioId, consulta.UsuarioIdRecibido);
        Assert.Equal(temaId, consulta.TemaIdRecibido);
    }

    private sealed class FakeConsultaResumenTema : IConsultaResumenTema
    {
        public int ObtenerLlamadas { get; private set; }
        public Guid UsuarioIdRecibido { get; private set; }
        public Guid TemaIdRecibido { get; private set; }
        public ResumenTemaDto? Resumen { get; init; }

        public Task<ResumenTemaDto?> ObtenerAsync(
            Guid usuarioId,
            Guid temaId,
            CancellationToken cancellationToken = default)
        {
            ObtenerLlamadas++;
            UsuarioIdRecibido = usuarioId;
            TemaIdRecibido = temaId;

            return Task.FromResult(Resumen);
        }
    }

    private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;
}
