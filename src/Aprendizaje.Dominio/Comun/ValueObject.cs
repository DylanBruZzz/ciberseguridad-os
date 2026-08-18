namespace Aprendizaje.Dominio.Comun;

public abstract class ValueObject
{
    protected abstract IEnumerable<object?> ComponentesIguales();

    public override bool Equals(object? obj) =>
        obj is ValueObject otro && GetType() == otro.GetType()
        && ComponentesIguales().SequenceEqual(otro.ComponentesIguales());

    public override int GetHashCode() =>
        ComponentesIguales().Aggregate(0, (h, c) => HashCode.Combine(h, c));
}
