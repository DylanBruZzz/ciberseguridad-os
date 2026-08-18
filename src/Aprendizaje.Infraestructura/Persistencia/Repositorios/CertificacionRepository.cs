using Aprendizaje.Dominio.Roadmap;
using Aprendizaje.Dominio.Roadmap.Repositorios;
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

    public void Agregar(Certificacion certificacion) => _context.Certificaciones.Add(certificacion);
}
