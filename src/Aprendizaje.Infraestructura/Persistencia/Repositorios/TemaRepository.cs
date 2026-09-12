using Aprendizaje.Dominio.Roadmap;
using Aprendizaje.Dominio.Roadmap.Repositorios;
using Aprendizaje.Infraestructura.Persistencia.ModelosUnion;
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

    public Task<bool> ExisteVinculoHerramientaAsync(
        Guid temaId,
        Guid herramientaId,
        CancellationToken cancellationToken = default) =>
        _context.Set<TemaHerramienta>()
            .AnyAsync(
                t => t.TemaId == temaId && t.HerramientaId == herramientaId,
                cancellationToken);

    public void Agregar(Tema tema) => _context.Temas.Add(tema);

    public void VincularHerramienta(Guid temaId, Guid herramientaId) =>
        _context.Set<TemaHerramienta>().Add(new TemaHerramienta(temaId, herramientaId));

    public async Task<bool> DesvincularHerramientaAsync(
        Guid temaId,
        Guid herramientaId,
        CancellationToken cancellationToken = default)
    {
        var vinculo = await _context.Set<TemaHerramienta>()
            .FirstOrDefaultAsync(
                t => t.TemaId == temaId && t.HerramientaId == herramientaId,
                cancellationToken);

        if (vinculo is null)
            return false;

        _context.Set<TemaHerramienta>().Remove(vinculo);
        return true;
    }
}
