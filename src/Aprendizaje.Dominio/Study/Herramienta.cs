using Aprendizaje.Dominio.Comun;

namespace Aprendizaje.Dominio.Study;

/// <summary>
/// Catálogo global compartido (no pertenece a un Usuario) de herramientas usables por
/// SesionEstudio, Proyecto, Laboratorio y ArtefactoTecnico. Sin borrado lógico: no implementa
/// IEliminableLogicamente.
/// </summary>
public sealed class Herramienta : AggregateRoot
{
    public string Nombre { get; private set; } = null!;
    public string? Categoria { get; private set; }

    private Herramienta() { } // requerido por EF Core

    private Herramienta(Guid id, string nombre) : base(id)
    {
        Nombre = nombre;
    }

    public static Herramienta Crear(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ArgumentException("El nombre de la Herramienta no puede estar vacío.", nameof(nombre));

        return new Herramienta(Guid.CreateVersion7(), nombre.Trim());
    }

    public void CambiarNombre(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ArgumentException("El nombre de la Herramienta no puede estar vacío.", nameof(nombre));

        Nombre = nombre.Trim();
    }

    public void ActualizarCategoria(string? categoria) => Categoria = categoria;
}
