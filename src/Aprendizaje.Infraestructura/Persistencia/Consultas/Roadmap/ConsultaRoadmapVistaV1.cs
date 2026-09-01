using Aprendizaje.Aplicacion.Roadmap.Vistas;
using Aprendizaje.Aplicacion.Roadmap.Vistas.RoadmapVistaV1;
using Aprendizaje.Dominio.Roadmap;
using Aprendizaje.Dominio.Roadmap.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Aprendizaje.Infraestructura.Persistencia.Consultas.Roadmap;

public sealed class ConsultaRoadmapVistaV1 : IConsultaRoadmapVistaV1
{
    private readonly AprendizajeDbContext _context;

    public ConsultaRoadmapVistaV1(AprendizajeDbContext context)
    {
        _context = context;
    }

    public async Task<RoadmapVistaV1Dto> ObtenerAsync(
        Guid usuarioId,
        DateTime ahoraUtc,
        CancellationToken cancellationToken = default)
    {
        var intervaloDefectoDias = await _context.Usuarios
            .AsNoTracking()
            .Where(u => u.Id == usuarioId)
            .Select(u => (int?)u.IntervaloRepasoDefectoDias)
            .SingleOrDefaultAsync(cancellationToken);

        if (!intervaloDefectoDias.HasValue)
            return new RoadmapVistaV1Dto(null, 0, 0, 0, []);

        var fases = await _context.Fases
            .AsNoTracking()
            .Where(f => f.UsuarioId == usuarioId)
            .OrderBy(f => f.Orden)
            .ThenBy(f => f.Id)
            .ToListAsync(cancellationToken);

        if (fases.Count == 0)
            return new RoadmapVistaV1Dto(null, 0, 0, 0, []);

        var faseIds = fases.Select(f => f.Id).ToArray();

        var temas = await _context.Temas
            .AsNoTracking()
            .Include(t => t.Criterios)
            .Where(t => t.UsuarioId == usuarioId && t.FaseId.HasValue && faseIds.Contains(t.FaseId.Value))
            .OrderBy(t => t.Nombre)
            .ThenBy(t => t.Id)
            .ToListAsync(cancellationToken);

        var temaIds = temas.Select(t => t.Id).ToArray();
        var ultimasSesiones = temaIds.Length == 0
            ? new Dictionary<Guid, DateOnly>()
            : await _context.SesionesEstudio
                .AsNoTracking()
                .Where(s => s.UsuarioId == usuarioId && temaIds.Contains(s.TemaId))
                .GroupBy(s => s.TemaId)
                .Select(grupo => new
                {
                    TemaId = grupo.Key,
                    UltimaSesion = grupo.Max(s => s.Fecha)
                })
                .ToDictionaryAsync(s => s.TemaId, s => s.UltimaSesion, cancellationToken);

        var intervaloDefecto = IntervaloRepaso.Crear(intervaloDefectoDias.Value);
        var temasPorFase = temas
            .Select(t => CrearTemaVista(t, ultimasSesiones, intervaloDefecto, ahoraUtc))
            .GroupBy(t => t.FaseId!.Value)
            .ToDictionary(grupo => grupo.Key, grupo => grupo.ToArray());

        var fasesIntermedias = fases
            .Select(f => CrearFaseIntermedia(f, temasPorFase.GetValueOrDefault(f.Id) ?? []))
            .ToArray();
        var faseActualId = CalcularFaseActualId(fasesIntermedias);

        var fasesVista = fasesIntermedias
            .Select(f => f.ToDto(f.Id == faseActualId))
            .ToArray();
        var todosLosTemas = fasesVista.SelectMany(f => f.Temas).ToArray();
        var temasEvaluablesGlobales = todosLosTemas
            .Where(t => !EsNodoOrganizativoSinCriterios(t, todosLosTemas))
            .ToArray();

        return new RoadmapVistaV1Dto(
            faseActualId,
            SemanticaTemaReadSide.CalcularPromedioPorcentaje(
                temasEvaluablesGlobales.Select(t => t.ProgresoPorcentaje)),
            todosLosTemas.Length,
            temasEvaluablesGlobales.Count(EsTemaCompletadoEstructuralmente),
            fasesVista);
    }

    private static TemaRoadmapVistaV1Dto CrearTemaVista(
        Tema tema,
        IReadOnlyDictionary<Guid, DateOnly> ultimasSesiones,
        IntervaloRepaso intervaloDefecto,
        DateTime ahoraUtc)
    {
        var ultimaSesion = ultimasSesiones.GetValueOrDefault(tema.Id);
        var semantica = SemanticaTemaReadSide.Calcular(
            tema,
            ultimaSesion == default ? null : ultimaSesion,
            intervaloDefecto,
            ahoraUtc);

        return new TemaRoadmapVistaV1Dto(
            tema.Id,
            tema.FaseId,
            tema.TemaPadreId,
            tema.Nombre,
            tema.Descripcion,
            tema.TipoConocimiento,
            tema.DificultadPercibida?.Valor,
            tema.Confianza?.Valor,
            semantica.IntervaloRepasoDias,
            semantica.Estado,
            semantica.CriteriosTotal,
            semantica.CriteriosCumplidos,
            semantica.ProgresoPorcentaje,
            ultimaSesion == default ? null : ultimaSesion,
            semantica.ProximaFechaRepaso,
            semantica.RepasoRecomendado);
    }

    private static FaseIntermedia CrearFaseIntermedia(
        Fase fase,
        IReadOnlyCollection<TemaRoadmapVistaV1Dto> temas)
    {
        var totalTemas = temas.Count;
        var temasEvaluables = temas
            .Where(t => !EsNodoOrganizativoSinCriterios(t, temas))
            .ToArray();
        var temasDominados = temasEvaluables.Count(EsTemaCompletadoEstructuralmente);
        var progresoPorcentaje = SemanticaTemaReadSide.CalcularPromedioPorcentaje(
            temasEvaluables.Select(t => t.ProgresoPorcentaje));
        var estaCompletada = temasEvaluables.Length > 0
            && temasEvaluables.All(EsTemaCompletadoEstructuralmente);

        return new FaseIntermedia(
            fase.Id,
            fase.Orden,
            fase.Nombre,
            fase.Color,
            fase.Descripcion,
            fase.Objetivos,
            fase.CriteriosAvance,
            fase.MesInicioRecomendado,
            fase.MesFinRecomendado,
            fase.CargaSemanalRecomendada,
            totalTemas,
            temasDominados,
            progresoPorcentaje,
            estaCompletada,
            temas);
    }

    private static Guid? CalcularFaseActualId(IReadOnlyCollection<FaseIntermedia> fases)
    {
        if (fases.Count == 0)
            return null;

        var primeraIncompleta = fases.FirstOrDefault(f => !f.EstaCompletada);

        return primeraIncompleta?.Id ?? fases.Last().Id;
    }

    private static bool EsTemaCompletadoEstructuralmente(TemaRoadmapVistaV1Dto tema) =>
        SemanticaTemaReadSide.EsTemaCompletadoEstructuralmente(
            tema.CriteriosTotal,
            tema.CriteriosCumplidos);

    private static bool EsNodoOrganizativoSinCriterios(
        TemaRoadmapVistaV1Dto tema,
        IReadOnlyCollection<TemaRoadmapVistaV1Dto> temasDeLaFase) =>
        SemanticaTemaReadSide.EsNodoOrganizativoSinCriterios(
            tema.Id,
            tema.CriteriosTotal,
            temasDeLaFase.Select(t => t.TemaPadreId));

    private sealed record FaseIntermedia(
        Guid Id,
        int Orden,
        string Nombre,
        string? Color,
        string? Descripcion,
        IReadOnlyCollection<string> Objetivos,
        IReadOnlyCollection<string> CriteriosAvance,
        int? MesInicioRecomendado,
        int? MesFinRecomendado,
        string? CargaSemanalRecomendada,
        int TotalTemas,
        int TemasDominados,
        int ProgresoPorcentaje,
        bool EstaCompletada,
        IReadOnlyCollection<TemaRoadmapVistaV1Dto> Temas)
    {
        public FaseRoadmapVistaV1Dto ToDto(bool esFaseActual) =>
            new(
                Id,
                Orden,
                Nombre,
                Color,
                Descripcion,
                Objetivos,
                CriteriosAvance,
                MesInicioRecomendado,
                MesFinRecomendado,
                CargaSemanalRecomendada,
                TotalTemas,
                TemasDominados,
                ProgresoPorcentaje,
                EstaCompletada,
                esFaseActual,
                Temas);
    }
}
