using Aprendizaje.Dominio.Comun;

namespace Aprendizaje.Dominio.Roadmap;

/// <summary>
/// Agrupador visual/curricular sobre el grafo de Temas. No tiene entidades internas,
/// Value Objects ni Domain Events — sus únicas invariantes son de validación simple.
/// </summary>
public sealed class Fase : AggregateRoot
{
    public Guid UsuarioId { get; private set; }
    public string Nombre { get; private set; } = null!;
    public int Orden { get; private set; }
    public string? Color { get; private set; }
    public string? Descripcion { get; private set; }

    private Fase() { } // requerido por EF Core

    private Fase(Guid id, Guid usuarioId, string nombre, int orden) : base(id)
    {
        UsuarioId = usuarioId;
        Nombre = nombre;
        Orden = orden;
    }

    public static Fase Crear(Guid usuarioId, string nombre, int orden)
    {
        if (usuarioId == Guid.Empty)
            throw new ArgumentException("La Fase debe pertenecer a un usuario válido.", nameof(usuarioId));

        if (string.IsNullOrWhiteSpace(nombre))
            throw new ArgumentException("El nombre de la Fase no puede estar vacío.", nameof(nombre));

        if (orden < 1)
            throw new ArgumentOutOfRangeException(nameof(orden), orden, "El orden de la Fase debe ser un entero positivo.");

        return new Fase(Guid.CreateVersion7(), usuarioId, nombre.Trim(), orden);
    }

    public void CambiarNombre(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ArgumentException("El nombre de la Fase no puede estar vacío.", nameof(nombre));

        Nombre = nombre.Trim();
    }

    public void Reordenar(int nuevoOrden)
    {
        if (nuevoOrden < 1)
            throw new ArgumentOutOfRangeException(nameof(nuevoOrden), nuevoOrden, "El orden de la Fase debe ser un entero positivo.");

        Orden = nuevoOrden;
    }

    public void AsignarColor(string? color) => Color = color;

    public void ActualizarDescripcion(string? descripcion) => Descripcion = descripcion;
}
