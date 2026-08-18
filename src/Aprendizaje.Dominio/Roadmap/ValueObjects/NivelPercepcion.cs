using Aprendizaje.Dominio.Comun;

namespace Aprendizaje.Dominio.Roadmap.ValueObjects;

/// <summary>
/// Representa un nivel de percepción subjetiva (dificultad o confianza) en escala 1-5.
/// La invariante de rango es la razón de ser de este Value Object — ver convención 17, criterio #1.
/// </summary>
public sealed class NivelPercepcion : ValueObject
{
    public int Valor { get; }

    private NivelPercepcion(int valor) => Valor = valor;

    public static NivelPercepcion Crear(int valor)
    {
        if (valor is < 1 or > 5)
            throw new ArgumentOutOfRangeException(
                nameof(valor), valor, "El nivel de percepción debe estar entre 1 y 5.");

        return new NivelPercepcion(valor);
    }

    protected override IEnumerable<object?> ComponentesIguales()
    {
        yield return Valor;
    }
}
