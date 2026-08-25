using Aprendizaje.Dominio.Evidence;
using Aprendizaje.Dominio.Evidence.Repositorios;
using Microsoft.EntityFrameworkCore;

namespace Aprendizaje.Infraestructura.Persistencia.Repositorios;

public sealed class CertificacionObtenidaRepository : ICertificacionObtenidaRepository
{
    private readonly AprendizajeDbContext _context;

    public CertificacionObtenidaRepository(AprendizajeDbContext context)
    {
        _context = context;
    }

    public Task<CertificacionObtenida?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.CertificacionesObtenidas.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<IReadOnlyCollection<CertificacionObtenida>> ListarPorUsuarioAsync(
        Guid usuarioId,
        CancellationToken cancellationToken = default) =>
        await _context.CertificacionesObtenidas
            .AsNoTracking()
            .Where(c => c.UsuarioId == usuarioId)
            .OrderByDescending(c => c.FechaObtencion)
            .ThenBy(c => c.Id)
            .ToListAsync(cancellationToken);

    public void Agregar(CertificacionObtenida certificacionObtenida) =>
        _context.CertificacionesObtenidas.Add(certificacionObtenida);
}
