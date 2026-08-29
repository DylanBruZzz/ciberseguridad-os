using Aprendizaje.Aplicacion.Nucleo.Usuarios;
using Aprendizaje.Dominio.Nucleo;
using Aprendizaje.Infraestructura.Persistencia;
using Aprendizaje.Infraestructura.Persistencia.Repositorios;
using Xunit;

namespace Aprendizaje.Tests.Integration.Persistencia;

public sealed class UsuarioActualSqlServerTests
{
    [Fact]
    public async Task ObtenerAsync_SinUsuariosVisibles_DevuelveSinUsuarioLocal()
    {
        await using var ambiente = await AmbientePersistenciaSqlServer.CrearAsync(CancellationToken);
        await using var contexto = ambiente.CrearNuevoContexto();

        var resultado = await CrearUsuarioActual(contexto).ObtenerAsync(CancellationToken);

        Assert.Equal(ObtenerUsuarioActualEstado.SinUsuarioLocal, resultado.Estado);
    }

    [Fact]
    public async Task ObtenerAsync_UnUsuarioVisible_DevuelveUsuarioActual()
    {
        await using var ambiente = await AmbientePersistenciaSqlServer.CrearAsync(CancellationToken);
        Usuario usuario;

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            usuario = CrearUsuario("Dylan");
            contexto.Usuarios.Add(usuario);
            await contexto.GuardarCambiosAsync(CancellationToken);
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            var resultado = await CrearUsuarioActual(contexto).ObtenerAsync(CancellationToken);

            Assert.Equal(ObtenerUsuarioActualEstado.Encontrado, resultado.Estado);
            Assert.NotNull(resultado.Usuario);
            Assert.Equal(usuario.Id, resultado.Usuario.Id);
            Assert.Equal("Dylan", resultado.Usuario.Nombre);
        }
    }

    [Fact]
    public async Task ObtenerAsync_DosUsuariosVisibles_DevuelveMultiplesUsuariosLocales()
    {
        await using var ambiente = await AmbientePersistenciaSqlServer.CrearAsync(CancellationToken);

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            contexto.Usuarios.AddRange(CrearUsuario("Dylan"), CrearUsuario("Otro"));
            await contexto.GuardarCambiosAsync(CancellationToken);
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            var resultado = await CrearUsuarioActual(contexto).ObtenerAsync(CancellationToken);

            Assert.Equal(ObtenerUsuarioActualEstado.MultiplesUsuariosLocales, resultado.Estado);
            Assert.Null(resultado.Usuario);
        }
    }

    [Fact]
    public async Task ObtenerAsync_UsuarioEliminadoNoCuentaComoVisible()
    {
        await using var ambiente = await AmbientePersistenciaSqlServer.CrearAsync(CancellationToken);

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            var usuario = CrearUsuario("Dylan");
            usuario.MarcarComoEliminado();
            contexto.Usuarios.Add(usuario);
            await contexto.GuardarCambiosAsync(CancellationToken);
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            var resultado = await CrearUsuarioActual(contexto).ObtenerAsync(CancellationToken);

            Assert.Equal(ObtenerUsuarioActualEstado.SinUsuarioLocal, resultado.Estado);
        }
    }

    [Fact]
    public async Task ObtenerAsync_UnVisibleYUnoEliminado_DevuelveVisible()
    {
        await using var ambiente = await AmbientePersistenciaSqlServer.CrearAsync(CancellationToken);
        Usuario visible;

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            visible = CrearUsuario("Dylan");
            var eliminado = CrearUsuario("Otro");
            eliminado.MarcarComoEliminado();
            contexto.Usuarios.AddRange(visible, eliminado);
            await contexto.GuardarCambiosAsync(CancellationToken);
        }

        await using (var contexto = ambiente.CrearNuevoContexto())
        {
            var resultado = await CrearUsuarioActual(contexto).ObtenerAsync(CancellationToken);

            Assert.Equal(ObtenerUsuarioActualEstado.Encontrado, resultado.Estado);
            Assert.NotNull(resultado.Usuario);
            Assert.Equal(visible.Id, resultado.Usuario.Id);
        }
    }

    private static UsuarioActualLocal CrearUsuarioActual(AprendizajeDbContext contexto) =>
        new(new UsuarioRepository(contexto));

    private static Usuario CrearUsuario(string nombre) =>
        Usuario.Registrar(nombre, $"{Guid.CreateVersion7():N}@aprendizaje.local");

    private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;
}
