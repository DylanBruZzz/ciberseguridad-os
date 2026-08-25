using Aprendizaje.Dominio.Evidence;
using Aprendizaje.Dominio.Evidence.Repositorios;
using Aprendizaje.Infraestructura.Persistencia.ModelosUnion;
using Microsoft.EntityFrameworkCore;

namespace Aprendizaje.Infraestructura.Persistencia.Repositorios;

public sealed class ArtefactoTecnicoRepository : IArtefactoTecnicoRepository
{
    private readonly AprendizajeDbContext _context;

    public ArtefactoTecnicoRepository(AprendizajeDbContext context)
    {
        _context = context;
    }

    public Task<ArtefactoTecnico?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.ArtefactosTecnicos.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public async Task<IReadOnlyCollection<ArtefactoTecnico>> ListarPorUsuarioAsync(
        Guid usuarioId,
        CancellationToken cancellationToken = default) =>
        await _context.ArtefactosTecnicos
            .AsNoTracking()
            .Where(a => a.UsuarioId == usuarioId)
            .OrderByDescending(a => a.FechaCreacionUtc)
            .ThenBy(a => a.Id)
            .ToListAsync(cancellationToken);

    public Task<bool> ExisteVinculoTemaAsync(
        Guid artefactoTecnicoId,
        Guid temaId,
        CancellationToken cancellationToken = default) =>
        _context.Set<ArtefactoTema>()
            .AnyAsync(a => a.ArtefactoTecnicoId == artefactoTecnicoId && a.TemaId == temaId, cancellationToken);

    public Task<bool> ExisteVinculoHerramientaAsync(
        Guid artefactoTecnicoId,
        Guid herramientaId,
        CancellationToken cancellationToken = default) =>
        _context.Set<ArtefactoHerramienta>()
            .AnyAsync(
                a => a.ArtefactoTecnicoId == artefactoTecnicoId && a.HerramientaId == herramientaId,
                cancellationToken);

    public void Agregar(ArtefactoTecnico artefactoTecnico) => _context.ArtefactosTecnicos.Add(artefactoTecnico);

    public void VincularTema(Guid artefactoTecnicoId, Guid temaId) =>
        _context.Set<ArtefactoTema>().Add(new ArtefactoTema(artefactoTecnicoId, temaId));

    public void VincularHerramienta(Guid artefactoTecnicoId, Guid herramientaId) =>
        _context.Set<ArtefactoHerramienta>().Add(new ArtefactoHerramienta(artefactoTecnicoId, herramientaId));
}
