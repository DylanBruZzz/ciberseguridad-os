using Aprendizaje.Dominio.Comun;

namespace Aprendizaje.Dominio.Roadmap.ValueObjects;

/// <summary>
/// Encapsula el intervalo de repaso de un Tema y el cálculo de su próxima fecha.
/// El comportamiento propio es la razón de ser de este Value Object — ver convención 17, criterio #2.
/// </summary>
public sealed class IntervaloRepaso : ValueObject
{
    public const int DiasPorDefecto = 90;

    public int Dias { get; }

    private IntervaloRepaso(int dias) => Dias = dias;

    public static IntervaloRepaso Crear(int dias)
    {
        if (dias <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(dias), dias, "El intervalo de repaso debe ser mayor a cero días.");

        return new IntervaloRepaso(dias);
    }

    public static IntervaloRepaso Defecto() => new(DiasPorDefecto);

    public DateTime ProximaFecha(DateTime ultimaPracticaUtc) => ultimaPracticaUtc.AddDays(Dias);

    public bool HaVencido(DateTime ultimaPracticaUtc, DateTime ahoraUtc) =>
        (ahoraUtc - ultimaPracticaUtc).TotalDays > Dias;

    protected override IEnumerable<object?> ComponentesIguales()
    {
        yield return Dias;
    }
}
