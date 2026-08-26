using Aprendizaje.Aplicacion.Analytics.Estudio;
using Microsoft.EntityFrameworkCore;

namespace Aprendizaje.Infraestructura.Persistencia.Consultas;

public sealed class ConsultaResumenEstudio : IConsultaResumenEstudio
{
    private readonly AprendizajeDbContext _context;

    public ConsultaResumenEstudio(AprendizajeDbContext context)
    {
        _context = context;
    }

    public async Task<ResumenEstudioDto> ObtenerAsync(
        Guid usuarioId,
        CancellationToken cancellationToken = default)
    {
        var sesionesUsuario = _context.SesionesEstudio
            .AsNoTracking()
            .Where(s => s.UsuarioId == usuarioId);

        var agregado = await sesionesUsuario
            .GroupBy(_ => 1)
            .Select(grupo => new
            {
                SesionesTotales = grupo.Count(),
                MinutosTotales = grupo.Sum(s => s.DuracionMinutos),
                TemasEstudiados = grupo.Select(s => s.TemaId).Distinct().Count(),
                UltimaSesion = grupo.Max(s => s.Fecha)
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (agregado is null)
            return new ResumenEstudioDto(usuarioId, 0, 0, 0, null, []);

        var sesionesPorTipo = await sesionesUsuario
            .GroupBy(s => s.Tipo)
            .Select(grupo => new SesionPorTipoDto(
                grupo.Key,
                grupo.Count(),
                grupo.Sum(s => s.DuracionMinutos)))
            .ToListAsync(cancellationToken);

        return new ResumenEstudioDto(
            usuarioId,
            agregado.SesionesTotales,
            agregado.MinutosTotales,
            agregado.TemasEstudiados,
            agregado.UltimaSesion,
            sesionesPorTipo.OrderBy(s => s.Tipo).ToArray());
    }
}
