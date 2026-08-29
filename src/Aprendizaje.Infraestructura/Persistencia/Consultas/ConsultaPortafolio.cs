using Aprendizaje.Aplicacion.Portafolio;
using Aprendizaje.Dominio.Evidence;
using Aprendizaje.Infraestructura.Persistencia.ModelosUnion;
using Microsoft.EntityFrameworkCore;

namespace Aprendizaje.Infraestructura.Persistencia.Consultas;

public sealed class ConsultaPortafolio : IConsultaPortafolio
{
    private readonly AprendizajeDbContext _context;

    public ConsultaPortafolio(AprendizajeDbContext context)
    {
        _context = context;
    }

    public async Task<PortafolioDto> ObtenerAsync(
        ObtenerPortafolioSolicitud solicitud,
        CancellationToken cancellationToken = default)
    {
        var proyectos = DebeIncluirTipo(solicitud.TipoEvidence, TipoEvidencePortafolio.Proyecto)
            ? await ObtenerProyectosAsync(solicitud, cancellationToken)
            : [];
        var laboratorios = DebeIncluirTipo(solicitud.TipoEvidence, TipoEvidencePortafolio.Laboratorio)
            ? await ObtenerLaboratoriosAsync(solicitud, cancellationToken)
            : [];
        var writeups = DebeIncluirTipo(solicitud.TipoEvidence, TipoEvidencePortafolio.Writeup)
            ? await ObtenerWriteupsAsync(solicitud, cancellationToken)
            : [];
        var artefactos = DebeIncluirTipo(solicitud.TipoEvidence, TipoEvidencePortafolio.ArtefactoTecnico)
            ? await ObtenerArtefactosAsync(solicitud, cancellationToken)
            : [];
        var certificaciones = DebeIncluirTipo(solicitud.TipoEvidence, TipoEvidencePortafolio.CertificacionObtenida)
            ? await ObtenerCertificacionesObtenidasAsync(solicitud, cancellationToken)
            : [];

        var resumen = CrearResumen(proyectos, laboratorios, writeups, artefactos, certificaciones);

        return new PortafolioDto(
            solicitud.UsuarioId,
            resumen,
            proyectos,
            laboratorios,
            writeups,
            artefactos,
            certificaciones);
    }

    private async Task<IReadOnlyCollection<PortafolioProyectoDto>> ObtenerProyectosAsync(
        ObtenerPortafolioSolicitud solicitud,
        CancellationToken cancellationToken)
    {
        var proyectos = await _context.Proyectos
            .AsNoTracking()
            .Where(p => p.UsuarioId == solicitud.UsuarioId
                && (p.EstadoMadurez == EstadoMadurez.ListoPortafolio || p.EstadoMadurez == EstadoMadurez.Publicado)
                && (!solicitud.EstadoMadurez.HasValue || p.EstadoMadurez == solicitud.EstadoMadurez.Value))
            .OrderByDescending(p => p.FechaFin ?? p.FechaInicio)
            .ThenBy(p => p.Nombre)
            .ThenBy(p => p.Id)
            .Select(p => new ProyectoPlano(
                p.Id,
                p.Nombre,
                p.Descripcion,
                p.Estado,
                p.EstadoMadurez,
                p.RepositorioUrl,
                p.FechaInicio,
                p.FechaFin))
            .ToListAsync(cancellationToken);

        var ids = proyectos.Select(p => p.ProyectoId).ToArray();
        var temas = await ObtenerTemasPorProyectoAsync(solicitud.UsuarioId, ids, cancellationToken);
        var herramientas = await ObtenerHerramientasPorProyectoAsync(ids, cancellationToken);

        return proyectos
            .Select(p => new PortafolioProyectoDto(
                p.ProyectoId,
                p.Nombre,
                p.Descripcion,
                p.Estado,
                p.EstadoMadurez,
                p.RepositorioUrl,
                p.FechaInicio,
                p.FechaFin,
                ObtenerColeccion(temas, p.ProyectoId),
                ObtenerColeccion(herramientas, p.ProyectoId)))
            .ToArray();
    }

    private async Task<IReadOnlyCollection<PortafolioLaboratorioDto>> ObtenerLaboratoriosAsync(
        ObtenerPortafolioSolicitud solicitud,
        CancellationToken cancellationToken)
    {
        var laboratorios = await _context.Laboratorios
            .AsNoTracking()
            .Where(l => l.UsuarioId == solicitud.UsuarioId
                && (l.EstadoMadurez == EstadoMadurez.ListoPortafolio || l.EstadoMadurez == EstadoMadurez.Publicado)
                && (!solicitud.EstadoMadurez.HasValue || l.EstadoMadurez == solicitud.EstadoMadurez.Value))
            .OrderByDescending(l => l.Fecha)
            .ThenBy(l => l.Nombre)
            .ThenBy(l => l.Id)
            .Select(l => new LaboratorioPlano(
                l.Id,
                l.Nombre,
                l.Objetivo,
                l.EntornoVms,
                l.Hallazgos,
                l.TiempoInvertidoMinutos,
                l.Fecha,
                l.EstadoMadurez))
            .ToListAsync(cancellationToken);

        var ids = laboratorios.Select(l => l.LaboratorioId).ToArray();
        var temas = await ObtenerTemasPorLaboratorioAsync(solicitud.UsuarioId, ids, cancellationToken);
        var herramientas = await ObtenerHerramientasPorLaboratorioAsync(ids, cancellationToken);

        return laboratorios
            .Select(l => new PortafolioLaboratorioDto(
                l.LaboratorioId,
                l.Nombre,
                l.Objetivo,
                l.EntornoVms,
                l.Hallazgos,
                l.TiempoInvertidoMinutos,
                l.Fecha,
                l.EstadoMadurez,
                ObtenerColeccion(temas, l.LaboratorioId),
                ObtenerColeccion(herramientas, l.LaboratorioId)))
            .ToArray();
    }

    private async Task<IReadOnlyCollection<PortafolioWriteupDto>> ObtenerWriteupsAsync(
        ObtenerPortafolioSolicitud solicitud,
        CancellationToken cancellationToken)
    {
        var writeups = await _context.Writeups
            .AsNoTracking()
            .Where(w => w.UsuarioId == solicitud.UsuarioId
                && (w.EstadoMadurez == EstadoMadurez.ListoPortafolio || w.EstadoMadurez == EstadoMadurez.Publicado)
                && (!solicitud.EstadoMadurez.HasValue || w.EstadoMadurez == solicitud.EstadoMadurez.Value))
            .OrderByDescending(w => w.Fecha)
            .ThenBy(w => w.Titulo)
            .ThenBy(w => w.Id)
            .Select(w => new WriteupPlano(
                w.Id,
                w.Titulo,
                w.PlataformaOrigen,
                w.Url,
                w.Fecha,
                w.EstadoMadurez))
            .ToListAsync(cancellationToken);

        var ids = writeups.Select(w => w.WriteupId).ToArray();
        var temas = await ObtenerTemasPorWriteupAsync(solicitud.UsuarioId, ids, cancellationToken);

        return writeups
            .Select(w => new PortafolioWriteupDto(
                w.WriteupId,
                w.Titulo,
                w.PlataformaOrigen,
                w.Url,
                w.Fecha,
                w.EstadoMadurez,
                ObtenerColeccion(temas, w.WriteupId)))
            .ToArray();
    }

    private async Task<IReadOnlyCollection<PortafolioArtefactoTecnicoDto>> ObtenerArtefactosAsync(
        ObtenerPortafolioSolicitud solicitud,
        CancellationToken cancellationToken)
    {
        var artefactos = await _context.ArtefactosTecnicos
            .AsNoTracking()
            .Where(a => a.UsuarioId == solicitud.UsuarioId
                && (a.EstadoMadurez == EstadoMadurez.ListoPortafolio || a.EstadoMadurez == EstadoMadurez.Publicado)
                && (!solicitud.EstadoMadurez.HasValue || a.EstadoMadurez == solicitud.EstadoMadurez.Value))
            .OrderBy(a => a.Nombre)
            .ThenBy(a => a.Id)
            .Select(a => new ArtefactoPlano(
                a.Id,
                a.TipoArtefacto,
                a.Nombre,
                a.ContenidoOUrl,
                a.LenguajeTecnologia,
                a.EstadoMadurez))
            .ToListAsync(cancellationToken);

        var ids = artefactos.Select(a => a.ArtefactoTecnicoId).ToArray();
        var temas = await ObtenerTemasPorArtefactoAsync(solicitud.UsuarioId, ids, cancellationToken);
        var herramientas = await ObtenerHerramientasPorArtefactoAsync(ids, cancellationToken);

        return artefactos
            .Select(a => new PortafolioArtefactoTecnicoDto(
                a.ArtefactoTecnicoId,
                a.TipoArtefacto,
                a.Nombre,
                a.ContenidoOUrl,
                a.LenguajeTecnologia,
                a.EstadoMadurez,
                ObtenerColeccion(temas, a.ArtefactoTecnicoId),
                ObtenerColeccion(herramientas, a.ArtefactoTecnicoId)))
            .ToArray();
    }

    private async Task<IReadOnlyCollection<PortafolioCertificacionObtenidaDto>> ObtenerCertificacionesObtenidasAsync(
        ObtenerPortafolioSolicitud solicitud,
        CancellationToken cancellationToken)
    {
        return await (from obtenida in _context.CertificacionesObtenidas.AsNoTracking()
                      join certificacion in _context.Certificaciones.AsNoTracking()
                          on obtenida.CertificacionId equals certificacion.Id
                      where obtenida.UsuarioId == solicitud.UsuarioId
                          && (obtenida.EstadoMadurez == EstadoMadurez.ListoPortafolio
                              || obtenida.EstadoMadurez == EstadoMadurez.Publicado)
                          && (!solicitud.EstadoMadurez.HasValue || obtenida.EstadoMadurez == solicitud.EstadoMadurez.Value)
                      orderby obtenida.FechaObtencion descending, certificacion.Nombre, obtenida.Id
                      select new PortafolioCertificacionObtenidaDto(
                          obtenida.Id,
                          certificacion.Id,
                          certificacion.Nombre,
                          certificacion.Proveedor,
                          certificacion.TipoCosto,
                          obtenida.FechaObtencion,
                          obtenida.EvidenciaUrl,
                          obtenida.EstadoMadurez))
            .ToListAsync(cancellationToken);
    }

    private async Task<IReadOnlyDictionary<Guid, IReadOnlyCollection<PortafolioTemaDto>>> ObtenerTemasPorProyectoAsync(
        Guid usuarioId,
        Guid[] proyectoIds,
        CancellationToken cancellationToken)
    {
        if (proyectoIds.Length == 0)
            return new Dictionary<Guid, IReadOnlyCollection<PortafolioTemaDto>>();

        var datos = await (from vinculo in _context.Set<ProyectoTema>().AsNoTracking()
                           join tema in _context.Temas.AsNoTracking() on vinculo.TemaId equals tema.Id
                           where proyectoIds.Contains(vinculo.ProyectoId) && tema.UsuarioId == usuarioId
                           orderby tema.Nombre, tema.Id
                           select new { vinculo.ProyectoId, Tema = new PortafolioTemaDto(tema.Id, tema.Nombre) })
            .ToListAsync(cancellationToken);

        return Agrupar(datos, d => d.ProyectoId, d => d.Tema);
    }

    private async Task<IReadOnlyDictionary<Guid, IReadOnlyCollection<PortafolioTemaDto>>> ObtenerTemasPorLaboratorioAsync(
        Guid usuarioId,
        Guid[] laboratorioIds,
        CancellationToken cancellationToken)
    {
        if (laboratorioIds.Length == 0)
            return new Dictionary<Guid, IReadOnlyCollection<PortafolioTemaDto>>();

        var datos = await (from vinculo in _context.Set<LaboratorioTema>().AsNoTracking()
                           join tema in _context.Temas.AsNoTracking() on vinculo.TemaId equals tema.Id
                           where laboratorioIds.Contains(vinculo.LaboratorioId) && tema.UsuarioId == usuarioId
                           orderby tema.Nombre, tema.Id
                           select new { vinculo.LaboratorioId, Tema = new PortafolioTemaDto(tema.Id, tema.Nombre) })
            .ToListAsync(cancellationToken);

        return Agrupar(datos, d => d.LaboratorioId, d => d.Tema);
    }

    private async Task<IReadOnlyDictionary<Guid, IReadOnlyCollection<PortafolioTemaDto>>> ObtenerTemasPorWriteupAsync(
        Guid usuarioId,
        Guid[] writeupIds,
        CancellationToken cancellationToken)
    {
        if (writeupIds.Length == 0)
            return new Dictionary<Guid, IReadOnlyCollection<PortafolioTemaDto>>();

        var datos = await (from vinculo in _context.Set<WriteupTema>().AsNoTracking()
                           join tema in _context.Temas.AsNoTracking() on vinculo.TemaId equals tema.Id
                           where writeupIds.Contains(vinculo.WriteupId) && tema.UsuarioId == usuarioId
                           orderby tema.Nombre, tema.Id
                           select new { vinculo.WriteupId, Tema = new PortafolioTemaDto(tema.Id, tema.Nombre) })
            .ToListAsync(cancellationToken);

        return Agrupar(datos, d => d.WriteupId, d => d.Tema);
    }

    private async Task<IReadOnlyDictionary<Guid, IReadOnlyCollection<PortafolioTemaDto>>> ObtenerTemasPorArtefactoAsync(
        Guid usuarioId,
        Guid[] artefactoIds,
        CancellationToken cancellationToken)
    {
        if (artefactoIds.Length == 0)
            return new Dictionary<Guid, IReadOnlyCollection<PortafolioTemaDto>>();

        var datos = await (from vinculo in _context.Set<ArtefactoTema>().AsNoTracking()
                           join tema in _context.Temas.AsNoTracking() on vinculo.TemaId equals tema.Id
                           where artefactoIds.Contains(vinculo.ArtefactoTecnicoId) && tema.UsuarioId == usuarioId
                           orderby tema.Nombre, tema.Id
                           select new { vinculo.ArtefactoTecnicoId, Tema = new PortafolioTemaDto(tema.Id, tema.Nombre) })
            .ToListAsync(cancellationToken);

        return Agrupar(datos, d => d.ArtefactoTecnicoId, d => d.Tema);
    }

    private async Task<IReadOnlyDictionary<Guid, IReadOnlyCollection<PortafolioHerramientaDto>>> ObtenerHerramientasPorProyectoAsync(
        Guid[] proyectoIds,
        CancellationToken cancellationToken)
    {
        if (proyectoIds.Length == 0)
            return new Dictionary<Guid, IReadOnlyCollection<PortafolioHerramientaDto>>();

        var datos = await (from vinculo in _context.Set<ProyectoHerramienta>().AsNoTracking()
                           join herramienta in _context.Herramientas.AsNoTracking()
                               on vinculo.HerramientaId equals herramienta.Id
                           where proyectoIds.Contains(vinculo.ProyectoId)
                           orderby herramienta.Nombre, herramienta.Id
                           select new
                           {
                               vinculo.ProyectoId,
                               Herramienta = new PortafolioHerramientaDto(
                                   herramienta.Id,
                                   herramienta.Nombre,
                                   herramienta.Categoria)
                           })
            .ToListAsync(cancellationToken);

        return Agrupar(datos, d => d.ProyectoId, d => d.Herramienta);
    }

    private async Task<IReadOnlyDictionary<Guid, IReadOnlyCollection<PortafolioHerramientaDto>>> ObtenerHerramientasPorLaboratorioAsync(
        Guid[] laboratorioIds,
        CancellationToken cancellationToken)
    {
        if (laboratorioIds.Length == 0)
            return new Dictionary<Guid, IReadOnlyCollection<PortafolioHerramientaDto>>();

        var datos = await (from vinculo in _context.Set<LaboratorioHerramienta>().AsNoTracking()
                           join herramienta in _context.Herramientas.AsNoTracking()
                               on vinculo.HerramientaId equals herramienta.Id
                           where laboratorioIds.Contains(vinculo.LaboratorioId)
                           orderby herramienta.Nombre, herramienta.Id
                           select new
                           {
                               vinculo.LaboratorioId,
                               Herramienta = new PortafolioHerramientaDto(
                                   herramienta.Id,
                                   herramienta.Nombre,
                                   herramienta.Categoria)
                           })
            .ToListAsync(cancellationToken);

        return Agrupar(datos, d => d.LaboratorioId, d => d.Herramienta);
    }

    private async Task<IReadOnlyDictionary<Guid, IReadOnlyCollection<PortafolioHerramientaDto>>> ObtenerHerramientasPorArtefactoAsync(
        Guid[] artefactoIds,
        CancellationToken cancellationToken)
    {
        if (artefactoIds.Length == 0)
            return new Dictionary<Guid, IReadOnlyCollection<PortafolioHerramientaDto>>();

        var datos = await (from vinculo in _context.Set<ArtefactoHerramienta>().AsNoTracking()
                           join herramienta in _context.Herramientas.AsNoTracking()
                               on vinculo.HerramientaId equals herramienta.Id
                           where artefactoIds.Contains(vinculo.ArtefactoTecnicoId)
                           orderby herramienta.Nombre, herramienta.Id
                           select new
                           {
                               vinculo.ArtefactoTecnicoId,
                               Herramienta = new PortafolioHerramientaDto(
                                   herramienta.Id,
                                   herramienta.Nombre,
                                   herramienta.Categoria)
                           })
            .ToListAsync(cancellationToken);

        return Agrupar(datos, d => d.ArtefactoTecnicoId, d => d.Herramienta);
    }

    private static IReadOnlyDictionary<TKey, IReadOnlyCollection<TValue>> Agrupar<TDato, TKey, TValue>(
        IEnumerable<TDato> datos,
        Func<TDato, TKey> obtenerClave,
        Func<TDato, TValue> obtenerValor)
        where TKey : notnull =>
        datos.GroupBy(obtenerClave)
            .ToDictionary(
                grupo => grupo.Key,
                grupo => (IReadOnlyCollection<TValue>)grupo.Select(obtenerValor).ToArray());

    private static IReadOnlyCollection<T> ObtenerColeccion<T>(
        IReadOnlyDictionary<Guid, IReadOnlyCollection<T>> datos,
        Guid id) =>
        datos.TryGetValue(id, out var valores) ? valores : [];

    private static bool DebeIncluirTipo(TipoEvidencePortafolio? filtro, TipoEvidencePortafolio tipo) =>
        !filtro.HasValue || filtro.Value == tipo;

    private static bool EsMadurezElegible(EstadoMadurez estadoMadurez) =>
        estadoMadurez is EstadoMadurez.ListoPortafolio or EstadoMadurez.Publicado;

    private static PortafolioResumenDto CrearResumen(
        IReadOnlyCollection<PortafolioProyectoDto> proyectos,
        IReadOnlyCollection<PortafolioLaboratorioDto> laboratorios,
        IReadOnlyCollection<PortafolioWriteupDto> writeups,
        IReadOnlyCollection<PortafolioArtefactoTecnicoDto> artefactos,
        IReadOnlyCollection<PortafolioCertificacionObtenidaDto> certificaciones)
    {
        var total = proyectos.Count + laboratorios.Count + writeups.Count + artefactos.Count + certificaciones.Count;
        var listos = proyectos.Count(p => p.EstadoMadurez == EstadoMadurez.ListoPortafolio)
            + laboratorios.Count(l => l.EstadoMadurez == EstadoMadurez.ListoPortafolio)
            + writeups.Count(w => w.EstadoMadurez == EstadoMadurez.ListoPortafolio)
            + artefactos.Count(a => a.EstadoMadurez == EstadoMadurez.ListoPortafolio)
            + certificaciones.Count(c => c.EstadoMadurez == EstadoMadurez.ListoPortafolio);
        var publicados = proyectos.Count(p => p.EstadoMadurez == EstadoMadurez.Publicado)
            + laboratorios.Count(l => l.EstadoMadurez == EstadoMadurez.Publicado)
            + writeups.Count(w => w.EstadoMadurez == EstadoMadurez.Publicado)
            + artefactos.Count(a => a.EstadoMadurez == EstadoMadurez.Publicado)
            + certificaciones.Count(c => c.EstadoMadurez == EstadoMadurez.Publicado);

        return new PortafolioResumenDto(
            total,
            proyectos.Count,
            laboratorios.Count,
            writeups.Count,
            artefactos.Count,
            certificaciones.Count,
            listos,
            publicados);
    }

    private sealed record ProyectoPlano(
        Guid ProyectoId,
        string Nombre,
        string? Descripcion,
        EstadoProyecto Estado,
        EstadoMadurez EstadoMadurez,
        string? RepositorioUrl,
        DateOnly? FechaInicio,
        DateOnly? FechaFin);

    private sealed record LaboratorioPlano(
        Guid LaboratorioId,
        string Nombre,
        string? Objetivo,
        string? EntornoVms,
        string? Hallazgos,
        int? TiempoInvertidoMinutos,
        DateOnly? Fecha,
        EstadoMadurez EstadoMadurez);

    private sealed record WriteupPlano(
        Guid WriteupId,
        string Titulo,
        string? PlataformaOrigen,
        string? Url,
        DateOnly? Fecha,
        EstadoMadurez EstadoMadurez);

    private sealed record ArtefactoPlano(
        Guid ArtefactoTecnicoId,
        TipoArtefacto TipoArtefacto,
        string Nombre,
        string? ContenidoOUrl,
        string? LenguajeTecnologia,
        EstadoMadurez EstadoMadurez);
}
