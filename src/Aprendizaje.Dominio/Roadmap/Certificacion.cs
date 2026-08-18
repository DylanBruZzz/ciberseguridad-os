using Aprendizaje.Dominio.Comun;

namespace Aprendizaje.Dominio.Roadmap;

/// <summary>
/// Catálogo global compartido (no pertenece a un Usuario) — referencia de una certificación
/// externa y su temario. Sin borrado lógico: no implementa IEliminableLogicamente.
/// </summary>
public sealed class Certificacion : AggregateRoot
{
    public string Nombre { get; private set; } = null!;
    public string? Proveedor { get; private set; }
    public TipoCosto TipoCosto { get; private set; }
    public string? Url { get; private set; }

    private Certificacion() { } // requerido por EF Core

    private Certificacion(Guid id, string nombre, TipoCosto tipoCosto) : base(id)
    {
        Nombre = nombre;
        TipoCosto = tipoCosto;
    }

    public static Certificacion Crear(string nombre, TipoCosto tipoCosto)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ArgumentException("El nombre de la Certificación no puede estar vacío.", nameof(nombre));

        return new Certificacion(Guid.CreateVersion7(), nombre.Trim(), tipoCosto);
    }

    public void ActualizarProveedor(string? proveedor) => Proveedor = proveedor;

    public void ActualizarUrl(string? url) => Url = url;

    public void CambiarTipoCosto(TipoCosto tipoCosto) => TipoCosto = tipoCosto;
}
