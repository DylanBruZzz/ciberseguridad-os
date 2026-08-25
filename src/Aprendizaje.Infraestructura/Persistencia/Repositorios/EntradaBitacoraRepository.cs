using Aprendizaje.Dominio.Study;
using Aprendizaje.Dominio.Study.Repositorios;
using Microsoft.EntityFrameworkCore;

namespace Aprendizaje.Infraestructura.Persistencia.Repositorios;

public sealed class EntradaBitacoraRepository : IEntradaBitacoraRepository
{
    private readonly AprendizajeDbContext _context;

    public EntradaBitacoraRepository(AprendizajeDbContext context)
    {
        _context = context;
    }

    public Task<EntradaBitacora?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.EntradasBitacora.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public async Task<IReadOnlyCollection<EntradaBitacora>> ListarPorUsuarioAsync(
        Guid usuarioId,
        CancellationToken cancellationToken = default) =>
        await _context.EntradasBitacora
            .AsNoTracking()
            .Where(e => e.UsuarioId == usuarioId)
            .OrderByDescending(e => e.Fecha)
            .ThenBy(e => e.Id)
            .ToListAsync(cancellationToken);

    public void Agregar(EntradaBitacora entrada) => _context.EntradasBitacora.Add(entrada);
}
