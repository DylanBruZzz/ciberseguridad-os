using Aprendizaje.Aplicacion.Evidence.Vistas.EvidenceListaV1;
using Aprendizaje.Dominio.Evidence;
using Aprendizaje.Infraestructura.Persistencia.ModelosUnion;
using Microsoft.EntityFrameworkCore;

namespace Aprendizaje.Infraestructura.Persistencia.Consultas.Evidence;

public sealed class ConsultaEvidenceListaV1 : IConsultaEvidenceListaV1
{
    private readonly AprendizajeDbContext _context;

    public ConsultaEvidenceListaV1(AprendizajeDbContext context)
    {
        _context = context;
    }

    public async Task<EvidenceListaV1Dto> ObtenerAsync(
        ObtenerEvidenceListaV1Solicitud solicitud,
        CancellationToken cancellationToken = default)
    {
        var proyectos = DebeIncluirTipo(solicitud.TipoEvidence, TipoEvidenceV1.Proyecto)
            ? await ObtenerProyectosAsync(solicitud, cancellationToken)
            : [];
        var laboratorios = DebeIncluirTipo(solicitud.TipoEvidence, TipoEvidenceV1.Laboratorio)
            ? await ObtenerLaboratoriosAsync(solicitud, cancellationToken)
            : [];
        var writeups = DebeIncluirTipo(solicitud.TipoEvidence, TipoEvidenceV1.Writeup)
            ? await ObtenerWriteupsAsync(solicitud, cancellationToken)
            : [];
        var artefactos = DebeIncluirTipo(solicitud.TipoEvidence, TipoEvidenceV1.ArtefactoTecnico)
            ? await ObtenerArtefactosAsync(solicitud, cancellationToken)
            : [];
        var certificaciones = DebeIncluirTipo(solicitud.TipoEvidence, TipoEvidenceV1.CertificacionObtenida)
            ? await ObtenerCertificacionesObtenidasAsync(solicitud, cancellationToken)
            : [];

        var proyectoIds = proyectos.Select(p => p.Id).ToArray();
        var laboratorioIds = laboratorios.Select(l => l.Id).ToArray();
        var writeupIds = writeups.Select(w => w.Id).ToArray();
        var artefactoIds = artefactos.Select(a => a.Id).ToArray();
        var certificacionIds = certificaciones
            .Where(c => c.CertificacionId.HasValue)
            .Select(c => c.CertificacionId!.Value)
            .Distinct()
            .ToArray();
        var herramientasVacias = new Dictionary<Guid, IReadOnlyCollection<EvidenceHerramientaV1Dto>>();

        var temasPorProyecto = await ObtenerTemasPorProyectoAsync(solicitud.UsuarioId, proyectoIds, cancellationToken);
        var temasPorLaboratorio = await ObtenerTemasPorLaboratorioAsync(solicitud.UsuarioId, laboratorioIds, cancellationToken);
        var temasPorWriteup = await ObtenerTemasPorWriteupAsync(solicitud.UsuarioId, writeupIds, cancellationToken);
        var temasPorArtefacto = await ObtenerTemasPorArtefactoAsync(solicitud.UsuarioId, artefactoIds, cancellationToken);
        var temasPorCertificacion = await ObtenerTemasPorCertificacionAsync(
            solicitud.UsuarioId,
            certificacionIds,
            cancellationToken);
        var herramientasPorProyecto = await ObtenerHerramientasPorProyectoAsync(proyectoIds, cancellationToken);
        var herramientasPorLaboratorio = await ObtenerHerramientasPorLaboratorioAsync(laboratorioIds, cancellationToken);
        var herramientasPorArtefacto = await ObtenerHerramientasPorArtefactoAsync(artefactoIds, cancellationToken);

        var items = proyectos.Select(p => CrearItem(p, temasPorProyecto, herramientasPorProyecto))
            .Concat(laboratorios.Select(l => CrearItem(l, temasPorLaboratorio, herramientasPorLaboratorio)))
            .Concat(writeups.Select(w => CrearItem(w, temasPorWriteup, herramientasVacias)))
            .Concat(artefactos.Select(a => CrearItem(a, temasPorArtefacto, herramientasPorArtefacto)))
            .Concat(certificaciones.Select(c => CrearItemCertificacion(c, temasPorCertificacion)))
            .OrderByDescending(i => i.FechaActividadUtc)
            .ThenBy(i => i.TipoEvidence)
            .ThenBy(i => i.Titulo)
            .ThenBy(i => i.Id)
            .ToArray();

        return new EvidenceListaV1Dto(items.Length, items);
    }

    private async Task<IReadOnlyCollection<EvidencePlano>> ObtenerProyectosAsync(
        ObtenerEvidenceListaV1Solicitud solicitud,
        CancellationToken cancellationToken)
    {
        var query = _context.Proyectos
            .AsNoTracking()
            .Where(p => p.UsuarioId == solicitud.UsuarioId
                && (!solicitud.EstadoMadurez.HasValue || p.EstadoMadurez == solicitud.EstadoMadurez.Value));

        if (solicitud.TemaId.HasValue)
        {
            var temaId = solicitud.TemaId.Value;
            query = from proyecto in query
                    join vinculacion in _context.Set<ProyectoTema>().AsNoTracking()
                        on proyecto.Id equals vinculacion.ProyectoId
                    join tema in _context.Temas.AsNoTracking()
                        on vinculacion.TemaId equals tema.Id
                    where tema.Id == temaId && tema.UsuarioId == solicitud.UsuarioId
                    select proyecto;
        }

        return await query
            .Select(p => new EvidencePlano(
                p.Id,
                null,
                TipoEvidenceV1.Proyecto,
                p.Nombre,
                p.EstadoMadurez,
                p.FechaCreacionUtc,
                p.FechaModificacionUtc,
                p.FechaModificacionUtc ?? p.FechaCreacionUtc,
                p.FechaFin ?? p.FechaInicio))
            .ToListAsync(cancellationToken);
    }

    private async Task<IReadOnlyCollection<EvidencePlano>> ObtenerLaboratoriosAsync(
        ObtenerEvidenceListaV1Solicitud solicitud,
        CancellationToken cancellationToken)
    {
        var query = _context.Laboratorios
            .AsNoTracking()
            .Where(l => l.UsuarioId == solicitud.UsuarioId
                && (!solicitud.EstadoMadurez.HasValue || l.EstadoMadurez == solicitud.EstadoMadurez.Value));

        if (solicitud.TemaId.HasValue)
        {
            var temaId = solicitud.TemaId.Value;
            query = from laboratorio in query
                    join vinculacion in _context.Set<LaboratorioTema>().AsNoTracking()
                        on laboratorio.Id equals vinculacion.LaboratorioId
                    join tema in _context.Temas.AsNoTracking()
                        on vinculacion.TemaId equals tema.Id
                    where tema.Id == temaId && tema.UsuarioId == solicitud.UsuarioId
                    select laboratorio;
        }

        return await query
            .Select(l => new EvidencePlano(
                l.Id,
                null,
                TipoEvidenceV1.Laboratorio,
                l.Nombre,
                l.EstadoMadurez,
                l.FechaCreacionUtc,
                l.FechaModificacionUtc,
                l.FechaModificacionUtc ?? l.FechaCreacionUtc,
                l.Fecha))
            .ToListAsync(cancellationToken);
    }

    private async Task<IReadOnlyCollection<EvidencePlano>> ObtenerWriteupsAsync(
        ObtenerEvidenceListaV1Solicitud solicitud,
        CancellationToken cancellationToken)
    {
        var query = _context.Writeups
            .AsNoTracking()
            .Where(w => w.UsuarioId == solicitud.UsuarioId
                && (!solicitud.EstadoMadurez.HasValue || w.EstadoMadurez == solicitud.EstadoMadurez.Value));

        if (solicitud.TemaId.HasValue)
        {
            var temaId = solicitud.TemaId.Value;
            query = from writeup in query
                    join vinculacion in _context.Set<WriteupTema>().AsNoTracking()
                        on writeup.Id equals vinculacion.WriteupId
                    join tema in _context.Temas.AsNoTracking()
                        on vinculacion.TemaId equals tema.Id
                    where tema.Id == temaId && tema.UsuarioId == solicitud.UsuarioId
                    select writeup;
        }

        return await query
            .Select(w => new EvidencePlano(
                w.Id,
                null,
                TipoEvidenceV1.Writeup,
                w.Titulo,
                w.EstadoMadurez,
                w.FechaCreacionUtc,
                w.FechaModificacionUtc,
                w.FechaModificacionUtc ?? w.FechaCreacionUtc,
                w.Fecha))
            .ToListAsync(cancellationToken);
    }

    private async Task<IReadOnlyCollection<EvidencePlano>> ObtenerArtefactosAsync(
        ObtenerEvidenceListaV1Solicitud solicitud,
        CancellationToken cancellationToken)
    {
        var query = _context.ArtefactosTecnicos
            .AsNoTracking()
            .Where(a => a.UsuarioId == solicitud.UsuarioId
                && (!solicitud.EstadoMadurez.HasValue || a.EstadoMadurez == solicitud.EstadoMadurez.Value));

        if (solicitud.TemaId.HasValue)
        {
            var temaId = solicitud.TemaId.Value;
            query = from artefacto in query
                    join vinculacion in _context.Set<ArtefactoTema>().AsNoTracking()
                        on artefacto.Id equals vinculacion.ArtefactoTecnicoId
                    join tema in _context.Temas.AsNoTracking()
                        on vinculacion.TemaId equals tema.Id
                    where tema.Id == temaId && tema.UsuarioId == solicitud.UsuarioId
                    select artefacto;
        }

        return await query
            .Select(a => new EvidencePlano(
                a.Id,
                null,
                TipoEvidenceV1.ArtefactoTecnico,
                a.Nombre,
                a.EstadoMadurez,
                a.FechaCreacionUtc,
                a.FechaModificacionUtc,
                a.FechaModificacionUtc ?? a.FechaCreacionUtc,
                null))
            .ToListAsync(cancellationToken);
    }

    private async Task<IReadOnlyCollection<EvidencePlano>> ObtenerCertificacionesObtenidasAsync(
        ObtenerEvidenceListaV1Solicitud solicitud,
        CancellationToken cancellationToken)
    {
        var query = from obtenida in _context.CertificacionesObtenidas.AsNoTracking()
                    join certificacion in _context.Certificaciones.AsNoTracking()
                        on obtenida.CertificacionId equals certificacion.Id
                    where obtenida.UsuarioId == solicitud.UsuarioId
                        && (!solicitud.EstadoMadurez.HasValue
                            || obtenida.EstadoMadurez == solicitud.EstadoMadurez.Value)
                    select new { Obtenida = obtenida, Certificacion = certificacion };

        if (solicitud.TemaId.HasValue)
        {
            var temaId = solicitud.TemaId.Value;
            query = from dato in query
                    join vinculacion in _context.Set<CertificacionTema>().AsNoTracking()
                        on dato.Certificacion.Id equals vinculacion.CertificacionId
                    join tema in _context.Temas.AsNoTracking()
                        on vinculacion.TemaId equals tema.Id
                    where tema.Id == temaId && tema.UsuarioId == solicitud.UsuarioId
                    select dato;
        }

        return await query
            .Select(c => new EvidencePlano(
                c.Obtenida.Id,
                c.Certificacion.Id,
                TipoEvidenceV1.CertificacionObtenida,
                c.Certificacion.Nombre,
                c.Obtenida.EstadoMadurez,
                c.Obtenida.FechaCreacionUtc,
                c.Obtenida.FechaModificacionUtc,
                c.Obtenida.FechaModificacionUtc ?? c.Obtenida.FechaCreacionUtc,
                c.Obtenida.FechaObtencion))
            .ToListAsync(cancellationToken);
    }

    private async Task<IReadOnlyDictionary<Guid, IReadOnlyCollection<EvidenceTemaV1Dto>>> ObtenerTemasPorProyectoAsync(
        Guid usuarioId,
        Guid[] proyectoIds,
        CancellationToken cancellationToken)
    {
        if (proyectoIds.Length == 0)
            return new Dictionary<Guid, IReadOnlyCollection<EvidenceTemaV1Dto>>();

        var datos = await (from vinculacion in _context.Set<ProyectoTema>().AsNoTracking()
                           join tema in _context.Temas.AsNoTracking() on vinculacion.TemaId equals tema.Id
                           where proyectoIds.Contains(vinculacion.ProyectoId) && tema.UsuarioId == usuarioId
                           orderby tema.Nombre, tema.Id
                           select new { vinculacion.ProyectoId, Tema = new EvidenceTemaV1Dto(tema.Id, tema.Nombre) })
            .ToListAsync(cancellationToken);

        return Agrupar(datos, d => d.ProyectoId, d => d.Tema);
    }

    private async Task<IReadOnlyDictionary<Guid, IReadOnlyCollection<EvidenceTemaV1Dto>>> ObtenerTemasPorLaboratorioAsync(
        Guid usuarioId,
        Guid[] laboratorioIds,
        CancellationToken cancellationToken)
    {
        if (laboratorioIds.Length == 0)
            return new Dictionary<Guid, IReadOnlyCollection<EvidenceTemaV1Dto>>();

        var datos = await (from vinculacion in _context.Set<LaboratorioTema>().AsNoTracking()
                           join tema in _context.Temas.AsNoTracking() on vinculacion.TemaId equals tema.Id
                           where laboratorioIds.Contains(vinculacion.LaboratorioId) && tema.UsuarioId == usuarioId
                           orderby tema.Nombre, tema.Id
                           select new { vinculacion.LaboratorioId, Tema = new EvidenceTemaV1Dto(tema.Id, tema.Nombre) })
            .ToListAsync(cancellationToken);

        return Agrupar(datos, d => d.LaboratorioId, d => d.Tema);
    }

    private async Task<IReadOnlyDictionary<Guid, IReadOnlyCollection<EvidenceTemaV1Dto>>> ObtenerTemasPorWriteupAsync(
        Guid usuarioId,
        Guid[] writeupIds,
        CancellationToken cancellationToken)
    {
        if (writeupIds.Length == 0)
            return new Dictionary<Guid, IReadOnlyCollection<EvidenceTemaV1Dto>>();

        var datos = await (from vinculacion in _context.Set<WriteupTema>().AsNoTracking()
                           join tema in _context.Temas.AsNoTracking() on vinculacion.TemaId equals tema.Id
                           where writeupIds.Contains(vinculacion.WriteupId) && tema.UsuarioId == usuarioId
                           orderby tema.Nombre, tema.Id
                           select new { vinculacion.WriteupId, Tema = new EvidenceTemaV1Dto(tema.Id, tema.Nombre) })
            .ToListAsync(cancellationToken);

        return Agrupar(datos, d => d.WriteupId, d => d.Tema);
    }

    private async Task<IReadOnlyDictionary<Guid, IReadOnlyCollection<EvidenceTemaV1Dto>>> ObtenerTemasPorArtefactoAsync(
        Guid usuarioId,
        Guid[] artefactoIds,
        CancellationToken cancellationToken)
    {
        if (artefactoIds.Length == 0)
            return new Dictionary<Guid, IReadOnlyCollection<EvidenceTemaV1Dto>>();

        var datos = await (from vinculacion in _context.Set<ArtefactoTema>().AsNoTracking()
                           join tema in _context.Temas.AsNoTracking() on vinculacion.TemaId equals tema.Id
                           where artefactoIds.Contains(vinculacion.ArtefactoTecnicoId) && tema.UsuarioId == usuarioId
                           orderby tema.Nombre, tema.Id
                           select new
                           {
                               vinculacion.ArtefactoTecnicoId,
                               Tema = new EvidenceTemaV1Dto(tema.Id, tema.Nombre)
                           })
            .ToListAsync(cancellationToken);

        return Agrupar(datos, d => d.ArtefactoTecnicoId, d => d.Tema);
    }

    private async Task<IReadOnlyDictionary<Guid, IReadOnlyCollection<EvidenceTemaV1Dto>>> ObtenerTemasPorCertificacionAsync(
        Guid usuarioId,
        Guid[] certificacionIds,
        CancellationToken cancellationToken)
    {
        if (certificacionIds.Length == 0)
            return new Dictionary<Guid, IReadOnlyCollection<EvidenceTemaV1Dto>>();

        var datos = await (from vinculacion in _context.Set<CertificacionTema>().AsNoTracking()
                           join tema in _context.Temas.AsNoTracking() on vinculacion.TemaId equals tema.Id
                           where certificacionIds.Contains(vinculacion.CertificacionId) && tema.UsuarioId == usuarioId
                           orderby tema.Nombre, tema.Id
                           select new
                           {
                               vinculacion.CertificacionId,
                               Tema = new EvidenceTemaV1Dto(tema.Id, tema.Nombre)
                           })
            .ToListAsync(cancellationToken);

        return Agrupar(datos, d => d.CertificacionId, d => d.Tema);
    }

    private async Task<IReadOnlyDictionary<Guid, IReadOnlyCollection<EvidenceHerramientaV1Dto>>> ObtenerHerramientasPorProyectoAsync(
        Guid[] proyectoIds,
        CancellationToken cancellationToken)
    {
        if (proyectoIds.Length == 0)
            return new Dictionary<Guid, IReadOnlyCollection<EvidenceHerramientaV1Dto>>();

        var datos = await (from vinculacion in _context.Set<ProyectoHerramienta>().AsNoTracking()
                           join herramienta in _context.Herramientas.AsNoTracking()
                               on vinculacion.HerramientaId equals herramienta.Id
                           where proyectoIds.Contains(vinculacion.ProyectoId)
                           orderby herramienta.Nombre, herramienta.Id
                           select new
                           {
                               vinculacion.ProyectoId,
                               Herramienta = new EvidenceHerramientaV1Dto(herramienta.Id, herramienta.Nombre)
                           })
            .ToListAsync(cancellationToken);

        return Agrupar(datos, d => d.ProyectoId, d => d.Herramienta);
    }

    private async Task<IReadOnlyDictionary<Guid, IReadOnlyCollection<EvidenceHerramientaV1Dto>>> ObtenerHerramientasPorLaboratorioAsync(
        Guid[] laboratorioIds,
        CancellationToken cancellationToken)
    {
        if (laboratorioIds.Length == 0)
            return new Dictionary<Guid, IReadOnlyCollection<EvidenceHerramientaV1Dto>>();

        var datos = await (from vinculacion in _context.Set<LaboratorioHerramienta>().AsNoTracking()
                           join herramienta in _context.Herramientas.AsNoTracking()
                               on vinculacion.HerramientaId equals herramienta.Id
                           where laboratorioIds.Contains(vinculacion.LaboratorioId)
                           orderby herramienta.Nombre, herramienta.Id
                           select new
                           {
                               vinculacion.LaboratorioId,
                               Herramienta = new EvidenceHerramientaV1Dto(herramienta.Id, herramienta.Nombre)
                           })
            .ToListAsync(cancellationToken);

        return Agrupar(datos, d => d.LaboratorioId, d => d.Herramienta);
    }

    private async Task<IReadOnlyDictionary<Guid, IReadOnlyCollection<EvidenceHerramientaV1Dto>>> ObtenerHerramientasPorArtefactoAsync(
        Guid[] artefactoIds,
        CancellationToken cancellationToken)
    {
        if (artefactoIds.Length == 0)
            return new Dictionary<Guid, IReadOnlyCollection<EvidenceHerramientaV1Dto>>();

        var datos = await (from vinculacion in _context.Set<ArtefactoHerramienta>().AsNoTracking()
                           join herramienta in _context.Herramientas.AsNoTracking()
                               on vinculacion.HerramientaId equals herramienta.Id
                           where artefactoIds.Contains(vinculacion.ArtefactoTecnicoId)
                           orderby herramienta.Nombre, herramienta.Id
                           select new
                           {
                               vinculacion.ArtefactoTecnicoId,
                               Herramienta = new EvidenceHerramientaV1Dto(herramienta.Id, herramienta.Nombre)
                           })
            .ToListAsync(cancellationToken);

        return Agrupar(datos, d => d.ArtefactoTecnicoId, d => d.Herramienta);
    }

    private static EvidenceItemV1Dto CrearItem(
        EvidencePlano plano,
        IReadOnlyDictionary<Guid, IReadOnlyCollection<EvidenceTemaV1Dto>> temas,
        IReadOnlyDictionary<Guid, IReadOnlyCollection<EvidenceHerramientaV1Dto>> herramientas) =>
        new(
            plano.Id,
            plano.TipoEvidence,
            plano.Titulo,
            plano.EstadoMadurez,
            plano.FechaCreacionUtc,
            plano.FechaModificacionUtc,
            plano.FechaActividadUtc,
            plano.FechaReferencia,
            ObtenerColeccion(temas, plano.Id),
            ObtenerColeccion(herramientas, plano.Id));

    private static EvidenceItemV1Dto CrearItemCertificacion(
        EvidencePlano plano,
        IReadOnlyDictionary<Guid, IReadOnlyCollection<EvidenceTemaV1Dto>> temasPorCertificacion) =>
        new(
            plano.Id,
            plano.TipoEvidence,
            plano.Titulo,
            plano.EstadoMadurez,
            plano.FechaCreacionUtc,
            plano.FechaModificacionUtc,
            plano.FechaActividadUtc,
            plano.FechaReferencia,
            plano.CertificacionId.HasValue ? ObtenerColeccion(temasPorCertificacion, plano.CertificacionId.Value) : [],
            []);

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

    private static bool DebeIncluirTipo(TipoEvidenceV1? filtro, TipoEvidenceV1 tipo) =>
        !filtro.HasValue || filtro.Value == tipo;

    private sealed record EvidencePlano(
        Guid Id,
        Guid? CertificacionId,
        TipoEvidenceV1 TipoEvidence,
        string Titulo,
        EstadoMadurez EstadoMadurez,
        DateTime FechaCreacionUtc,
        DateTime? FechaModificacionUtc,
        DateTime FechaActividadUtc,
        DateOnly? FechaReferencia);
}
