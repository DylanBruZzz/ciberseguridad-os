using System.Net;
using Aprendizaje.Api.Configuracion;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Aprendizaje.Tests.Api;

[Collection("ApiEndpoints")]
public sealed class FrontendHostingTests
{
    [Fact]
    public async Task RutaSpa_DevuelveIndexHtml()
    {
        await using var app = await AplicacionFrontendPrueba.CrearAsync();

        var respuesta = await app.Client.GetAsync("/roadmap/tema/11111111-1111-1111-1111-111111111111", CancellationToken);

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        Assert.Equal("text/html", respuesta.Content.Headers.ContentType?.MediaType);
        Assert.Contains("Ciberseguridad OS Test", await respuesta.Content.ReadAsStringAsync(CancellationToken));
    }

    [Fact]
    public async Task RutaApiReal_ContinuaFuncionando()
    {
        await using var app = await AplicacionFrontendPrueba.CrearAsync();

        var respuesta = await app.Client.GetAsync("/api/estado-prueba", CancellationToken);

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        Assert.Contains("ok", await respuesta.Content.ReadAsStringAsync(CancellationToken));
    }

    [Fact]
    public async Task RutaApiInexistente_NoDevuelveIndexHtml()
    {
        await using var app = await AplicacionFrontendPrueba.CrearAsync();

        var respuesta = await app.Client.GetAsync("/api/ruta-inexistente", CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
        Assert.DoesNotContain("Ciberseguridad OS Test", await respuesta.Content.ReadAsStringAsync(CancellationToken));
    }

    [Fact]
    public async Task AssetEstatico_SeSirveDesdeWwwroot()
    {
        await using var app = await AplicacionFrontendPrueba.CrearAsync();

        var respuesta = await app.Client.GetAsync("/assets/app-test.txt", CancellationToken);

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        Assert.Equal("asset-ok", await respuesta.Content.ReadAsStringAsync(CancellationToken));
    }

    [Theory]
    [InlineData(true, null, null, "Personal")]
    [InlineData(true, "Development", null, null)]
    [InlineData(true, null, "Development", null)]
    [InlineData(false, null, null, null)]
    public void ResolverEntornoInicial_SoloUsaPersonalEnEmpaquetadoSinEntornoExplicito(
        bool esEmpaquetado,
        string? aspnetCoreEnvironment,
        string? dotnetEnvironment,
        string? esperado)
    {
        var resultado = RuntimeLocalWindows.ResolverEntornoInicial(
            esEmpaquetado,
            aspnetCoreEnvironment,
            dotnetEnvironment);

        Assert.Equal(esperado, resultado);
    }

    private sealed class AplicacionFrontendPrueba : IAsyncDisposable
    {
        private readonly WebApplication _app;
        private readonly string _webRoot;

        private AplicacionFrontendPrueba(WebApplication app, HttpClient client, string webRoot)
        {
            _app = app;
            Client = client;
            _webRoot = webRoot;
        }

        public HttpClient Client { get; }

        public static async Task<AplicacionFrontendPrueba> CrearAsync()
        {
            var webRoot = Path.Combine(Path.GetTempPath(), $"aprendizaje-frontend-{Guid.CreateVersion7():N}");
            Directory.CreateDirectory(Path.Combine(webRoot, "assets"));
            await File.WriteAllTextAsync(
                Path.Combine(webRoot, "index.html"),
                "<!doctype html><html><body>Ciberseguridad OS Test</body></html>",
                CancellationToken);
            await File.WriteAllTextAsync(
                Path.Combine(webRoot, "assets", "app-test.txt"),
                "asset-ok",
                CancellationToken);

            var builder = WebApplication.CreateBuilder(new WebApplicationOptions
            {
                EnvironmentName = "Personal",
                WebRootPath = webRoot
            });
            builder.WebHost.UseUrls("http://127.0.0.1:0");

            var app = builder.Build();
            app.UseFrontendEstatico();
            app.MapGet("/api/estado-prueba", () => Results.Ok(new { estado = "ok" }));
            app.MapFrontendFallback();
            await app.StartAsync(CancellationToken);

            var direccion = app.Services
                .GetRequiredService<IServer>()
                .Features
                .Get<IServerAddressesFeature>()!
                .Addresses
                .Single();
            var client = new HttpClient { BaseAddress = new Uri(direccion) };

            return new AplicacionFrontendPrueba(app, client, webRoot);
        }

        public async ValueTask DisposeAsync()
        {
            Client.Dispose();
            await _app.StopAsync(CancellationToken.None);
            await _app.DisposeAsync();

            if (Directory.Exists(_webRoot))
                Directory.Delete(_webRoot, recursive: true);
        }
    }

    private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;
}
