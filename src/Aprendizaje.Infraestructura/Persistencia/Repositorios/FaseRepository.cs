using Aprendizaje.Dominio.Roadmap;
using Aprendizaje.Dominio.Roadmap.Repositorios;
using Microsoft.EntityFrameworkCore;

namespace Aprendizaje.Infraestructura.Persistencia.Repositorios;

public sealed class FaseRepository : IFaseRepository
{
    private readonly AprendizajeDbContext _context;

    public FaseRepository(AprendizajeDbContext context)
    {
        _context = context;
    }

    public Task<Fase?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.Fases.FirstOrDefaultAsync(f => f.Id == id, cancellationToken);

    public void Agregar(Fase fase) => _context.Fases.Add(fase);
}
