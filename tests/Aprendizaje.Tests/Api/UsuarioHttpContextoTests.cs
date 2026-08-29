using Aprendizaje.Api.Endpoints.Nucleo;
using Aprendizaje.Aplicacion.Nucleo.Usuarios;
using Aprendizaje.Dominio.Nucleo;
using Aprendizaje.Tests.Soporte;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Aprendizaje.Tests.Api;

public sealed class UsuarioHttpContextoTests
{
    [Fact]
    public async Task ResolverAsync_en_development_usa_usuario_explicito()
    {
        var usuarioId = Guid.CreateVersion7();
        var usuarioActual = CrearUsuarioActual();

        var resultado = await UsuarioHttpContexto.ResolverAsync(
            usuarioId,
            new Ambiente("Development"),
            usuarioActual,
            CancellationToken.None);

        Assert.True(resultado.Exitosa);
        Assert.Equal(usuarioId, resultado.UsuarioId);
    }

    [Fact]
    public async Task ResolverAsync_en_development_sin_usuario_explicito_falla()
    {
        var usuarioActual = CrearUsuarioActual();

        var resultado = await UsuarioHttpContexto.ResolverAsync(
            null,
            new Ambiente("Development"),
            usuarioActual,
            CancellationToken.None);

        Assert.False(resultado.Exitosa);
        Assert.Equal(StatusCodes.Status400BadRequest, ObtenerStatusCode(resultado.Error!));
    }

    [Fact]
    public async Task ResolverAsync_en_personal_sin_usuario_explicito_usa_unico_visible()
    {
        var usuario = Usuario.Registrar("Dylan", "dylan@aprendizaje.local");
        var usuarioActual = CrearUsuarioActual(usuario);

        var resultado = await UsuarioHttpContexto.ResolverAsync(
            null,
            new Ambiente("Personal"),
            usuarioActual,
            CancellationToken.None);

        Assert.True(resultado.Exitosa);
        Assert.Equal(usuario.Id, resultado.UsuarioId);
    }

    [Fact]
    public async Task ResolverAsync_en_personal_rechaza_usuario_explicito_distinto()
    {
        var usuario = Usuario.Registrar("Dylan", "dylan@aprendizaje.local");
        var usuarioActual = CrearUsuarioActual(usuario);

        var resultado = await UsuarioHttpContexto.ResolverAsync(
            Guid.CreateVersion7(),
            new Ambiente("Personal"),
            usuarioActual,
            CancellationToken.None);

        Assert.False(resultado.Exitosa);
        Assert.Equal(StatusCodes.Status409Conflict, ObtenerStatusCode(resultado.Error!));
    }

    [Fact]
    public async Task ResolverAsync_en_personal_rechaza_multiples_usuarios_visibles()
    {
        var usuarioActual = CrearUsuarioActual(
            Usuario.Registrar("Dylan", "dylan@aprendizaje.local"),
            Usuario.Registrar("Otro", "otro@aprendizaje.local"));

        var resultado = await UsuarioHttpContexto.ResolverAsync(
            null,
            new Ambiente("Personal"),
            usuarioActual,
            CancellationToken.None);

        Assert.False(resultado.Exitosa);
        Assert.Equal(StatusCodes.Status409Conflict, ObtenerStatusCode(resultado.Error!));
    }

    [Fact]
    public async Task ValidarPertenenciaPersonalAsync_devuelve_notfound_si_entidad_no_es_del_usuario_actual()
    {
        var usuario = Usuario.Registrar("Dylan", "dylan@aprendizaje.local");
        var usuarioActual = CrearUsuarioActual(usuario);

        var resultado = await UsuarioHttpContexto.ValidarPertenenciaPersonalAsync(
            Guid.CreateVersion7(),
            new Ambiente("Personal"),
            usuarioActual,
            CancellationToken.None);

        Assert.NotNull(resultado);
        Assert.Equal(StatusCodes.Status404NotFound, ObtenerStatusCode(resultado));
    }

    [Fact]
    public async Task ValidarPertenenciaPersonalAsync_no_aplica_fuera_de_personal()
    {
        var usuarioActual = CrearUsuarioActual();

        var resultado = await UsuarioHttpContexto.ValidarPertenenciaPersonalAsync(
            Guid.CreateVersion7(),
            new Ambiente("Development"),
            usuarioActual,
            CancellationToken.None);

        Assert.Null(resultado);
    }

    private static IUsuarioActual CrearUsuarioActual(params Usuario[] usuarios)
    {
        var repositorio = new FakeUsuarioRepository();

        foreach (var usuario in usuarios)
            repositorio.Agregar(usuario);

        return new UsuarioActualLocal(repositorio);
    }

    private static int ObtenerStatusCode(IResult resultado)
    {
        var statusCode = Assert.IsAssignableFrom<IStatusCodeHttpResult>(resultado);

        return statusCode.StatusCode!.Value;
    }

    private sealed class Ambiente : IHostEnvironment
    {
        public Ambiente(string environmentName)
        {
            EnvironmentName = environmentName;
        }

        public string EnvironmentName { get; set; }

        public string ApplicationName { get; set; } = "Aprendizaje.Tests";

        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;

        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
