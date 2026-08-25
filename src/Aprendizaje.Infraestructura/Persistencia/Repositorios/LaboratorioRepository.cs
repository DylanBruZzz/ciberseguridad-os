using Aprendizaje.Dominio.Evidence;
using Aprendizaje.Dominio.Evidence.Repositorios;
using Aprendizaje.Infraestructura.Persistencia.ModelosUnion;
using Microsoft.EntityFrameworkCore;

namespace Aprendizaje.Infraestructura.Persistencia.Repositorios;

public sealed class LaboratorioRepository : ILaboratorioRepository
{
    private readonly AprendizajeDbContext _context;

    public LaboratorioRepository(AprendizajeDbContext context)
    {
        _context = context;
    }

    public Task<Laboratorio?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.Laboratorios.FirstOrDefaultAsync(l => l.Id == id, cancellationToken);

    public async Task<IReadOnlyCollection<Laboratorio>> ListarPorUsuarioAsync(
        Guid usuarioId,
        CancellationToken cancellationToken = default) =>
        await _context.Laboratorios
            .AsNoTracking()
            .Where(l => l.UsuarioId == usuarioId)
            .OrderByDescending(l => l.Fecha)
            .ThenBy(l => l.Id)
            .ToListAsync(cancellationToken);

    public Task<bool> ExisteVinculoTemaAsync(
        Guid laboratorioId,
        Guid temaId,
        CancellationToken cancellationToken = default) =>
        _context.Set<LaboratorioTema>()
            .AnyAsync(l => l.LaboratorioId == laboratorioId && l.TemaId == temaId, cancellationToken);

    public Task<bool> ExisteVinculoHerramientaAsync(
        Guid laboratorioId,
        Guid herramientaId,
        CancellationToken cancellationToken = default) =>
        _context.Set<LaboratorioHerramienta>()
            .AnyAsync(l => l.LaboratorioId == laboratorioId && l.HerramientaId == herramientaId, cancellationToken);

    public void Agregar(Laboratorio laboratorio) => _context.Laboratorios.Add(laboratorio);

    public void VincularTema(Guid laboratorioId, Guid temaId) =>
        _context.Set<LaboratorioTema>().Add(new LaboratorioTema(laboratorioId, temaId));

    public void VincularHerramienta(Guid laboratorioId, Guid herramientaId) =>
        _context.Set<LaboratorioHerramienta>().Add(new LaboratorioHerramienta(laboratorioId, herramientaId));
}
