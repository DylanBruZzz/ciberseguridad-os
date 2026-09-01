using Aprendizaje.Aplicacion.Roadmap.Temas.GuardarApuntesTema;
using Aprendizaje.Aplicacion.Roadmap.Temas.ObtenerApuntesTema;
using Aprendizaje.Dominio.Nucleo;
using Aprendizaje.Dominio.Roadmap;
using Aprendizaje.Infraestructura.Persistencia.Repositorios;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Aprendizaje.Tests.Integration.Persistencia;

[Collection(ColeccionPersistenciaSqlServer.Nombre)]
public sealed class ApuntesTemaSqlServerTests
{
    [Fact]
    public async Task ApunteTema_PersisteYActualizaSinDuplicar()
    {
        await using var ambiente = await AmbientePersistenciaSqlServer.CrearAsync(CancellationToken);
        var usuario = Usuario.Registrar("Dylan Tests", EmailUnico());
        var tema = Tema.Crear(usuario.Id, "Modelo OSI", TipoConocimiento.Conceptual);

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            contexto.Usuarios.Add(usuario);
            contexto.Temas.Add(tema);
            await contexto.GuardarCambiosAsync(CancellationToken);
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            var casoUso = new GuardarApuntesTemaCasoUso(
                new ApunteTemaRepository(contexto),
                new TemaRepository(contexto),
                contexto);

            var crear = await casoUso.EjecutarAsync(
                new GuardarApuntesTemaSolicitud(usuario.Id, tema.Id, "  Las capas superiores encapsulan datos  "),
                CancellationToken);
            var actualizar = await casoUso.EjecutarAsync(
                new GuardarApuntesTemaSolicitud(usuario.Id, tema.Id, "La capa de red enruta paquetes"),
                CancellationToken);

            Assert.Equal(GuardarApuntesTemaEstado.Guardado, crear.Estado);
            Assert.Equal(GuardarApuntesTemaEstado.Guardado, actualizar.Estado);
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            var filas = await contexto.Database
                .SqlQueryRaw<int>(
                    "SELECT COUNT(*) AS [Value] FROM [roadmap].[ApunteTema] WHERE [TemaId] = {0}",
                    tema.Id)
                .SingleAsync(CancellationToken);
            var persistido = await contexto.ApuntesTema
                .AsNoTracking()
                .SingleAsync(a => a.TemaId == tema.Id, CancellationToken);

            Assert.Equal(1, filas);
            Assert.Equal(usuario.Id, persistido.UsuarioId);
            Assert.Equal("La capa de red enruta paquetes", persistido.Contenido);
            Assert.NotNull(persistido.FechaModificacionUtc);
        }
    }

    [Fact]
    public async Task ApunteTema_UniqueTemaId_RechazaDuplicados()
    {
        await using var ambiente = await AmbientePersistenciaSqlServer.CrearAsync(CancellationToken);
        var usuario = Usuario.Registrar("Dylan Tests", EmailUnico());
        var tema = Tema.Crear(usuario.Id, "Modelo OSI", TipoConocimiento.Conceptual);
        var primero = ApunteTema.Crear(usuario.Id, tema.Id, "Primero");
        var segundo = ApunteTema.Crear(usuario.Id, tema.Id, "Segundo");

        await using var contexto = ambiente.CrearNuevoContexto();
        contexto.Usuarios.Add(usuario);
        contexto.Temas.Add(tema);
        contexto.ApuntesTema.AddRange(primero, segundo);

        await Assert.ThrowsAsync<DbUpdateException>(() => contexto.GuardarCambiosAsync(CancellationToken));
    }

    [Fact]
    public async Task ObtenerApuntes_TemaEliminadoLogicamente_RetornaTemaNoEncontrado()
    {
        await using var ambiente = await AmbientePersistenciaSqlServer.CrearAsync(CancellationToken);
        var usuario = Usuario.Registrar("Dylan Tests", EmailUnico());
        var tema = Tema.Crear(usuario.Id, "Modelo OSI", TipoConocimiento.Conceptual);
        var apunte = ApunteTema.Crear(usuario.Id, tema.Id, "Apunte visible mientras el Tema existe");

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            contexto.Usuarios.Add(usuario);
            contexto.Temas.Add(tema);
            contexto.ApuntesTema.Add(apunte);
            await contexto.GuardarCambiosAsync(CancellationToken);
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            var persistido = await contexto.Temas.SingleAsync(t => t.Id == tema.Id, CancellationToken);
            persistido.MarcarComoEliminado();
            await contexto.GuardarCambiosAsync(CancellationToken);
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            var casoUso = new ObtenerApuntesTemaCasoUso(
                new ApunteTemaRepository(contexto),
                new TemaRepository(contexto));

            var resultado = await casoUso.EjecutarAsync(
                new ObtenerApuntesTemaSolicitud(usuario.Id, tema.Id),
                CancellationToken);

            Assert.Equal(ObtenerApuntesTemaEstado.TemaNoEncontrado, resultado.Estado);
            Assert.NotNull(await contexto.ApuntesTema.IgnoreQueryFilters().SingleAsync(a => a.Id == apunte.Id, CancellationToken));
        }
    }

    private static string EmailUnico() => $"apuntes-tema-{Guid.CreateVersion7():N}@local.test";

    private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;
}
