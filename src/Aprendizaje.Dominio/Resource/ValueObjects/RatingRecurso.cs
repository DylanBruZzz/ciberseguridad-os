using Aprendizaje.Dominio.Comun;

namespace Aprendizaje.Dominio.Resource.ValueObjects;

/// <summary>
/// Valoración personal de un Recurso en escala 1-5. Concepto propio del módulo Resource:
/// deliberadamente no reutiliza NivelPercepcion (Roadmap) aunque comparta el mismo rango — la
/// similitud de la restricción no implica identidad semántica del concepto, y ningún módulo de
/// dominio importa tipos concretos de otro (los módulos solo se referencian por Guid).
/// </summary>
public sealed class RatingRecurso : ValueObject
{
    public int Valor { get; }

    private RatingRecurso(int valor) => Valor = valor;

    public static RatingRecurso Crear(int valor)
    {
        if (valor is < 1 or > 5)
            throw new ArgumentOutOfRangeException(nameof(valor), valor, "El rating de un Recurso debe estar entre 1 y 5.");

        return new RatingRecurso(valor);
    }

    protected override IEnumerable<object?> ComponentesIguales()
    {
        yield return Valor;
    }
}
