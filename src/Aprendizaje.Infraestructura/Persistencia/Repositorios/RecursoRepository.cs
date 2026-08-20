using Aprendizaje.Dominio.Resource;
using Aprendizaje.Dominio.Resource.Repositorios;
using Aprendizaje.Infraestructura.Persistencia.ModelosUnion;
using Microsoft.EntityFrameworkCore;

namespace Aprendizaje.Infraestructura.Persistencia.Repositorios;

public sealed class RecursoRepository : IRecursoRepository
{
    private readonly AprendizajeDbContext _context;

    public RecursoRepository(AprendizajeDbContext context)
    {
        _context = context;
    }

    public Task<Recurso?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.Recursos.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public async Task<IReadOnlyCollection<Recurso>> ListarPorUsuarioAsync(
        Guid usuarioId,
        CancellationToken cancellationToken = default) =>
        await _context.Recursos
            .AsNoTracking()
            .Where(r => r.UsuarioId == usuarioId)
            .OrderBy(r => r.Titulo)
            .ThenBy(r => r.Id)
            .ToListAsync(cancellationToken);

    public Task<bool> ExisteVinculoTemaAsync(
        Guid recursoId,
        Guid temaId,
        CancellationToken cancellationToken = default) =>
        _context.Set<RecursoTema>()
            .AnyAsync(r => r.RecursoId == recursoId && r.TemaId == temaId, cancellationToken);

    public void Agregar(Recurso recurso) => _context.Recursos.Add(recurso);

    public void VincularTema(Guid recursoId, Guid temaId) =>
        _context.Set<RecursoTema>().Add(new RecursoTema(recursoId, temaId));
}
