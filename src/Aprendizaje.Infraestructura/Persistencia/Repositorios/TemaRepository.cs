using Aprendizaje.Dominio.Roadmap;
using Aprendizaje.Dominio.Roadmap.Repositorios;
using Microsoft.EntityFrameworkCore;

namespace Aprendizaje.Infraestructura.Persistencia.Repositorios;

public sealed class TemaRepository : ITemaRepository
{
    private readonly AprendizajeDbContext _context;

    public TemaRepository(AprendizajeDbContext context)
    {
        _context = context;
    }

    public Task<Tema?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.Temas
            .Include(t => t.Criterios)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public async Task<IReadOnlyCollection<Tema>> ListarPorUsuarioAsync(
        Guid usuarioId,
        CancellationToken cancellationToken = default) =>
        await _context.Temas
            .AsNoTracking()
            .Where(t => t.UsuarioId == usuarioId)
            .OrderBy(t => t.Nombre)
            .ThenBy(t => t.Id)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyCollection<Tema>> ListarPorUsuarioParaImportacionAsync(
        Guid usuarioId,
        CancellationToken cancellationToken = default) =>
        await _context.Temas
            .Where(t => t.UsuarioId == usuarioId)
            .OrderBy(t => t.Nombre)
            .ThenBy(t => t.Id)
            .ToListAsync(cancellationToken);

    public void Agregar(Tema tema) => _context.Temas.Add(tema);
}
