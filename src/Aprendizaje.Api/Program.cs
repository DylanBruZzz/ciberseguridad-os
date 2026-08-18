using Aprendizaje.Infraestructura.Configuracion;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfraestructura(builder.Configuration);

var app = builder.Build();

app.Run();