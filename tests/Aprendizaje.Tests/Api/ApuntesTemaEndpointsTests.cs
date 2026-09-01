using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Aprendizaje.Api.Endpoints.Roadmap;
using Aprendizaje.Aplicacion.Comun;
using Aprendizaje.Aplicacion.Nucleo.Usuarios;
using Aprendizaje.Aplicacion.Roadmap.Temas.ActualizarPercepcion;
using Aprendizaje.Aplicacion.Roadmap.Temas.ActualizarPlanificacion;
using Aprendizaje.Aplicacion.Roadmap.Temas.AsignarTemaAFase;
using Aprendizaje.Aplicacion.Roadmap.Temas.AsignarTemaPadre;
using Aprendizaje.Aplicacion.Roadmap.Temas.ConfigurarIntervaloRepaso;
using Aprendizaje.Aplicacion.Roadmap.Temas.CrearTema;
using Aprendizaje.Aplicacion.Roadmap.Temas.DefinirCriteriosRelevantes;
using Aprendizaje.Aplicacion.Roadmap.Temas.DesmarcarCriterio;
using Aprendizaje.Aplicacion.Roadmap.Temas.EstablecerObjetivos;
using Aprendizaje.Aplicacion.Roadmap.Temas.GuardarApuntesTema;
using Aprendizaje.Aplicacion.Roadmap.Temas.ListarTemas;
using Aprendizaje.Aplicacion.Roadmap.Temas.MarcarCriterio;
using Aprendizaje.Aplicacion.Roadmap.Temas.ObtenerApuntesTema;
using Aprendizaje.Aplicacion.Roadmap.Temas.ObtenerTemaPorId;
using Aprendizaje.Dominio.Nucleo;
using Aprendizaje.Dominio.Nucleo.Repositorios;
using Aprendizaje.Dominio.Roadmap;
using Aprendizaje.Dominio.Roadmap.Repositorios;
using Aprendizaje.Tests.Soporte;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Aprendizaje.Tests.Api;

public sealed class ApuntesTemaEndpointsTests
{
    [Fact]
    public async Task GetApuntes_EnPersonal_SinUsuarioIdExplicito_RetornaContenidoVacio()
    {
        await using var app = await AplicacionApiPrueba.CrearAsync();
        var tema = app.AgregarTemaDelUsuarioActual();

        var respuesta = await app.Client.GetAsync($"/api/temas/{tema.Id}/apuntes", CancellationToken);

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        using var json = await JsonDocument.ParseAsync(
            await respuesta.Content.ReadAsStreamAsync(CancellationToken),
            cancellationToken: CancellationToken);
        Assert.Equal(tema.Id, json.RootElement.GetProperty("temaId").GetGuid());
        Assert.Equal(string.Empty, json.RootElement.GetProperty("contenido").GetString());
        Assert.Equal(JsonValueKind.Null, json.RootElement.GetProperty("fechaModificacionUtc").ValueKind);
    }

    [Fact]
    public async Task PutApuntes_EnPersonal_SinUsuarioIdExplicito_GuardaYGetPosteriorLoDevuelve()
    {
        await using var app = await AplicacionApiPrueba.CrearAsync();
        var tema = app.AgregarTemaDelUsuarioActual();

        var put = await app.Client.PutAsJsonAsync(
            $"/api/temas/{tema.Id}/apuntes",
            new { contenido = "  TCP confirma entrega; UDP no  " },
            CancellationToken);
        var get = await app.Client.GetAsync($"/api/temas/{tema.Id}/apuntes", CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, put.StatusCode);
        Assert.Equal(HttpStatusCode.OK, get.StatusCode);
        using var json = await JsonDocument.ParseAsync(
            await get.Content.ReadAsStreamAsync(CancellationToken),
            cancellationToken: CancellationToken);
        Assert.Equal("TCP confirma entrega; UDP no", json.RootElement.GetProperty("contenido").GetString());
        Assert.Equal(1, app.Apuntes.AgregarLlamadas);
    }

    [Fact]
    public async Task PutApuntes_EnPersonal_TemaDeOtroUsuario_RetornaNotFoundSinGuardar()
    {
        await using var app = await AplicacionApiPrueba.CrearAsync();
        var temaAjeno = app.AgregarTema(Guid.CreateVersion7());

        var respuesta = await app.Client.PutAsJsonAsync(
            $"/api/temas/{temaAjeno.Id}/apuntes",
            new { contenido = "No debería guardar" },
            CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
        Assert.Equal(0, app.Apuntes.AgregarLlamadas);
    }

    [Fact]
    public async Task GetApuntes_EnPersonal_TemaDeOtroUsuario_RetornaNotFound()
    {
        await using var app = await AplicacionApiPrueba.CrearAsync();
        var temaAjeno = app.AgregarTema(Guid.CreateVersion7());

        var respuesta = await app.Client.GetAsync($"/api/temas/{temaAjeno.Id}/apuntes", CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
    }

    private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

    private sealed class AplicacionApiPrueba : IAsyncDisposable
    {
        private readonly WebApplication _app;

        private AplicacionApiPrueba(
            WebApplication app,
            HttpClient client,
            Usuario usuarioActual,
            FakeTemaRepository temas,
            FakeApunteTemaRepository apuntes)
        {
            _app = app;
            Client = client;
            UsuarioActual = usuarioActual;
            Temas = temas;
            Apuntes = apuntes;
        }

        public HttpClient Client { get; }

        public Usuario UsuarioActual { get; }

        public FakeTemaRepository Temas { get; }

        public FakeApunteTemaRepository Apuntes { get; }

        public static async Task<AplicacionApiPrueba> CrearAsync()
        {
            var usuario = Usuario.Registrar("Dylan", $"api-{Guid.CreateVersion7():N}@local.test");
            var usuarios = new FakeUsuarioRepository();
            usuarios.Agregar(usuario);

            var temas = new FakeTemaRepository();
            var apuntes = new FakeApunteTemaRepository();
            var unitOfWork = new FakeUnitOfWork();

            var builder = WebApplication.CreateBuilder(new WebApplicationOptions
            {
                EnvironmentName = "Personal"
            });
            builder.WebHost.UseUrls("http://127.0.0.1:0");
            builder.Services.AddSingleton<IUsuarioRepository>(usuarios);
            builder.Services.AddSingleton<ITemaRepository>(temas);
            builder.Services.AddSingleton<IFaseRepository>(new FakeFaseRepository());
            builder.Services.AddSingleton<IApunteTemaRepository>(apuntes);
            builder.Services.AddSingleton<IUnitOfWork>(unitOfWork);
            builder.Services.AddScoped<IUsuarioActual, UsuarioActualLocal>();
            builder.Services.AddScoped<ActualizarPercepcionTemaCasoUso>();
            builder.Services.AddScoped<ActualizarPlanificacionTemaCasoUso>();
            builder.Services.AddScoped<AsignarTemaAFaseCasoUso>();
            builder.Services.AddScoped<AsignarTemaPadreCasoUso>();
            builder.Services.AddScoped<ConfigurarIntervaloRepasoTemaCasoUso>();
            builder.Services.AddScoped<CrearTemaCasoUso>();
            builder.Services.AddScoped<DefinirCriteriosRelevantesTemaCasoUso>();
            builder.Services.AddScoped<DesmarcarCriterioTemaCasoUso>();
            builder.Services.AddScoped<EstablecerObjetivosTemaCasoUso>();
            builder.Services.AddScoped<ObtenerApuntesTemaCasoUso>();
            builder.Services.AddScoped<GuardarApuntesTemaCasoUso>();
            builder.Services.AddScoped<ListarTemasCasoUso>();
            builder.Services.AddScoped<MarcarCriterioTemaCasoUso>();
            builder.Services.AddScoped<ObtenerTemaPorIdCasoUso>();

            var app = builder.Build();
            app.MapTemaEndpoints();
            await app.StartAsync(CancellationToken);

            var direccion = app.Services
                .GetRequiredService<IServer>()
                .Features
                .Get<IServerAddressesFeature>()!
                .Addresses
                .Single();
            var client = new HttpClient { BaseAddress = new Uri(direccion) };

            return new AplicacionApiPrueba(app, client, usuario, temas, apuntes);
        }

        public Tema AgregarTemaDelUsuarioActual() => AgregarTema(UsuarioActual.Id);

        public Tema AgregarTema(Guid usuarioId)
        {
            var tema = Tema.Crear(usuarioId, $"Tema API {Guid.CreateVersion7():N}", TipoConocimiento.Conceptual);
            Temas.Agregar(tema);

            return tema;
        }

        public async ValueTask DisposeAsync()
        {
            Client.Dispose();
            await _app.StopAsync(CancellationToken.None);
            await _app.DisposeAsync();
        }
    }
}
