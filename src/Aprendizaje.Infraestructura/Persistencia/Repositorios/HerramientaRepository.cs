using Aprendizaje.Dominio.Study;
using Aprendizaje.Dominio.Study.Repositorios;
using Microsoft.EntityFrameworkCore;

namespace Aprendizaje.Infraestructura.Persistencia.Repositorios;

public sealed class HerramientaRepository : IHerramientaRepository
{
    private readonly AprendizajeDbContext _context;

    public HerramientaRepository(AprendizajeDbContext context)
    {
        _context = context;
    }

    public Task<Herramienta?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.Herramientas.FirstOrDefaultAsync(h => h.Id == id, cancellationToken);

    public async Task<IReadOnlyCollection<Herramienta>> ListarAsync(CancellationToken cancellationToken = default) =>
        await _context.Herramientas
            .AsNoTracking()
            .OrderBy(h => h.Nombre)
            .ThenBy(h => h.Id)
            .ToListAsync(cancellationToken);

    public void Agregar(Herramienta herramienta) => _context.Herramientas.Add(herramienta);
}
