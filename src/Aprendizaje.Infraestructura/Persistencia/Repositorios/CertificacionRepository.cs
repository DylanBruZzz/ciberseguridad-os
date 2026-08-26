using Aprendizaje.Dominio.Roadmap;
using Aprendizaje.Dominio.Roadmap.Repositorios;
using Aprendizaje.Infraestructura.Persistencia.ModelosUnion;
using Microsoft.EntityFrameworkCore;

namespace Aprendizaje.Infraestructura.Persistencia.Repositorios;

public sealed class CertificacionRepository : ICertificacionRepository
{
    private readonly AprendizajeDbContext _context;

    public CertificacionRepository(AprendizajeDbContext context)
    {
        _context = context;
    }

    public Task<Certificacion?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.Certificaciones.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<IReadOnlyCollection<Certificacion>> ListarAsync(CancellationToken cancellationToken = default) =>
        await _context.Certificaciones
            .AsNoTracking()
            .OrderBy(c => c.Nombre)
            .ThenBy(c => c.Id)
            .ToListAsync(cancellationToken);

    public Task<bool> ExisteVinculoTemaAsync(
        Guid certificacionId,
        Guid temaId,
        CancellationToken cancellationToken = default) =>
        _context.Set<CertificacionTema>()
            .AnyAsync(c => c.CertificacionId == certificacionId && c.TemaId == temaId, cancellationToken);

    public void Agregar(Certificacion certificacion) => _context.Certificaciones.Add(certificacion);

    public void VincularTema(Guid certificacionId, Guid temaId) =>
        _context.Set<CertificacionTema>().Add(new CertificacionTema(certificacionId, temaId));
}
