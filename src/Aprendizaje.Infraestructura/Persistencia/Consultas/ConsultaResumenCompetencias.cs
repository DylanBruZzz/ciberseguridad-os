using Aprendizaje.Aplicacion.Analytics.Competencias;
using Aprendizaje.Infraestructura.Persistencia.ModelosUnion;
using Microsoft.EntityFrameworkCore;

namespace Aprendizaje.Infraestructura.Persistencia.Consultas;

public sealed class ConsultaResumenCompetencias : IConsultaResumenCompetencias
{
    private readonly AprendizajeDbContext _context;

    public ConsultaResumenCompetencias(AprendizajeDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyCollection<ResumenCompetenciaDto>> ListarAsync(
        Guid usuarioId,
        CancellationToken cancellationToken = default) =>
        await _context.Competencias
            .AsNoTracking()
            .Where(c => c.UsuarioId == usuarioId)
            .OrderBy(c => c.Nombre)
            .ThenBy(c => c.Id)
            .Select(c => new ResumenCompetenciaDto(
                c.Id,
                c.Nombre,
                (from vinculo in _context.Set<CompetenciaTema>().AsNoTracking()
                 join tema in _context.Temas.AsNoTracking() on vinculo.TemaId equals tema.Id
                 where vinculo.CompetenciaId == c.Id && tema.UsuarioId == usuarioId
                 select tema.Id)
                .Distinct()
                .Count()))
            .ToListAsync(cancellationToken);
}
