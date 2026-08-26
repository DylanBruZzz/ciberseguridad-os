using Aprendizaje.Aplicacion.Analytics.Temas;
using Aprendizaje.Dominio.Roadmap;
using Aprendizaje.Infraestructura.Persistencia.ModelosUnion;
using Microsoft.EntityFrameworkCore;

namespace Aprendizaje.Infraestructura.Persistencia.Consultas;

public sealed class ConsultaResumenTema : IConsultaResumenTema
{
    private readonly AprendizajeDbContext _context;

    public ConsultaResumenTema(AprendizajeDbContext context)
    {
        _context = context;
    }

    public async Task<ResumenTemaDto?> ObtenerAsync(
        Guid usuarioId,
        Guid temaId,
        CancellationToken cancellationToken = default)
    {
        var tema = await _context.Temas
            .AsNoTracking()
            .Where(t => t.Id == temaId)
            .Select(t => new
            {
                t.Id,
                t.UsuarioId,
                t.Nombre,
                DificultadPercibida = t.DificultadPercibida == null ? null : (int?)t.DificultadPercibida.Valor,
                Confianza = t.Confianza == null ? null : (int?)t.Confianza.Valor,
                t.FechaInicio,
                t.FechaFin,
                IntervaloRepasoDias = t.IntervaloRepaso == null ? null : (int?)t.IntervaloRepaso.Dias
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (tema is null || tema.UsuarioId != usuarioId)
            return null;

        var estudio = await _context.SesionesEstudio
            .AsNoTracking()
            .Where(s => s.UsuarioId == usuarioId && s.TemaId == temaId)
            .GroupBy(_ => 1)
            .Select(grupo => new
            {
                SesionesTotales = grupo.Count(),
                MinutosTotales = grupo.Sum(s => s.DuracionMinutos),
                UltimaSesion = grupo.Max(s => s.Fecha)
            })
            .SingleOrDefaultAsync(cancellationToken);

        var criterios = await _context.Set<CriterioTema>()
            .AsNoTracking()
            .Where(c => c.TemaId == temaId)
            .GroupBy(_ => 1)
            .Select(grupo => new
            {
                CriteriosTotales = grupo.Count(),
                CriteriosCumplidos = grupo.Count(c => c.Cumplido)
            })
            .SingleOrDefaultAsync(cancellationToken);

        var recursos = await ContarRecursosAsync(usuarioId, temaId, cancellationToken);
        var laboratorios = await ContarLaboratoriosAsync(usuarioId, temaId, cancellationToken);
        var proyectos = await ContarProyectosAsync(usuarioId, temaId, cancellationToken);
        var artefactos = await ContarArtefactosAsync(usuarioId, temaId, cancellationToken);
        var writeups = await ContarWriteupsAsync(usuarioId, temaId, cancellationToken);
        var notas = await _context.Notas
            .AsNoTracking()
            .CountAsync(n => n.UsuarioId == usuarioId && n.TemaId == temaId, cancellationToken);

        return new ResumenTemaDto(
            tema.Id,
            tema.Nombre,
            tema.DificultadPercibida,
            tema.Confianza,
            tema.FechaInicio,
            tema.FechaFin,
            tema.IntervaloRepasoDias,
            estudio?.SesionesTotales ?? 0,
            estudio?.MinutosTotales ?? 0,
            estudio?.UltimaSesion,
            criterios?.CriteriosTotales ?? 0,
            criterios?.CriteriosCumplidos ?? 0,
            recursos,
            laboratorios,
            proyectos,
            artefactos,
            writeups,
            notas);
    }

    private async Task<int> ContarRecursosAsync(Guid usuarioId, Guid temaId, CancellationToken cancellationToken) =>
        await (from recurso in _context.Recursos.AsNoTracking()
               join vinculo in _context.Set<RecursoTema>().AsNoTracking() on recurso.Id equals vinculo.RecursoId
               where recurso.UsuarioId == usuarioId && vinculo.TemaId == temaId
               select recurso.Id)
            .Distinct()
            .CountAsync(cancellationToken);

    private async Task<int> ContarLaboratoriosAsync(Guid usuarioId, Guid temaId, CancellationToken cancellationToken) =>
        await (from laboratorio in _context.Laboratorios.AsNoTracking()
               join vinculo in _context.Set<LaboratorioTema>().AsNoTracking() on laboratorio.Id equals vinculo.LaboratorioId
               where laboratorio.UsuarioId == usuarioId && vinculo.TemaId == temaId
               select laboratorio.Id)
            .Distinct()
            .CountAsync(cancellationToken);

    private async Task<int> ContarProyectosAsync(Guid usuarioId, Guid temaId, CancellationToken cancellationToken) =>
        await (from proyecto in _context.Proyectos.AsNoTracking()
               join vinculo in _context.Set<ProyectoTema>().AsNoTracking() on proyecto.Id equals vinculo.ProyectoId
               where proyecto.UsuarioId == usuarioId && vinculo.TemaId == temaId
               select proyecto.Id)
            .Distinct()
            .CountAsync(cancellationToken);

    private async Task<int> ContarArtefactosAsync(Guid usuarioId, Guid temaId, CancellationToken cancellationToken) =>
        await (from artefacto in _context.ArtefactosTecnicos.AsNoTracking()
               join vinculo in _context.Set<ArtefactoTema>().AsNoTracking() on artefacto.Id equals vinculo.ArtefactoTecnicoId
               where artefacto.UsuarioId == usuarioId && vinculo.TemaId == temaId
               select artefacto.Id)
            .Distinct()
            .CountAsync(cancellationToken);

    private async Task<int> ContarWriteupsAsync(Guid usuarioId, Guid temaId, CancellationToken cancellationToken) =>
        await (from writeup in _context.Writeups.AsNoTracking()
               join vinculo in _context.Set<WriteupTema>().AsNoTracking() on writeup.Id equals vinculo.WriteupId
               where writeup.UsuarioId == usuarioId && vinculo.TemaId == temaId
               select writeup.Id)
            .Distinct()
            .CountAsync(cancellationToken);
}
