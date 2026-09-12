using System.Net;
using Aprendizaje.Api.Endpoints.Roadmap;
using Aprendizaje.Aplicacion.Comun;
using Aprendizaje.Aplicacion.Nucleo.Usuarios;
using Aprendizaje.Aplicacion.Roadmap.Temas.DesvincularHerramientaDeTema;
using Aprendizaje.Aplicacion.Roadmap.Temas.VincularHerramientaATema;
using Aprendizaje.Dominio.Nucleo;
using Aprendizaje.Dominio.Nucleo.Repositorios;
using Aprendizaje.Dominio.Roadmap;
using Aprendizaje.Dominio.Roadmap.Repositorios;
using Aprendizaje.Dominio.Study;
using Aprendizaje.Dominio.Study.Repositorios;
using Aprendizaje.Tests.Soporte;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Aprendizaje.Tests.Api;

[Collection("ApiEndpoints")]
public sealed class TemaHerramientaEndpointsTests
{
    [Fact]
    public async Task PutTemaHerramienta_EnPersonal_UsaUsuarioActualYVincula()
    {
        var usuario = Usuario.Registrar("Dylan", EmailUnico());
        var tema = Tema.Crear(usuario.Id, "Analisis de trafico", TipoConocimiento.Procedimental);
        var herramienta = Herramienta.Crear("Wireshark");
        await using var app = await AplicacionApiPrueba.CrearAsync("Personal", [usuario], [tema], [herramienta]);

        var respuesta = await app.Client.PutAsync(
            $"/api/temas/{tema.Id}/herramientas/{herramienta.Id}",
            content: null,
            CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, respuesta.StatusCode);
        Assert.True(await app.Temas.ExisteVinculoHerramientaAsync(tema.Id, herramienta.Id, CancellationToken));
        Assert.Equal(1, app.UnitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task DeleteTemaHerramienta_EnPersonal_EliminaSoloElVinculo()
    {
        var usuario = Usuario.Registrar("Dylan", EmailUnico());
        var tema = Tema.Crear(usuario.Id, "Active Directory", TipoConocimiento.Procedimental);
        var herramienta = Herramienta.Crear("BloodHound");
        await using var app = await AplicacionApiPrueba.CrearAsync("Personal", [usuario], [tema], [herramienta]);
        app.Temas.VincularHerramienta(tema.Id, herramienta.Id);

        var respuesta = await app.Client.DeleteAsync(
            $"/api/temas/{tema.Id}/herramientas/{herramienta.Id}",
            CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, respuesta.StatusCode);
        Assert.False(await app.Temas.ExisteVinculoHerramientaAsync(tema.Id, herramienta.Id, CancellationToken));
        Assert.NotNull(await app.Herramientas.ObtenerPorIdAsync(herramienta.Id, CancellationToken));
        Assert.Equal(1, app.UnitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task PutTemaHerramienta_EnPersonal_TemaAjenoRetornaNotFound()
    {
        var usuario = Usuario.Registrar("Dylan", EmailUnico());
        var temaAjeno = Tema.Crear(Guid.CreateVersion7(), "Tema ajeno", TipoConocimiento.Conceptual);
        var herramienta = Herramienta.Crear("PowerShell");
        await using var app = await AplicacionApiPrueba.CrearAsync("Personal", [usuario], [temaAjeno], [herramienta]);

        var respuesta = await app.Client.PutAsync(
            $"/api/temas/{temaAjeno.Id}/herramientas/{herramienta.Id}",
            content: null,
            CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
        Assert.False(await app.Temas.ExisteVinculoHerramientaAsync(temaAjeno.Id, herramienta.Id, CancellationToken));
        Assert.Equal(0, app.UnitOfWork.GuardarCambiosLlamadas);
    }

    [Fact]
    public async Task PutTemaHerramienta_FueraDePersonal_SinUsuarioIdRetornaBadRequest()
    {
        var tema = Tema.Crear(Guid.CreateVersion7(), "Tema", TipoConocimiento.Conceptual);
        var herramienta = Herramienta.Crear("nmap");
        await using var app = await AplicacionApiPrueba.CrearAsync("Development", [], [tema], [herramienta]);

        var respuesta = await app.Client.PutAsync(
            $"/api/temas/{tema.Id}/herramientas/{herramienta.Id}",
            content: null,
            CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
        Assert.Equal(0, app.UnitOfWork.GuardarCambiosLlamadas);
    }

    private sealed class AplicacionApiPrueba : IAsyncDisposable
    {
        private readonly WebApplication _app;

        private AplicacionApiPrueba(
            WebApplication app,
            HttpClient client,
            FakeTemaRepository temas,
            FakeHerramientaRepository herramientas,
            FakeUnitOfWork unitOfWork)
        {
            _app = app;
            Client = client;
            Temas = temas;
            Herramientas = herramientas;
            UnitOfWork = unitOfWork;
        }

        public HttpClient Client { get; }
        public FakeTemaRepository Temas { get; }
        public FakeHerramientaRepository Herramientas { get; }
        public FakeUnitOfWork UnitOfWork { get; }

        public static async Task<AplicacionApiPrueba> CrearAsync(
            string environmentName,
            IReadOnlyCollection<Usuario> usuarios,
            IReadOnlyCollection<Tema> temasIniciales,
            IReadOnlyCollection<Herramienta> herramientasIniciales)
        {
            var repositorioUsuarios = new FakeUsuarioRepository();
            foreach (var usuario in usuarios)
                repositorioUsuarios.Agregar(usuario);

            var temas = new FakeTemaRepository();
            foreach (var tema in temasIniciales)
                temas.Agregar(tema);

            var herramientas = new FakeHerramientaRepository();
            foreach (var herramienta in herramientasIniciales)
                herramientas.Agregar(herramienta);

            var unitOfWork = new FakeUnitOfWork();
            var builder = WebApplication.CreateBuilder(new WebApplicationOptions
            {
                EnvironmentName = environmentName
            });
            builder.WebHost.UseUrls("http://127.0.0.1:0");
            builder.Services.AddSingleton<IUsuarioRepository>(repositorioUsuarios);
            builder.Services.AddSingleton<ITemaRepository>(temas);
            builder.Services.AddSingleton<IHerramientaRepository>(herramientas);
            builder.Services.AddSingleton<IUnitOfWork>(unitOfWork);
            builder.Services.AddScoped<IUsuarioActual, UsuarioActualLocal>();
            builder.Services.AddScoped<VincularHerramientaATemaCasoUso>();
            builder.Services.AddScoped<DesvincularHerramientaDeTemaCasoUso>();

            var app = builder.Build();
            app.MapTemaHerramientaEndpoints();
            await app.StartAsync(CancellationToken);

            var direccion = app.Services
                .GetRequiredService<IServer>()
                .Features
                .Get<IServerAddressesFeature>()!
                .Addresses
                .Single();
            var client = new HttpClient { BaseAddress = new Uri(direccion) };

            return new AplicacionApiPrueba(app, client, temas, herramientas, unitOfWork);
        }

        public async ValueTask DisposeAsync()
        {
            Client.Dispose();
            await _app.StopAsync(CancellationToken.None);
            await _app.DisposeAsync();
        }
    }

    private static string EmailUnico() => $"tema-herramienta-api-{Guid.CreateVersion7():N}@local.test";

    private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;
}
