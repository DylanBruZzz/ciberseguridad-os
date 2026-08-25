using Aprendizaje.Dominio.Evidence;
using Aprendizaje.Dominio.Evidence.Repositorios;
using Aprendizaje.Infraestructura.Persistencia.ModelosUnion;
using Microsoft.EntityFrameworkCore;

namespace Aprendizaje.Infraestructura.Persistencia.Repositorios;

public sealed class WriteupRepository : IWriteupRepository
{
    private readonly AprendizajeDbContext _context;

    public WriteupRepository(AprendizajeDbContext context)
    {
        _context = context;
    }

    public Task<Writeup?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.Writeups.FirstOrDefaultAsync(w => w.Id == id, cancellationToken);

    public async Task<IReadOnlyCollection<Writeup>> ListarPorUsuarioAsync(
        Guid usuarioId,
        CancellationToken cancellationToken = default) =>
        await _context.Writeups
            .AsNoTracking()
            .Where(w => w.UsuarioId == usuarioId)
            .OrderByDescending(w => w.Fecha)
            .ThenByDescending(w => w.FechaCreacionUtc)
            .ThenBy(w => w.Id)
            .ToListAsync(cancellationToken);

    public Task<bool> ExisteVinculoTemaAsync(
        Guid writeupId,
        Guid temaId,
        CancellationToken cancellationToken = default) =>
        _context.Set<WriteupTema>()
            .AnyAsync(w => w.WriteupId == writeupId && w.TemaId == temaId, cancellationToken);

    public void Agregar(Writeup writeup) => _context.Writeups.Add(writeup);

    public void VincularTema(Guid writeupId, Guid temaId) =>
        _context.Set<WriteupTema>().Add(new WriteupTema(writeupId, temaId));
}
