using System.Net;
using System.Text.Json;
using Aprendizaje.Api.Endpoints.Roadmap;
using Aprendizaje.Aplicacion.Nucleo.Usuarios;
using Aprendizaje.Aplicacion.Roadmap.Vistas.TemaWorkspaceV1;
using Aprendizaje.Dominio.Nucleo;
using Aprendizaje.Dominio.Nucleo.Repositorios;
using Aprendizaje.Dominio.Roadmap;
using Aprendizaje.Tests.Soporte;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Aprendizaje.Tests.Api;

[Collection("ApiEndpoints")]
public sealed class TemaWorkspaceEndpointsTests
{
    [Fact]
    public async Task GetTemaWorkspace_EnPersonal_SinUsuarioIdExplicito_UsaUsuarioActual()
    {
        var usuario = Usuario.Registrar("Dylan", EmailUnico());
        var temaId = Guid.CreateVersion7();
        await using var app = await AplicacionApiPrueba.CrearAsync(
            "Personal",
            [usuario],
            CrearWorkspace(temaId));

        var respuesta = await app.Client.GetAsync($"/api/temas/{temaId}/workspace", CancellationToken);

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        Assert.Equal(usuario.Id, app.Consulta.UsuarioIdRecibido);
        Assert.Equal(temaId, app.Consulta.TemaIdRecibido);
        using var json = await JsonDocument.ParseAsync(
            await respuesta.Content.ReadAsStreamAsync(CancellationToken),
            cancellationToken: CancellationToken);
        Assert.Equal(temaId, json.RootElement.GetProperty("tema").GetProperty("id").GetGuid());
        Assert.Equal(string.Empty, json.RootElement.GetProperty("apuntes").GetProperty("contenido").GetString());
        var herramienta = Assert.Single(json.RootElement.GetProperty("herramientas").EnumerateArray());
        Assert.Equal("Wireshark", herramienta.GetProperty("nombre").GetString());
        var certificacion = Assert.Single(json.RootElement.GetProperty("certificaciones").EnumerateArray());
        Assert.Equal("Security+", certificacion.GetProperty("nombre").GetString());
    }

    [Fact]
    public async Task GetTemaWorkspace_ConsultaNoEncuentra_RetornaNotFound()
    {
        var usuario = Usuario.Registrar("Dylan", EmailUnico());
        await using var app = await AplicacionApiPrueba.CrearAsync("Personal", [usuario], null);

        var respuesta = await app.Client.GetAsync($"/api/temas/{Guid.CreateVersion7()}/workspace", CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
        Assert.Equal(1, app.Consulta.ObtenerLlamadas);
    }

    [Fact]
    public async Task GetTemaWorkspace_FueraDePersonal_SinUsuarioIdExplicito_RetornaBadRequest()
    {
        await using var app = await AplicacionApiPrueba.CrearAsync("Development", [], null);

        var respuesta = await app.Client.GetAsync($"/api/temas/{Guid.CreateVersion7()}/workspace", CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
        Assert.Equal(0, app.Consulta.ObtenerLlamadas);
    }

    [Fact]
    public async Task GetTemaWorkspace_EnPersonal_ContextoInvalido_RetornaConflict()
    {
        await using var app = await AplicacionApiPrueba.CrearAsync(
            "Personal",
            [
                Usuario.Registrar("Usuario A", EmailUnico()),
                Usuario.Registrar("Usuario B", EmailUnico())
            ],
            null);

        var respuesta = await app.Client.GetAsync($"/api/temas/{Guid.CreateVersion7()}/workspace", CancellationToken);

        Assert.Equal(HttpStatusCode.Conflict, respuesta.StatusCode);
        Assert.Equal(0, app.Consulta.ObtenerLlamadas);
    }

    private sealed class AplicacionApiPrueba : IAsyncDisposable
    {
        private readonly WebApplication _app;

        private AplicacionApiPrueba(
            WebApplication app,
            HttpClient client,
            FakeConsultaTemaWorkspaceV1 consulta)
        {
            _app = app;
            Client = client;
            Consulta = consulta;
        }

        public HttpClient Client { get; }

        public FakeConsultaTemaWorkspaceV1 Consulta { get; }

        public static async Task<AplicacionApiPrueba> CrearAsync(
            string environmentName,
            IReadOnlyCollection<Usuario> usuarios,
            TemaWorkspaceV1Dto? workspace)
        {
            var repositorioUsuarios = new FakeUsuarioRepository();
            foreach (var usuario in usuarios)
                repositorioUsuarios.Agregar(usuario);

            var consulta = new FakeConsultaTemaWorkspaceV1 { Workspace = workspace };

            var builder = WebApplication.CreateBuilder(new WebApplicationOptions
            {
                EnvironmentName = environmentName
            });
            builder.WebHost.UseUrls("http://127.0.0.1:0");
            builder.Services.AddSingleton<IUsuarioRepository>(repositorioUsuarios);
            builder.Services.AddSingleton<IConsultaTemaWorkspaceV1>(consulta);
            builder.Services.AddScoped<IUsuarioActual, UsuarioActualLocal>();
            builder.Services.AddScoped<ObtenerTemaWorkspaceV1CasoUso>();

            var app = builder.Build();
            app.MapTemaWorkspaceEndpoints();
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

    private sealed class FakeConsultaTemaWorkspaceV1 : IConsultaTemaWorkspaceV1
    {
        public int ObtenerLlamadas { get; private set; }
        public Guid UsuarioIdRecibido { get; private set; }
        public Guid TemaIdRecibido { get; private set; }
        public TemaWorkspaceV1Dto? Workspace { get; init; }

        public Task<TemaWorkspaceV1Dto?> ObtenerAsync(
            Guid usuarioId,
            Guid temaId,
            DateTime ahoraUtc,
            CancellationToken cancellationToken = default)
        {
            ObtenerLlamadas++;
            UsuarioIdRecibido = usuarioId;
            TemaIdRecibido = temaId;

            return Task.FromResult(Workspace);
        }
    }

    private static TemaWorkspaceV1Dto CrearWorkspace(Guid temaId) =>
        new(
            new TemaWorkspaceTemaDto(
                temaId,
                null,
                "Tema API",
                null,
                TipoConocimiento.Conceptual,
                EstadoTema.NoIniciado,
                null,
                null,
                30,
                0,
                0,
                0,
                [],
                []),
            null,
            new TemaWorkspaceApuntesDto(string.Empty, null),
            [new TemaWorkspaceHerramientaDto(Guid.CreateVersion7(), "Wireshark")],
            [new TemaWorkspaceCertificacionDto(Guid.CreateVersion7(), "Security+", "CompTIA", TipoCosto.Pago)],
            null,
            new TemaWorkspaceRepasoDto(null, false),
            new TemaWorkspaceResourcesResumenDto(0),
            new TemaWorkspaceSesionesResumenDto(0, 0),
            new TemaWorkspaceEvidenceResumenDto(0, 0, 0, 0, 0, 0));

    private static string EmailUnico() => $"tema-workspace-api-{Guid.CreateVersion7():N}@local.test";

    private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;
}
