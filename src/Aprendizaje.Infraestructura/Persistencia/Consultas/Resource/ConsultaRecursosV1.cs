using Aprendizaje.Aplicacion.Resource.Recursos;
using Aprendizaje.Aplicacion.Resource.Recursos.ListarRecursos;
using Aprendizaje.Aplicacion.Resource.Recursos.ObtenerRecursoPorId;
using Aprendizaje.Dominio.Resource;
using Aprendizaje.Infraestructura.Persistencia.ModelosUnion;
using Microsoft.EntityFrameworkCore;

namespace Aprendizaje.Infraestructura.Persistencia.Consultas.Resource;

public sealed class ConsultaRecursosV1 : IConsultaRecursosV1
{
    private readonly AprendizajeDbContext _context;

    public ConsultaRecursosV1(AprendizajeDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyCollection<RecursoResumen>> ListarAsync(
        ListarRecursosSolicitud solicitud,
        CancellationToken cancellationToken = default)
    {
        var recursos = await CrearQueryRecursos(solicitud)
            .OrderBy(r => r.Titulo)
            .ThenBy(r => r.Id)
            .Select(r => new RecursoPlano(
                r.Id,
                r.UsuarioId,
                r.Tipo,
                r.Titulo,
                r.Url,
                r.Estado,
                null,
                null,
                null,
                null))
            .ToListAsync(cancellationToken);

        var temasPorRecurso = await ObtenerTemasPorRecursoAsync(
            solicitud.UsuarioId,
            recursos.Select(r => r.Id).ToArray(),
            cancellationToken);

        return recursos
            .Select(r => new RecursoResumen(
                r.Id,
                r.UsuarioId,
                r.Tipo,
                r.Titulo,
                r.Url,
                r.Estado,
                ObtenerColeccion(temasPorRecurso, r.Id)))
            .ToArray();
    }

    public async Task<RecursoDetalle?> ObtenerDetalleAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var recurso = await _context.Recursos
            .AsNoTracking()
            .Where(r => r.Id == id)
            .Select(r => new RecursoPlano(
                r.Id,
                r.UsuarioId,
                r.Tipo,
                r.Titulo,
                r.Url,
                r.Estado,
                r.Rating == null ? null : r.Rating.Valor,
                r.Notas,
                r.HerramientaIA,
                r.PromptsUtilizados))
            .SingleOrDefaultAsync(cancellationToken);

        if (recurso is null)
            return null;

        var temasPorRecurso = await ObtenerTemasPorRecursoAsync(
            recurso.UsuarioId,
            [recurso.Id],
            cancellationToken);

        return new RecursoDetalle(
            recurso.Id,
            recurso.UsuarioId,
            recurso.Tipo,
            recurso.Titulo,
            recurso.Url,
            recurso.Estado,
            recurso.Rating,
            recurso.Notas,
            recurso.HerramientaIA,
            recurso.PromptsUtilizados,
            ObtenerColeccion(temasPorRecurso, recurso.Id));
    }

    private IQueryable<Recurso> CrearQueryRecursos(ListarRecursosSolicitud solicitud)
    {
        var query = _context.Recursos
            .AsNoTracking()
            .Where(r => r.UsuarioId == solicitud.UsuarioId);

        if (!solicitud.TemaId.HasValue)
            return query;

        var temaId = solicitud.TemaId.Value;

        return from recurso in query
               join vinculacion in _context.Set<RecursoTema>().AsNoTracking()
                   on recurso.Id equals vinculacion.RecursoId
               join tema in _context.Temas.AsNoTracking()
                   on vinculacion.TemaId equals tema.Id
               where tema.Id == temaId && tema.UsuarioId == solicitud.UsuarioId
               select recurso;
    }

    private async Task<IReadOnlyDictionary<Guid, IReadOnlyCollection<RecursoTemaResumen>>> ObtenerTemasPorRecursoAsync(
        Guid usuarioId,
        Guid[] recursoIds,
        CancellationToken cancellationToken)
    {
        if (recursoIds.Length == 0)
            return new Dictionary<Guid, IReadOnlyCollection<RecursoTemaResumen>>();

        var datos = await (from vinculacion in _context.Set<RecursoTema>().AsNoTracking()
                           join tema in _context.Temas.AsNoTracking()
                               on vinculacion.TemaId equals tema.Id
                           where recursoIds.Contains(vinculacion.RecursoId) && tema.UsuarioId == usuarioId
                           orderby tema.Nombre, tema.Id
                           select new
                           {
                               vinculacion.RecursoId,
                               Tema = new RecursoTemaResumen(tema.Id, tema.Nombre)
                           })
            .ToListAsync(cancellationToken);

        return datos
            .GroupBy(d => d.RecursoId)
            .ToDictionary(
                grupo => grupo.Key,
                grupo => (IReadOnlyCollection<RecursoTemaResumen>)grupo.Select(d => d.Tema).ToArray());
    }

    private static IReadOnlyCollection<RecursoTemaResumen> ObtenerColeccion(
        IReadOnlyDictionary<Guid, IReadOnlyCollection<RecursoTemaResumen>> datos,
        Guid id) =>
        datos.TryGetValue(id, out var valores) ? valores : [];

    private sealed record RecursoPlano(
        Guid Id,
        Guid UsuarioId,
        TipoRecurso Tipo,
        string Titulo,
        string? Url,
        EstadoRecurso Estado,
        int? Rating,
        string? Notas,
        string? HerramientaIA,
        string? PromptsUtilizados);
}
