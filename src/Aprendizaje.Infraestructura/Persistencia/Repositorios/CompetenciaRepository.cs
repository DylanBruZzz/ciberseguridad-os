using Aprendizaje.Dominio.Roadmap;
using Aprendizaje.Dominio.Roadmap.Repositorios;
using Aprendizaje.Infraestructura.Persistencia.ModelosUnion;
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

    public async Task<IReadOnlyCollection<Competencia>> ListarPorUsuarioAsync(
        Guid usuarioId,
        CancellationToken cancellationToken = default) =>
        await _context.Competencias
            .AsNoTracking()
            .Where(c => c.UsuarioId == usuarioId)
            .OrderBy(c => c.Nombre)
            .ThenBy(c => c.Id)
            .ToListAsync(cancellationToken);

    public Task<bool> ExisteVinculoTemaAsync(
        Guid competenciaId,
        Guid temaId,
        CancellationToken cancellationToken = default) =>
        _context.Set<CompetenciaTema>()
            .AnyAsync(c => c.CompetenciaId == competenciaId && c.TemaId == temaId, cancellationToken);

    public void Agregar(Competencia competencia) => _context.Competencias.Add(competencia);

    public void VincularTema(Guid competenciaId, Guid temaId) =>
        _context.Set<CompetenciaTema>().Add(new CompetenciaTema(competenciaId, temaId));
}
