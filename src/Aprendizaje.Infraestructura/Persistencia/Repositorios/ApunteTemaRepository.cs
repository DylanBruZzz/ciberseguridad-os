using Aprendizaje.Dominio.Roadmap;
using Aprendizaje.Dominio.Roadmap.Repositorios;
using Microsoft.EntityFrameworkCore;

namespace Aprendizaje.Infraestructura.Persistencia.Repositorios;

public sealed class ApunteTemaRepository : IApunteTemaRepository
{
    private readonly AprendizajeDbContext _context;

    public ApunteTemaRepository(AprendizajeDbContext context)
    {
        _context = context;
    }

    public Task<ApunteTema?> ObtenerPorTemaIdAsync(Guid temaId, CancellationToken cancellationToken = default) =>
        _context.ApuntesTema.FirstOrDefaultAsync(a => a.TemaId == temaId, cancellationToken);

    public void Agregar(ApunteTema apunte) => _context.ApuntesTema.Add(apunte);
}
