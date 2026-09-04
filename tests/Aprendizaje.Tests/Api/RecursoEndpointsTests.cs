using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Aprendizaje.Api.Endpoints.Resource;
using Aprendizaje.Aplicacion.Comun;
using Aprendizaje.Aplicacion.Nucleo.Usuarios;
using Aprendizaje.Aplicacion.Resource.Recursos;
using Aprendizaje.Aplicacion.Resource.Recursos.ActualizarRecurso;
using Aprendizaje.Aplicacion.Resource.Recursos.CrearRecurso;
using Aprendizaje.Aplicacion.Resource.Recursos.EliminarRecurso;
using Aprendizaje.Aplicacion.Resource.Recursos.ListarRecursos;
using Aprendizaje.Aplicacion.Resource.Recursos.ObtenerRecursoPorId;
using Aprendizaje.Aplicacion.Resource.Recursos.VincularRecursoATema;
using Aprendizaje.Dominio.Nucleo;
using Aprendizaje.Dominio.Nucleo.Repositorios;
using Aprendizaje.Dominio.Resource;
using Aprendizaje.Dominio.Resource.Repositorios;
using Aprendizaje.Dominio.Roadmap.Repositorios;
using Aprendizaje.Tests.Soporte;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Aprendizaje.Tests.Api;

[Collection("ApiEndpoints")]
public sealed class RecursoEndpointsTests
{
    [Fact]
    public async Task GetRecursos_EnPersonal_SinUsuarioIdExplicito_UsaUsuarioActualYPasaTemaId()
    {
        var usuario = Usuario.Registrar("Dylan", EmailUnico());
        var temaId = Guid.CreateVersion7();
        var recursoId = Guid.CreateVersion7();
        await using var app = await AplicacionApiPrueba.CrearAsync(
            "Personal",
            [usuario],
            recursos:
            [
                new RecursoResumen(
                    recursoId,
                    usuario.Id,
                    TipoRecurso.Documentacion,
                    "Documentacion API",
                    null,
                    EstadoRecurso.PorClasificar,
                    [new RecursoTemaResumen(temaId, "Redes")])
            ]);

        var respuesta = await app.Client.GetAsync($"/api/recursos?temaId={temaId}", CancellationToken);

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        Assert.Equal(usuario.Id, app.Consulta.SolicitudRecibida?.UsuarioId);
        Assert.Equal(temaId, app.Consulta.SolicitudRecibida?.TemaId);
        using var json = await JsonDocument.ParseAsync(
            await respuesta.Content.ReadAsStreamAsync(CancellationToken),
            cancellationToken: CancellationToken);
        var item = json.RootElement[0];
        Assert.Equal(recursoId, item.GetProperty("id").GetGuid());
        Assert.Equal(temaId, item.GetProperty("temas")[0].GetProperty("id").GetGuid());
        Assert.Equal("Redes", item.GetProperty("temas")[0].GetProperty("nombre").GetString());
    }

    [Fact]
    public async Task GetRecursoPorId_ExponeTemasRelacionados()
    {
        var usuario = Usuario.Registrar("Dylan", EmailUnico());
        var recursoId = Guid.CreateVersion7();
        var temaId = Guid.CreateVersion7();
        await using var app = await AplicacionApiPrueba.CrearAsync(
            "Personal",
            [usuario],
            detalle: new RecursoDetalle(
                recursoId,
                usuario.Id,
                TipoRecurso.Video,
                "Video API",
                "https://example.local/video",
                EstadoRecurso.EnUso,
                4,
                "Notas",
                null,
                null,
                [new RecursoTemaResumen(temaId, "Web")]));

        var respuesta = await app.Client.GetAsync($"/api/recursos/{recursoId}", CancellationToken);

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        using var json = await JsonDocument.ParseAsync(
            await respuesta.Content.ReadAsStreamAsync(CancellationToken),
            cancellationToken: CancellationToken);
        Assert.Equal(recursoId, json.RootElement.GetProperty("id").GetGuid());
        Assert.Equal("Web", json.RootElement.GetProperty("temas")[0].GetProperty("nombre").GetString());
    }

    [Fact]
    public async Task GetRecursoPorId_EnPersonal_OcultaRecursoAjeno()
    {
        var usuario = Usuario.Registrar("Dylan", EmailUnico());
        var recursoId = Guid.CreateVersion7();
        await using var app = await AplicacionApiPrueba.CrearAsync(
            "Personal",
            [usuario],
            detalle: new RecursoDetalle(
                recursoId,
                Guid.CreateVersion7(),
                TipoRecurso.Video,
                "Video ajeno",
                null,
                EstadoRecurso.EnUso,
                null,
                null,
                null,
                null,
                []));

        var respuesta = await app.Client.GetAsync($"/api/recursos/{recursoId}", CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
    }

    [Fact]
    public async Task GetRecursos_FueraDePersonal_SinUsuarioIdExplicito_RetornaBadRequest()
    {
        await using var app = await AplicacionApiPrueba.CrearAsync("Development", []);

        var respuesta = await app.Client.GetAsync("/api/recursos", CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
        Assert.Equal(0, app.Consulta.LlamadasLista);
    }

    private sealed class AplicacionApiPrueba : IAsyncDisposable
    {
        private readonly WebApplication _app;

        private AplicacionApiPrueba(WebApplication app, HttpClient client, FakeConsultaRecursosV1 consulta)
        {
            _app = app;
            Client = client;
            Consulta = consulta;
        }

        public HttpClient Client { get; }

        public FakeConsultaRecursosV1 Consulta { get; }

        public static async Task<AplicacionApiPrueba> CrearAsync(
            string environmentName,
            IReadOnlyCollection<Usuario> usuarios,
            IReadOnlyCollection<RecursoResumen>? recursos = null,
            RecursoDetalle? detalle = null)
        {
            var repositorioUsuarios = new FakeUsuarioRepository();
            foreach (var usuario in usuarios)
                repositorioUsuarios.Agregar(usuario);

            var consulta = new FakeConsultaRecursosV1
            {
                Recursos = recursos ?? [],
                Detalle = detalle
            };

            var builder = WebApplication.CreateBuilder(new WebApplicationOptions
            {
                EnvironmentName = environmentName
            });
            builder.WebHost.UseUrls("http://127.0.0.1:0");
            builder.Services.AddSingleton<IUsuarioRepository>(repositorioUsuarios);
            builder.Services.AddSingleton<IRecursoRepository>(new FakeRecursoRepository());
            builder.Services.AddSingleton<ITemaRepository>(new FakeTemaRepository());
            builder.Services.AddSingleton<IUnitOfWork>(new FakeUnitOfWork());
            builder.Services.AddSingleton<IConsultaRecursosV1>(consulta);
            builder.Services.AddScoped<IUsuarioActual, UsuarioActualLocal>();
            builder.Services.AddScoped<CrearRecursoCasoUso>();
            builder.Services.AddScoped<ActualizarRecursoCasoUso>();
            builder.Services.AddScoped<EliminarRecursoCasoUso>();
            builder.Services.AddScoped<ListarRecursosCasoUso>();
            builder.Services.AddScoped<ObtenerRecursoPorIdCasoUso>();
            builder.Services.AddScoped<VincularRecursoATemaCasoUso>();
            builder.Services.ConfigureHttpJsonOptions(options =>
                options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

            var app = builder.Build();
            app.MapRecursoEndpoints();
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

    private sealed class FakeConsultaRecursosV1 : IConsultaRecursosV1
    {
        public int LlamadasLista { get; private set; }

        public ListarRecursosSolicitud? SolicitudRecibida { get; private set; }

        public IReadOnlyCollection<RecursoResumen> Recursos { get; init; } = [];

        public RecursoDetalle? Detalle { get; init; }

        public Task<IReadOnlyCollection<RecursoResumen>> ListarAsync(
            ListarRecursosSolicitud solicitud,
            CancellationToken cancellationToken = default)
        {
            LlamadasLista++;
            SolicitudRecibida = solicitud;

            return Task.FromResult(Recursos);
        }

        public Task<RecursoDetalle?> ObtenerDetalleAsync(
            Guid id,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(Detalle?.Id == id ? Detalle : null);
    }

    private static string EmailUnico() => $"recurso-api-{Guid.CreateVersion7():N}@local.test";

    private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;
}
