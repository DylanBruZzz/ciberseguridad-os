namespace Aprendizaje.Dominio.Comun;

public abstract class Entidad
{
    public Guid Id { get; protected set; }
    public DateTime FechaCreacionUtc { get; protected set; }

    /// <summary>
    /// Setter 'protected', pero EF Core la escribe igualmente a través de su ChangeTracker
    /// (metadata de sombra sobre el miembro, no acceso normal de C#) desde el
    /// AuditoriaInterceptor descrito en la convención 4/12 — nunca se asigna a mano
    /// dentro de un método de comportamiento del dominio.
    /// </summary>
    public DateTime? FechaModificacionUtc { get; protected set; }

    protected Entidad() { } // requerido por EF Core, nunca público

    protected Entidad(Guid id)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("El identificador de una Entidad no puede ser Guid.Empty.", nameof(id));

        Id = id;
        FechaCreacionUtc = DateTime.UtcNow;
    }

    public override bool Equals(object? obj) =>
        obj is Entidad otra && otra.GetType() == GetType() && otra.Id == Id;

    public override int GetHashCode() => Id.GetHashCode();
}
