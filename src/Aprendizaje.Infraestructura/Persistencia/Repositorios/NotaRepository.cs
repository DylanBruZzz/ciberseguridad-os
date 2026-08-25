using Aprendizaje.Dominio.Evidence;
using Aprendizaje.Dominio.Evidence.Repositorios;
using Microsoft.EntityFrameworkCore;

namespace Aprendizaje.Infraestructura.Persistencia.Repositorios;

public sealed class NotaRepository : INotaRepository
{
    private readonly AprendizajeDbContext _context;

    public NotaRepository(AprendizajeDbContext context)
    {
        _context = context;
    }

    public Task<Nota?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.Notas.FirstOrDefaultAsync(n => n.Id == id, cancellationToken);

    public async Task<IReadOnlyCollection<Nota>> ListarPorUsuarioAsync(
        Guid usuarioId,
        CancellationToken cancellationToken = default) =>
        await _context.Notas
            .AsNoTracking()
            .Where(n => n.UsuarioId == usuarioId)
            .OrderByDescending(n => n.Fecha)
            .ThenBy(n => n.Id)
            .ToListAsync(cancellationToken);

    public void Agregar(Nota nota) => _context.Notas.Add(nota);
}
