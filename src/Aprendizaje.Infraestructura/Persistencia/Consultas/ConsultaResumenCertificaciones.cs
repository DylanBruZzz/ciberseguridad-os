using Aprendizaje.Aplicacion.Analytics.Certificaciones;
using Aprendizaje.Infraestructura.Persistencia.ModelosUnion;
using Microsoft.EntityFrameworkCore;

namespace Aprendizaje.Infraestructura.Persistencia.Consultas;

public sealed class ConsultaResumenCertificaciones : IConsultaResumenCertificaciones
{
    private readonly AprendizajeDbContext _context;

    public ConsultaResumenCertificaciones(AprendizajeDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyCollection<ResumenCertificacionDto>> ListarAsync(
        Guid usuarioId,
        CancellationToken cancellationToken = default)
    {
        var temasPorCertificacion =
            from vinculo in _context.Set<CertificacionTema>().AsNoTracking()
            join tema in _context.Temas.AsNoTracking() on vinculo.TemaId equals tema.Id
            where tema.UsuarioId == usuarioId
            group tema.Id by vinculo.CertificacionId into grupo
            select new
            {
                CertificacionId = grupo.Key,
                TemasVinculados = grupo.Distinct().Count()
            };

        return await (from certificacion in _context.Certificaciones.AsNoTracking()
                      join temas in temasPorCertificacion on certificacion.Id equals temas.CertificacionId
                      orderby certificacion.Nombre, certificacion.Id
                      select new ResumenCertificacionDto(
                          certificacion.Id,
                          certificacion.Nombre,
                          certificacion.Proveedor,
                          certificacion.TipoCosto,
                          temas.TemasVinculados,
                          _context.CertificacionesObtenidas
                              .AsNoTracking()
                              .Count(c =>
                                  c.UsuarioId == usuarioId &&
                                  c.CertificacionId == certificacion.Id)))
            .ToListAsync(cancellationToken);
    }
}
