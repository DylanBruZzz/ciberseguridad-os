using Aprendizaje.Aplicacion.Roadmap.Vistas;
using Aprendizaje.Aplicacion.Roadmap.Vistas.TemaWorkspaceV1;
using Aprendizaje.Dominio.Roadmap;
using Aprendizaje.Dominio.Roadmap.ValueObjects;
using Aprendizaje.Infraestructura.Persistencia.ModelosUnion;
using Microsoft.EntityFrameworkCore;

namespace Aprendizaje.Infraestructura.Persistencia.Consultas.Roadmap;

public sealed class ConsultaTemaWorkspaceV1 : IConsultaTemaWorkspaceV1
{
    private const string TipoProyecto = "Proyecto";
    private const string TipoLaboratorio = "Laboratorio";
    private const string TipoWriteup = "Writeup";
    private const string TipoArtefactoTecnico = "ArtefactoTecnico";
    private const string TipoCertificacionObtenida = "CertificacionObtenida";

    private readonly AprendizajeDbContext _context;

    public ConsultaTemaWorkspaceV1(AprendizajeDbContext context)
    {
        _context = context;
    }

    public async Task<TemaWorkspaceV1Dto?> ObtenerAsync(
        Guid usuarioId,
        Guid temaId,
        DateTime ahoraUtc,
        CancellationToken cancellationToken = default)
    {
        var intervaloDefectoDias = await _context.Usuarios
            .AsNoTracking()
            .Where(u => u.Id == usuarioId)
            .Select(u => (int?)u.IntervaloRepasoDefectoDias)
            .SingleOrDefaultAsync(cancellationToken);

        if (!intervaloDefectoDias.HasValue)
            return null;

        var tema = await _context.Temas
            .AsNoTracking()
            .Include(t => t.Criterios)
            .Where(t => t.Id == temaId && t.UsuarioId == usuarioId)
            .SingleOrDefaultAsync(cancellationToken);

        if (tema is null)
            return null;

        var fase = tema.FaseId.HasValue
            ? await _context.Fases
                .AsNoTracking()
                .Where(f => f.Id == tema.FaseId.Value && f.UsuarioId == usuarioId)
                .Select(f => new TemaWorkspaceFaseDto(f.Id, f.Orden, f.Nombre))
                .SingleOrDefaultAsync(cancellationToken)
            : null;

        var apunte = await _context.ApuntesTema
            .AsNoTracking()
            .Where(a => a.TemaId == temaId && a.UsuarioId == usuarioId)
            .Select(a => new TemaWorkspaceApuntesDto(
                a.Contenido,
                a.FechaModificacionUtc ?? a.FechaCreacionUtc))
            .SingleOrDefaultAsync(cancellationToken)
            ?? new TemaWorkspaceApuntesDto(string.Empty, null);

        var herramientas = await (
            from vinculacion in _context.Set<TemaHerramienta>().AsNoTracking()
            join herramienta in _context.Herramientas.AsNoTracking()
                on vinculacion.HerramientaId equals herramienta.Id
            where vinculacion.TemaId == temaId
            orderby herramienta.Nombre, herramienta.Id
            select new TemaWorkspaceHerramientaDto(herramienta.Id, herramienta.Nombre))
            .ToArrayAsync(cancellationToken);

        var certificacionesRelacionadas = await (
            from vinculacion in _context.Set<CertificacionTema>().AsNoTracking()
            join certificacion in _context.Certificaciones.AsNoTracking()
                on vinculacion.CertificacionId equals certificacion.Id
            where vinculacion.TemaId == temaId
            orderby certificacion.Nombre, certificacion.Id
            select new TemaWorkspaceCertificacionDto(
                certificacion.Id,
                certificacion.Nombre,
                certificacion.Proveedor,
                certificacion.TipoCosto))
            .ToArrayAsync(cancellationToken);

        var ultimaSesion = await _context.SesionesEstudio
            .AsNoTracking()
            .Where(s => s.TemaId == temaId && s.UsuarioId == usuarioId)
            .OrderByDescending(s => s.Fecha)
            .ThenByDescending(s => s.FechaCreacionUtc)
            .ThenByDescending(s => s.Id)
            .Select(s => new TemaWorkspaceSesionDto(s.Id, s.Fecha, s.DuracionMinutos, s.Tipo))
            .FirstOrDefaultAsync(cancellationToken);

        var sesionesResumen = await _context.SesionesEstudio
            .AsNoTracking()
            .Where(s => s.TemaId == temaId && s.UsuarioId == usuarioId)
            .GroupBy(_ => 1)
            .Select(g => new TemaWorkspaceSesionesResumenDto(
                g.Count(),
                g.Sum(s => s.DuracionMinutos)))
            .SingleOrDefaultAsync(cancellationToken)
            ?? new TemaWorkspaceSesionesResumenDto(0, 0);

        var resourcesTotal = await (
            from recurso in _context.Recursos.AsNoTracking()
            join vinculacion in _context.Set<RecursoTema>().AsNoTracking()
                on recurso.Id equals vinculacion.RecursoId
            where recurso.UsuarioId == usuarioId && vinculacion.TemaId == temaId
            select recurso.Id)
            .CountAsync(cancellationToken);

        var evidenceResumen = await CrearEvidenceResumenAsync(usuarioId, temaId, cancellationToken);
        var intervaloDefecto = IntervaloRepaso.Crear(intervaloDefectoDias.Value);
        var semantica = SemanticaTemaReadSide.Calcular(
            tema,
            ultimaSesion?.Fecha,
            intervaloDefecto,
            ahoraUtc);

        var criterios = tema.Criterios
            .OrderBy(c => c.Tipo)
            .ThenBy(c => c.Id)
            .Select(c => new TemaWorkspaceCriterioDto(c.Id, c.Tipo, c.Cumplido, c.FechaCumplido))
            .ToArray();

        var temaDto = new TemaWorkspaceTemaDto(
            tema.Id,
            tema.TemaPadreId,
            tema.Nombre,
            tema.Descripcion,
            tema.TipoConocimiento,
            semantica.Estado,
            tema.DificultadPercibida?.Valor,
            tema.Confianza?.Valor,
            semantica.IntervaloRepasoDias,
            semantica.CriteriosTotal,
            semantica.CriteriosCumplidos,
            semantica.ProgresoPorcentaje,
            tema.Objetivos.ToArray(),
            criterios);

        return new TemaWorkspaceV1Dto(
            temaDto,
            fase,
            apunte,
            herramientas,
            certificacionesRelacionadas,
            ultimaSesion,
            new TemaWorkspaceRepasoDto(semantica.ProximaFechaRepaso, semantica.RepasoRecomendado),
            new TemaWorkspaceResourcesResumenDto(resourcesTotal),
            sesionesResumen,
            evidenceResumen);
    }

    private async Task<TemaWorkspaceEvidenceResumenDto> CrearEvidenceResumenAsync(
        Guid usuarioId,
        Guid temaId,
        CancellationToken cancellationToken)
    {
        var proyectos =
            from proyecto in _context.Proyectos.AsNoTracking()
            join vinculacion in _context.Set<ProyectoTema>().AsNoTracking()
                on proyecto.Id equals vinculacion.ProyectoId
            where proyecto.UsuarioId == usuarioId && vinculacion.TemaId == temaId
            select TipoProyecto;

        var laboratorios =
            from laboratorio in _context.Laboratorios.AsNoTracking()
            join vinculacion in _context.Set<LaboratorioTema>().AsNoTracking()
                on laboratorio.Id equals vinculacion.LaboratorioId
            where laboratorio.UsuarioId == usuarioId && vinculacion.TemaId == temaId
            select TipoLaboratorio;

        var writeups =
            from writeup in _context.Writeups.AsNoTracking()
            join vinculacion in _context.Set<WriteupTema>().AsNoTracking()
                on writeup.Id equals vinculacion.WriteupId
            where writeup.UsuarioId == usuarioId && vinculacion.TemaId == temaId
            select TipoWriteup;

        var artefactos =
            from artefacto in _context.ArtefactosTecnicos.AsNoTracking()
            join vinculacion in _context.Set<ArtefactoTema>().AsNoTracking()
                on artefacto.Id equals vinculacion.ArtefactoTecnicoId
            where artefacto.UsuarioId == usuarioId && vinculacion.TemaId == temaId
            select TipoArtefactoTecnico;

        var certificaciones =
            from certificacion in _context.CertificacionesObtenidas.AsNoTracking()
            join vinculacion in _context.Set<CertificacionTema>().AsNoTracking()
                on certificacion.CertificacionId equals vinculacion.CertificacionId
            where certificacion.UsuarioId == usuarioId && vinculacion.TemaId == temaId
            select TipoCertificacionObtenida;

        var conteos = await proyectos
            .Concat(laboratorios)
            .Concat(writeups)
            .Concat(artefactos)
            .Concat(certificaciones)
            .GroupBy(tipo => tipo)
            .Select(g => new EvidenceConteo(g.Key, g.Count()))
            .ToDictionaryAsync(c => c.Tipo, c => c.Total, cancellationToken);

        var totalProyectos = ObtenerConteo(conteos, TipoProyecto);
        var totalLaboratorios = ObtenerConteo(conteos, TipoLaboratorio);
        var totalWriteups = ObtenerConteo(conteos, TipoWriteup);
        var totalArtefactos = ObtenerConteo(conteos, TipoArtefactoTecnico);
        var totalCertificaciones = ObtenerConteo(conteos, TipoCertificacionObtenida);

        return new TemaWorkspaceEvidenceResumenDto(
            totalProyectos + totalLaboratorios + totalWriteups + totalArtefactos + totalCertificaciones,
            totalProyectos,
            totalLaboratorios,
            totalWriteups,
            totalArtefactos,
            totalCertificaciones);
    }

    private static int ObtenerConteo(IReadOnlyDictionary<string, int> conteos, string tipo) =>
        conteos.TryGetValue(tipo, out var total) ? total : 0;

    private sealed record EvidenceConteo(string Tipo, int Total);
}
