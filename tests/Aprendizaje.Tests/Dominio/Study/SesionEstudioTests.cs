using Aprendizaje.Dominio.Study;
using Aprendizaje.Dominio.Study.Eventos;
using Xunit;

namespace Aprendizaje.Tests.Dominio.Study;

public sealed class SesionEstudioTests
{
    [Fact]
    public void Registrar_DebeCrearSesionConPropiedadesIndicadas()
    {
        var usuarioId = Guid.CreateVersion7();
        var temaId = Guid.CreateVersion7();
        var fecha = new DateOnly(2026, 8, 20);

        var sesion = SesionEstudio.Registrar(
            usuarioId,
            temaId,
            fecha,
            30,
            TipoSesion.Teoria,
            "Estudio inicial");

        Assert.NotEqual(Guid.Empty, sesion.Id);
        Assert.Equal(usuarioId, sesion.UsuarioId);
        Assert.Equal(temaId, sesion.TemaId);
        Assert.Equal(fecha, sesion.Fecha);
        Assert.Equal(30, sesion.DuracionMinutos);
        Assert.Equal(TipoSesion.Teoria, sesion.Tipo);
        Assert.Equal("Estudio inicial", sesion.Notas);
    }

    [Fact]
    public void Registrar_DebePermitirNotasNull()
    {
        var sesion = SesionEstudio.Registrar(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            new DateOnly(2026, 8, 20),
            30,
            TipoSesion.Practica);

        Assert.Null(sesion.Notas);
    }

    [Fact]
    public void Registrar_DebeRechazarUsuarioIdVacio()
    {
        Assert.Throws<ArgumentException>(() =>
            SesionEstudio.Registrar(
                Guid.Empty,
                Guid.CreateVersion7(),
                new DateOnly(2026, 8, 20),
                30,
                TipoSesion.Teoria));
    }

    [Fact]
    public void Registrar_DebeRechazarTemaIdVacio()
    {
        Assert.Throws<ArgumentException>(() =>
            SesionEstudio.Registrar(
                Guid.CreateVersion7(),
                Guid.Empty,
                new DateOnly(2026, 8, 20),
                30,
                TipoSesion.Teoria));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Registrar_DebeRechazarDuracionNoPositiva(int duracionMinutos)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            SesionEstudio.Registrar(
                Guid.CreateVersion7(),
                Guid.CreateVersion7(),
                new DateOnly(2026, 8, 20),
                duracionMinutos,
                TipoSesion.Teoria));
    }

    [Fact]
    public void CorregirDuracion_DebeActualizarDuracion()
    {
        var sesion = CrearSesion();

        sesion.CorregirDuracion(45);

        Assert.Equal(45, sesion.DuracionMinutos);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void CorregirDuracion_DebeRechazarDuracionNoPositiva(int duracionMinutos)
    {
        var sesion = CrearSesion();

        Assert.Throws<ArgumentOutOfRangeException>(() => sesion.CorregirDuracion(duracionMinutos));
    }

    [Fact]
    public void Registrar_DebeGenerarSesionRegistradaEvento()
    {
        var temaId = Guid.CreateVersion7();
        var antes = DateTime.UtcNow;

        var sesion = SesionEstudio.Registrar(
            Guid.CreateVersion7(),
            temaId,
            new DateOnly(2026, 8, 20),
            30,
            TipoSesion.Teoria,
            "Estudio inicial");

        var evento = Assert.IsType<SesionRegistradaEvento>(Assert.Single(sesion.EventosDominio));
        Assert.Equal(sesion.Id, evento.SesionId);
        Assert.Equal(temaId, evento.TemaId);
        Assert.True(evento.OcurrioEnUtc >= antes);
    }

    private static SesionEstudio CrearSesion() =>
        SesionEstudio.Registrar(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            new DateOnly(2026, 8, 20),
            30,
            TipoSesion.Teoria,
            "Estudio inicial");
}
