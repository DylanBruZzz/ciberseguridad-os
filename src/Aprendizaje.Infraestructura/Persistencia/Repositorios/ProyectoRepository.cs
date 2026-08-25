using Aprendizaje.Dominio.Evidence;
using Aprendizaje.Dominio.Evidence.Repositorios;
using Aprendizaje.Infraestructura.Persistencia.ModelosUnion;
using Microsoft.EntityFrameworkCore;

namespace Aprendizaje.Infraestructura.Persistencia.Repositorios;

public sealed class ProyectoRepository : IProyectoRepository
{
    private readonly AprendizajeDbContext _context;

    public ProyectoRepository(AprendizajeDbContext context)
    {
        _context = context;
    }

    public Task<Proyecto?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.Proyectos.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<IReadOnlyCollection<Proyecto>> ListarPorUsuarioAsync(
        Guid usuarioId,
        CancellationToken cancellationToken = default) =>
        await _context.Proyectos
            .AsNoTracking()
            .Where(p => p.UsuarioId == usuarioId)
            .OrderByDescending(p => p.FechaInicio)
            .ThenBy(p => p.Id)
            .ToListAsync(cancellationToken);

    public Task<bool> ExisteVinculoTemaAsync(
        Guid proyectoId,
        Guid temaId,
        CancellationToken cancellationToken = default) =>
        _context.Set<ProyectoTema>()
            .AnyAsync(p => p.ProyectoId == proyectoId && p.TemaId == temaId, cancellationToken);

    public Task<bool> ExisteVinculoHerramientaAsync(
        Guid proyectoId,
        Guid herramientaId,
        CancellationToken cancellationToken = default) =>
        _context.Set<ProyectoHerramienta>()
            .AnyAsync(p => p.ProyectoId == proyectoId && p.HerramientaId == herramientaId, cancellationToken);

    public void Agregar(Proyecto proyecto) => _context.Proyectos.Add(proyecto);

    public void VincularTema(Guid proyectoId, Guid temaId) =>
        _context.Set<ProyectoTema>().Add(new ProyectoTema(proyectoId, temaId));

    public void VincularHerramienta(Guid proyectoId, Guid herramientaId) =>
        _context.Set<ProyectoHerramienta>().Add(new ProyectoHerramienta(proyectoId, herramientaId));
}
