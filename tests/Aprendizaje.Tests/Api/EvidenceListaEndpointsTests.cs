using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Aprendizaje.Api.Endpoints.Evidence;
using Aprendizaje.Aplicacion.Evidence.Vistas.EvidenceListaV1;
using Aprendizaje.Aplicacion.Nucleo.Usuarios;
using Aprendizaje.Dominio.Evidence;
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
public sealed class EvidenceListaEndpointsTests
{
    [Fact]
    public async Task GetEvidence_EnPersonal_SinUsuarioIdExplicito_UsaUsuarioActual()
    {
        var usuario = Usuario.Registrar("Dylan", EmailUnico());
        await using var app = await AplicacionApiPrueba.CrearAsync(
            "Personal",
            [usuario],
            CrearListaConProyecto());

        var respuesta = await app.Client.GetAsync("/api/evidence", CancellationToken);

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        Assert.Equal(usuario.Id, app.Consulta.SolicitudRecibida?.UsuarioId);
        using var json = await JsonDocument.ParseAsync(
            await respuesta.Content.ReadAsStreamAsync(CancellationToken),
            cancellationToken: CancellationToken);
        Assert.Equal(1, json.RootElement.GetProperty("total").GetInt32());
        Assert.Equal("Proyecto", json.RootElement.GetProperty("items")[0].GetProperty("tipoEvidence").GetString());
    }

    [Fact]
    public async Task GetEvidence_PasaFiltrosALaConsulta()
    {
        var usuario = Usuario.Registrar("Dylan", EmailUnico());
        var temaId = Guid.CreateVersion7();
        await using var app = await AplicacionApiPrueba.CrearAsync("Personal", [usuario], new EvidenceListaV1Dto(0, []));

        var respuesta = await app.Client.GetAsync(
            $"/api/evidence?tipoEvidence=Writeup&estadoMadurez=Documentado&temaId={temaId}",
            CancellationToken);

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        Assert.Equal(TipoEvidenceV1.Writeup, app.Consulta.SolicitudRecibida?.TipoEvidence);
        Assert.Equal(EstadoMadurez.Documentado, app.Consulta.SolicitudRecibida?.EstadoMadurez);
        Assert.Equal(temaId, app.Consulta.SolicitudRecibida?.TemaId);
    }

    [Fact]
    public async Task GetEvidence_FueraDePersonal_SinUsuarioIdExplicito_RetornaBadRequest()
    {
        await using var app = await AplicacionApiPrueba.CrearAsync("Development", [], new EvidenceListaV1Dto(0, []));

        var respuesta = await app.Client.GetAsync("/api/evidence", CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
        Assert.Equal(0, app.Consulta.Llamadas);
    }

    [Fact]
    public async Task GetEvidence_EnPersonal_ContextoInvalido_RetornaConflict()
    {
        await using var app = await AplicacionApiPrueba.CrearAsync(
            "Personal",
            [
                Usuario.Registrar("Usuario A", EmailUnico()),
                Usuario.Registrar("Usuario B", EmailUnico())
            ],
            new EvidenceListaV1Dto(0, []));

        var respuesta = await app.Client.GetAsync("/api/evidence", CancellationToken);

        Assert.Equal(HttpStatusCode.Conflict, respuesta.StatusCode);
        Assert.Equal(0, app.Consulta.Llamadas);
    }

    private sealed class AplicacionApiPrueba : IAsyncDisposable
    {
        private readonly WebApplication _app;

        private AplicacionApiPrueba(WebApplication app, HttpClient client, FakeConsultaEvidenceListaV1 consulta)
        {
            _app = app;
            Client = client;
            Consulta = consulta;
        }

        public HttpClient Client { get; }

        public FakeConsultaEvidenceListaV1 Consulta { get; }

        public static async Task<AplicacionApiPrueba> CrearAsync(
            string environmentName,
            IReadOnlyCollection<Usuario> usuarios,
            EvidenceListaV1Dto evidence)
        {
            var repositorioUsuarios = new FakeUsuarioRepository();
            foreach (var usuario in usuarios)
                repositorioUsuarios.Agregar(usuario);

            var consulta = new FakeConsultaEvidenceListaV1 { Evidence = evidence };
            var builder = WebApplication.CreateBuilder(new WebApplicationOptions
            {
                EnvironmentName = environmentName
            });
            builder.WebHost.UseUrls("http://127.0.0.1:0");
            builder.Services.AddSingleton<IUsuarioRepository>(repositorioUsuarios);
            builder.Services.AddSingleton<IConsultaEvidenceListaV1>(consulta);
            builder.Services.AddScoped<IUsuarioActual, UsuarioActualLocal>();
            builder.Services.AddScoped<ObtenerEvidenceListaV1CasoUso>();
            builder.Services.ConfigureHttpJsonOptions(options =>
                options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

            var app = builder.Build();
            app.MapEvidenceListaEndpoints();
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

    private sealed class FakeConsultaEvidenceListaV1 : IConsultaEvidenceListaV1
    {
        public int Llamadas { get; private set; }
        public ObtenerEvidenceListaV1Solicitud? SolicitudRecibida { get; private set; }
        public EvidenceListaV1Dto Evidence { get; init; } = new(0, []);

        public Task<EvidenceListaV1Dto> ObtenerAsync(
            ObtenerEvidenceListaV1Solicitud solicitud,
            CancellationToken cancellationToken = default)
        {
            Llamadas++;
            SolicitudRecibida = solicitud;

            return Task.FromResult(Evidence);
        }
    }

    private static EvidenceListaV1Dto CrearListaConProyecto()
    {
        var fecha = new DateTime(2026, 9, 1, 12, 0, 0, DateTimeKind.Utc);

        return new EvidenceListaV1Dto(
            1,
            [
                new EvidenceItemV1Dto(
                    Guid.CreateVersion7(),
                    TipoEvidenceV1.Proyecto,
                    "Proyecto API",
                    EstadoMadurez.Borrador,
                    fecha,
                    null,
                    fecha,
                    null,
                    [],
                    [])
            ]);
    }

    private static string EmailUnico() => $"evidence-lista-api-{Guid.CreateVersion7():N}@local.test";

    private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;
}
