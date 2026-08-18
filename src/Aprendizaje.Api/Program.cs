using Aprendizaje.Infraestructura.Configuracion;
using Aprendizaje.Api.Endpoints.Nucleo;
using Aprendizaje.Api.Endpoints.Roadmap;
using Aprendizaje.Aplicacion.Nucleo.Usuarios.CrearUsuario;
using Aprendizaje.Aplicacion.Roadmap.Fases.CrearFase;
using Aprendizaje.Aplicacion.Roadmap.Temas.CrearTema;
using Aprendizaje.Aplicacion.Roadmap.Temas.EstablecerObjetivos;
using Aprendizaje.Aplicacion.Roadmap.Temas.ListarTemas;
using Aprendizaje.Aplicacion.Roadmap.Temas.ObtenerTemaPorId;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<CrearFaseCasoUso>();
builder.Services.AddScoped<CrearTemaCasoUso>();
builder.Services.AddScoped<ObtenerTemaPorIdCasoUso>();
builder.Services.AddScoped<EstablecerObjetivosTemaCasoUso>();
builder.Services.AddScoped<ListarTemasCasoUso>();
builder.Services.AddScoped<CrearUsuarioCasoUso>();
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
builder.Services.AddInfraestructura(builder.Configuration);

var app = builder.Build();

app.MapFaseEndpoints();
app.MapTemaEndpoints();
app.MapUsuarioEndpoints();

app.Run();
