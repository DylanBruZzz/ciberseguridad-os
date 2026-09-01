using Aprendizaje.Aplicacion.Comun;
using Aprendizaje.Dominio.Analytics;
using Aprendizaje.Dominio.Evidence;
using Aprendizaje.Dominio.Integration;
using Aprendizaje.Dominio.Nucleo;
using Aprendizaje.Dominio.Resource;
using Aprendizaje.Dominio.Roadmap;
using Aprendizaje.Dominio.Study;
using Microsoft.EntityFrameworkCore;

namespace Aprendizaje.Infraestructura.Persistencia;

public sealed class AprendizajeDbContext : DbContext, IUnitOfWork
{
    public AprendizajeDbContext(DbContextOptions<AprendizajeDbContext> options) : base(options)
    {
    }

    // Nucleo
    public DbSet<Usuario> Usuarios => Set<Usuario>();

    // Roadmap
    public DbSet<Fase> Fases => Set<Fase>();
    public DbSet<Certificacion> Certificaciones => Set<Certificacion>();
    public DbSet<Tema> Temas => Set<Tema>();
    public DbSet<ApunteTema> ApuntesTema => Set<ApunteTema>();
    public DbSet<Competencia> Competencias => Set<Competencia>();

    // Study
    public DbSet<Herramienta> Herramientas => Set<Herramienta>();
    public DbSet<SesionEstudio> SesionesEstudio => Set<SesionEstudio>();
    public DbSet<EntradaBitacora> EntradasBitacora => Set<EntradaBitacora>();

    // Evidence
    public DbSet<Proyecto> Proyectos => Set<Proyecto>();
    public DbSet<Laboratorio> Laboratorios => Set<Laboratorio>();
    public DbSet<Writeup> Writeups => Set<Writeup>();
    public DbSet<ArtefactoTecnico> ArtefactosTecnicos => Set<ArtefactoTecnico>();
    public DbSet<CertificacionObtenida> CertificacionesObtenidas => Set<CertificacionObtenida>();
    public DbSet<Nota> Notas => Set<Nota>();

    // Resource
    public DbSet<Recurso> Recursos => Set<Recurso>();

    // Analytics
    public DbSet<SnapshotProgreso> SnapshotsProgreso => Set<SnapshotProgreso>();

    // Integration
    public DbSet<Conector> Conectores => Set<Conector>();

    // Nota: CriterioTema y LogSincronizacion son entidades internas — se acceden únicamente
    // a través de su Aggregate Root dueño (Tema y Conector respectivamente), nunca por DbSet propio.

    public Task<int> GuardarCambiosAsync(CancellationToken cancellationToken = default) =>
        SaveChangesAsync(cancellationToken);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AprendizajeDbContext).Assembly);
    }
}
