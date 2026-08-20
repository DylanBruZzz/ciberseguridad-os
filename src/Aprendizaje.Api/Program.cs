using Aprendizaje.Api.Endpoints.Nucleo;
using Aprendizaje.Api.Endpoints.Resource;
using Aprendizaje.Api.Endpoints.Roadmap;
using Aprendizaje.Api.Endpoints.Study;
using Aprendizaje.Aplicacion.Nucleo.Usuarios.CrearUsuario;
using Aprendizaje.Aplicacion.Resource.Recursos.CrearRecurso;
using Aprendizaje.Aplicacion.Resource.Recursos.ListarRecursos;
using Aprendizaje.Aplicacion.Resource.Recursos.ObtenerRecursoPorId;
using Aprendizaje.Aplicacion.Resource.Recursos.VincularRecursoATema;
using Aprendizaje.Aplicacion.Roadmap.Fases.CrearFase;
using Aprendizaje.Aplicacion.Roadmap.Fases.ListarFases;
using Aprendizaje.Aplicacion.Roadmap.Temas.AsignarTemaAFase;
using Aprendizaje.Aplicacion.Roadmap.Temas.AsignarTemaPadre;
using Aprendizaje.Aplicacion.Roadmap.Temas.CrearTema;
using Aprendizaje.Aplicacion.Roadmap.Temas.DefinirCriteriosRelevantes;
using Aprendizaje.Aplicacion.Roadmap.Temas.DesmarcarCriterio;
using Aprendizaje.Aplicacion.Roadmap.Temas.EstablecerObjetivos;
using Aprendizaje.Aplicacion.Roadmap.Temas.ListarTemas;
using Aprendizaje.Aplicacion.Roadmap.Temas.MarcarCriterio;
using Aprendizaje.Aplicacion.Roadmap.Temas.ObtenerTemaPorId;
using Aprendizaje.Aplicacion.Study.SesionesEstudio.CorregirDuracionSesionEstudio;
using Aprendizaje.Aplicacion.Study.SesionesEstudio.ListarSesionesEstudio;
using Aprendizaje.Aplicacion.Study.SesionesEstudio.ObtenerSesionEstudioPorId;
using Aprendizaje.Aplicacion.Study.SesionesEstudio.RegistrarSesionEstudio;
using Aprendizaje.Infraestructura.Configuracion;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<CrearFaseCasoUso>();
builder.Services.AddScoped<ListarFasesCasoUso>();
builder.Services.AddScoped<AsignarTemaAFaseCasoUso>();
builder.Services.AddScoped<AsignarTemaPadreCasoUso>();
builder.Services.AddScoped<CrearTemaCasoUso>();
builder.Services.AddScoped<DefinirCriteriosRelevantesTemaCasoUso>();
builder.Services.AddScoped<DesmarcarCriterioTemaCasoUso>();
builder.Services.AddScoped<ObtenerTemaPorIdCasoUso>();
builder.Services.AddScoped<EstablecerObjetivosTemaCasoUso>();
builder.Services.AddScoped<ListarTemasCasoUso>();
builder.Services.AddScoped<MarcarCriterioTemaCasoUso>();
builder.Services.AddScoped<CrearUsuarioCasoUso>();
builder.Services.AddScoped<CrearRecursoCasoUso>();
builder.Services.AddScoped<ListarRecursosCasoUso>();
builder.Services.AddScoped<ObtenerRecursoPorIdCasoUso>();
builder.Services.AddScoped<VincularRecursoATemaCasoUso>();
builder.Services.AddScoped<CorregirDuracionSesionEstudioCasoUso>();
builder.Services.AddScoped<ListarSesionesEstudioCasoUso>();
builder.Services.AddScoped<ObtenerSesionEstudioPorIdCasoUso>();
builder.Services.AddScoped<RegistrarSesionEstudioCasoUso>();
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
builder.Services.AddInfraestructura(builder.Configuration);

var app = builder.Build();

app.MapFaseEndpoints();
app.MapRecursoEndpoints();
app.MapSesionEstudioEndpoints();
app.MapTemaEndpoints();
app.MapUsuarioEndpoints();

app.Run();
