using Aprendizaje.Dominio.Roadmap;
using Aprendizaje.Dominio.Roadmap.ValueObjects;

namespace Aprendizaje.Aplicacion.Roadmap.Vistas;

public sealed record SemanticaTemaReadSideResultado(
    int IntervaloRepasoDias,
    EstadoTema Estado,
    int CriteriosTotal,
    int CriteriosCumplidos,
    int ProgresoPorcentaje,
    DateOnly? ProximaFechaRepaso,
    bool RepasoRecomendado);

public static class SemanticaTemaReadSide
{
    public static SemanticaTemaReadSideResultado Calcular(
        Tema tema,
        DateOnly? ultimaSesion,
        IntervaloRepaso intervaloDefecto,
        DateTime ahoraUtc)
    {
        ArgumentNullException.ThrowIfNull(tema);
        ArgumentNullException.ThrowIfNull(intervaloDefecto);

        var intervaloEfectivo = tema.IntervaloRepaso ?? intervaloDefecto;
        var ultimaPracticaUtc = ultimaSesion.HasValue ? ConvertirAUtc(ultimaSesion.Value) : (DateTime?)null;
        var estado = tema.CalcularEstado(ultimaPracticaUtc, ahoraUtc, intervaloEfectivo);
        var criteriosTotal = tema.Criterios.Count;
        var criteriosCumplidos = tema.Criterios.Count(c => c.Cumplido);

        return new SemanticaTemaReadSideResultado(
            intervaloEfectivo.Dias,
            estado,
            criteriosTotal,
            criteriosCumplidos,
            CalcularPorcentaje(criteriosCumplidos, criteriosTotal),
            ultimaPracticaUtc is null
                ? null
                : DateOnly.FromDateTime(intervaloEfectivo.ProximaFecha(ultimaPracticaUtc.Value)),
            estado == EstadoTema.EnRepaso);
    }

    public static bool EsTemaCompletadoEstructuralmente(int criteriosTotal, int criteriosCumplidos) =>
        criteriosTotal > 0 && criteriosCumplidos == criteriosTotal;

    public static bool EsNodoOrganizativoSinCriterios(
        Guid temaId,
        int criteriosTotal,
        IEnumerable<Guid?> temaPadreIds) =>
        criteriosTotal == 0 && temaPadreIds.Any(id => id == temaId);

    public static int CalcularPromedioPorcentaje(IEnumerable<int> porcentajes)
    {
        var valores = porcentajes.ToArray();

        return valores.Length == 0
            ? 0
            : (int)Math.Round(valores.Average(), MidpointRounding.AwayFromZero);
    }

    private static int CalcularPorcentaje(int cumplidos, int total)
    {
        if (total <= 0)
            return 0;

        return (int)Math.Round(cumplidos * 100m / total, MidpointRounding.AwayFromZero);
    }

    private static DateTime ConvertirAUtc(DateOnly fecha) =>
        DateTime.SpecifyKind(fecha.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
}
