using Aprendizaje.Aplicacion.Nucleo.Usuarios;
using Aprendizaje.Dominio.Nucleo;
using Aprendizaje.Tests.Soporte;
using Xunit;

namespace Aprendizaje.Tests.Application;

public sealed class UsuarioActualApplicationTests
{
    [Fact]
    public async Task ObtenerAsync_SinUsuariosVisibles_DevuelveSinUsuarioLocal()
    {
        var usuarioActual = new UsuarioActualLocal(new FakeUsuarioRepository());

        var resultado = await usuarioActual.ObtenerAsync(CancellationToken);

        Assert.Equal(ObtenerUsuarioActualEstado.SinUsuarioLocal, resultado.Estado);
        Assert.Null(resultado.Usuario);
        Assert.False(resultado.Encontrado);
    }

    [Fact]
    public async Task ObtenerAsync_UnUsuarioVisible_DevuelveUsuarioActual()
    {
        var usuarios = new FakeUsuarioRepository();
        var usuario = Usuario.Registrar("Dylan", "dylan@aprendizaje.local");
        usuarios.Agregar(usuario);
        var usuarioActual = new UsuarioActualLocal(usuarios);

        var resultado = await usuarioActual.ObtenerAsync(CancellationToken);

        Assert.Equal(ObtenerUsuarioActualEstado.Encontrado, resultado.Estado);
        Assert.True(resultado.Encontrado);
        Assert.NotNull(resultado.Usuario);
        Assert.Equal(usuario.Id, resultado.Usuario.Id);
        Assert.Equal("Dylan", resultado.Usuario.Nombre);
    }

    [Fact]
    public async Task ObtenerAsync_DosUsuariosVisibles_DevuelveMultiplesUsuariosLocales()
    {
        var usuarios = new FakeUsuarioRepository();
        usuarios.Agregar(Usuario.Registrar("Dylan", "dylan@aprendizaje.local"));
        usuarios.Agregar(Usuario.Registrar("Otro", "otro@aprendizaje.local"));
        var usuarioActual = new UsuarioActualLocal(usuarios);

        var resultado = await usuarioActual.ObtenerAsync(CancellationToken);

        Assert.Equal(ObtenerUsuarioActualEstado.MultiplesUsuariosLocales, resultado.Estado);
        Assert.Null(resultado.Usuario);
    }

    [Fact]
    public async Task ObtenerAsync_UsuarioEliminadoNoCuentaComoVisible()
    {
        var usuarios = new FakeUsuarioRepository();
        var eliminado = Usuario.Registrar("Dylan", "dylan@aprendizaje.local");
        eliminado.MarcarComoEliminado();
        usuarios.Agregar(eliminado);
        var usuarioActual = new UsuarioActualLocal(usuarios);

        var resultado = await usuarioActual.ObtenerAsync(CancellationToken);

        Assert.Equal(ObtenerUsuarioActualEstado.SinUsuarioLocal, resultado.Estado);
    }

    [Fact]
    public async Task ObtenerAsync_UnVisibleYUnoEliminado_DevuelveVisible()
    {
        var usuarios = new FakeUsuarioRepository();
        var visible = Usuario.Registrar("Dylan", "dylan@aprendizaje.local");
        var eliminado = Usuario.Registrar("Otro", "otro@aprendizaje.local");
        eliminado.MarcarComoEliminado();
        usuarios.Agregar(visible);
        usuarios.Agregar(eliminado);
        var usuarioActual = new UsuarioActualLocal(usuarios);

        var resultado = await usuarioActual.ObtenerAsync(CancellationToken);

        Assert.Equal(ObtenerUsuarioActualEstado.Encontrado, resultado.Estado);
        Assert.NotNull(resultado.Usuario);
        Assert.Equal(visible.Id, resultado.Usuario.Id);
    }

    private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;
}
