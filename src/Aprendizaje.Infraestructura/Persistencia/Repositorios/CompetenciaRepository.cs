using Aprendizaje.Dominio.Roadmap;
using Aprendizaje.Dominio.Roadmap.Repositorios;
using Microsoft.EntityFrameworkCore;

namespace Aprendizaje.Infraestructura.Persistencia.Repositorios;

public sealed class CompetenciaRepository : ICompetenciaRepository
{
    private readonly AprendizajeDbContext _context;

    public CompetenciaRepository(AprendizajeDbContext context)
    {
        _context = context;
    }

    public Task<Competencia?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.Competencias.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public void Agregar(Competencia competencia) => _context.Competencias.Add(competencia);
}
