using Aprendizaje.Dominio.Study;
using Aprendizaje.Dominio.Study.Repositorios;
using Aprendizaje.Infraestructura.Persistencia.ModelosUnion;
using Microsoft.EntityFrameworkCore;

namespace Aprendizaje.Infraestructura.Persistencia.Repositorios;

public sealed class SesionEstudioRepository : ISesionEstudioRepository
{
    private readonly AprendizajeDbContext _context;

    public SesionEstudioRepository(AprendizajeDbContext context)
    {
        _context = context;
    }

    public Task<SesionEstudio?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.SesionesEstudio.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public async Task<IReadOnlyCollection<SesionEstudio>> ListarPorUsuarioAsync(
        Guid usuarioId,
        CancellationToken cancellationToken = default) =>
        await _context.SesionesEstudio
            .AsNoTracking()
            .Where(s => s.UsuarioId == usuarioId)
            .OrderByDescending(s => s.Fecha)
            .ThenBy(s => s.Id)
            .ToListAsync(cancellationToken);

    public Task<bool> ExisteVinculoHerramientaAsync(
        Guid sesionId,
        Guid herramientaId,
        CancellationToken cancellationToken = default) =>
        _context.Set<SesionHerramienta>()
            .AnyAsync(s => s.SesionId == sesionId && s.HerramientaId == herramientaId, cancellationToken);

    public void Agregar(SesionEstudio sesion) => _context.SesionesEstudio.Add(sesion);

    public void VincularHerramienta(Guid sesionId, Guid herramientaId) =>
        _context.Set<SesionHerramienta>().Add(new SesionHerramienta(sesionId, herramientaId));
}
