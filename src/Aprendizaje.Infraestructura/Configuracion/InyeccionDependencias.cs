using Aprendizaje.Aplicacion.Comun;
using Aprendizaje.Dominio.Evidence.Repositorios;
using Aprendizaje.Dominio.Nucleo.Repositorios;
using Aprendizaje.Dominio.Resource.Repositorios;
using Aprendizaje.Dominio.Roadmap.Repositorios;
using Aprendizaje.Dominio.Study.Repositorios;
using Aprendizaje.Infraestructura.Persistencia;
using Aprendizaje.Infraestructura.Persistencia.Interceptores;
using Aprendizaje.Infraestructura.Persistencia.Repositorios;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Aprendizaje.Infraestructura.Configuracion;

public static class InyeccionDependencias
{
    public static IServiceCollection AddInfraestructura(this IServiceCollection services, IConfiguration configuration)
    {
        // Sin estado propio: un único AuditoriaInterceptor puede reutilizarse en todo el
        // ciclo de vida de la aplicación.
        services.AddSingleton<AuditoriaInterceptor>();

        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<ITemaRepository, TemaRepository>();
        services.AddScoped<IFaseRepository, FaseRepository>();
        services.AddScoped<ICompetenciaRepository, CompetenciaRepository>();
        services.AddScoped<ICertificacionRepository, CertificacionRepository>();
        services.AddScoped<ILaboratorioRepository, LaboratorioRepository>();
        services.AddScoped<IRecursoRepository, RecursoRepository>();
        services.AddScoped<IEntradaBitacoraRepository, EntradaBitacoraRepository>();
        services.AddScoped<IHerramientaRepository, HerramientaRepository>();
        services.AddScoped<ISesionEstudioRepository, SesionEstudioRepository>();
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AprendizajeDbContext>());

        services.AddDbContext<AprendizajeDbContext>((sp, options) =>
        {
            options.UseSqlServer(configuration.GetConnectionString("AprendizajeDb"));
            options.AddInterceptors(sp.GetRequiredService<AuditoriaInterceptor>());

            // DespachoEventosInterceptor NO se registra ni se conecta aquí todavía: su único
            // constructor exige IDespachadorEventos, cuya implementación concreta está
            // deliberadamente diferida a la Fase 3 (convención 16.3). Registrarlo ahora sin una
            // implementación real de esa interfaz rompería la resolución de dependencias en
            // cuanto se solicitara una instancia de AprendizajeDbContext — no es un olvido, es
            // un pendiente documentado a propósito. Se conecta aquí mismo, en esta línea, en
            // cuanto exista una implementación real de IDespachadorEventos.
        });

        return services;
    }
}
