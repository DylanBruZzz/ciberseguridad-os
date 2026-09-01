using System.Net;
using System.Text.Json;
using Aprendizaje.Api.Endpoints.Roadmap;
using Aprendizaje.Aplicacion.Nucleo.Usuarios;
using Aprendizaje.Aplicacion.Roadmap.Vistas.RoadmapVistaV1;
using Aprendizaje.Dominio.Nucleo;
using Aprendizaje.Dominio.Nucleo.Repositorios;
using Aprendizaje.Tests.Soporte;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Aprendizaje.Tests.Api;

[Collection("ApiEndpoints")]
public sealed class RoadmapVistaEndpointsTests
{
    [Fact]
    public async Task GetRoadmapVista_EnPersonal_SinUsuarioIdExplicito_UsaUsuarioActual()
    {
        var usuario = Usuario.Registrar("Dylan", EmailUnico());
        await using var app = await AplicacionApiPrueba.CrearAsync("Personal", [usuario]);

        var respuesta = await app.Client.GetAsync("/api/roadmap/vista", CancellationToken);

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        Assert.Equal(usuario.Id, app.Consulta.UsuarioIdRecibido);
        using var json = await JsonDocument.ParseAsync(
            await respuesta.Content.ReadAsStreamAsync(CancellationToken),
            cancellationToken: CancellationToken);
        Assert.Equal(0, json.RootElement.GetProperty("progresoGlobalPorcentaje").GetInt32());
        Assert.Empty(json.RootElement.GetProperty("fases").EnumerateArray());
    }

    [Fact]
    public async Task GetRoadmapVista_FueraDePersonal_SinUsuarioIdExplicito_RetornaBadRequest()
    {
        await using var app = await AplicacionApiPrueba.CrearAsync("Development", []);

        var respuesta = await app.Client.GetAsync("/api/roadmap/vista", CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
        Assert.Equal(0, app.Consulta.ObtenerLlamadas);
    }

    [Fact]
    public async Task GetRoadmapVista_EnPersonal_ContextoInvalido_RetornaConflict()
    {
        await using var app = await AplicacionApiPrueba.CrearAsync(
            "Personal",
            [
                Usuario.Registrar("Usuario A", EmailUnico()),
                Usuario.Registrar("Usuario B", EmailUnico())
            ]);

        var respuesta = await app.Client.GetAsync("/api/roadmap/vista", CancellationToken);

        Assert.Equal(HttpStatusCode.Conflict, respuesta.StatusCode);
        Assert.Equal(0, app.Consulta.ObtenerLlamadas);
    }

    private sealed class AplicacionApiPrueba : IAsyncDisposable
    {
        private readonly WebApplication _app;

        private AplicacionApiPrueba(
            WebApplication app,
            HttpClient client,
            FakeConsultaRoadmapVistaV1 consulta)
        {
            _app = app;
            Client = client;
            Consulta = consulta;
        }

        public HttpClient Client { get; }

        public FakeConsultaRoadmapVistaV1 Consulta { get; }

        public static async Task<AplicacionApiPrueba> CrearAsync(
            string environmentName,
            IReadOnlyCollection<Usuario> usuarios)
        {
            var repositorioUsuarios = new FakeUsuarioRepository();
            foreach (var usuario in usuarios)
                repositorioUsuarios.Agregar(usuario);

            var consulta = new FakeConsultaRoadmapVistaV1();

            var builder = WebApplication.CreateBuilder(new WebApplicationOptions
            {
                EnvironmentName = environmentName
            });
            builder.WebHost.UseUrls("http://127.0.0.1:0");
            builder.Services.AddSingleton<IUsuarioRepository>(repositorioUsuarios);
            builder.Services.AddSingleton<IConsultaRoadmapVistaV1>(consulta);
            builder.Services.AddScoped<IUsuarioActual, UsuarioActualLocal>();
            builder.Services.AddScoped<ObtenerRoadmapVistaV1CasoUso>();

            var app = builder.Build();
            app.MapRoadmapVistaEndpoints();
            await app.StartAsync(CancellationToken);

            var direccion = app.Services
                .GetRequiredService<IServer>()
                .Features
                .Get<IServerAddressesFeature>()!
                .Addresses
                .Single();
            var client = new HttpClient { BaseAddress = new Uri(direccion) };

            return new AplicacionApiPrueba(app, client, consulta);
        }

        public async ValueTask DisposeAsync()
        {
            Client.Dispose();
            await _app.StopAsync(CancellationToken.None);
            await _app.DisposeAsync();
        }
    }

    private sealed class FakeConsultaRoadmapVistaV1 : IConsultaRoadmapVistaV1
    {
        public int ObtenerLlamadas { get; private set; }
        public Guid UsuarioIdRecibido { get; private set; }

        public Task<RoadmapVistaV1Dto> ObtenerAsync(
            Guid usuarioId,
            DateTime ahoraUtc,
            CancellationToken cancellationToken = default)
        {
            ObtenerLlamadas++;
            UsuarioIdRecibido = usuarioId;

            return Task.FromResult(new RoadmapVistaV1Dto(null, 0, 0, 0, []));
        }
    }

    private static string EmailUnico() => $"roadmap-vista-api-{Guid.CreateVersion7():N}@local.test";

    private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;
}
