using Aprendizaje.Dominio.Comun;

namespace Aprendizaje.Dominio.Roadmap;

/// <summary>
/// Habilidad transversal que agrega varios Temas (vínculo gestionado por la tabla de unión
/// CompetenciaTema, no por navegación de objeto). Su nivel de madurez se calcula, no se persiste.
/// </summary>
public sealed class Competencia : AggregateRoot
{
    public Guid UsuarioId { get; private set; }
    public string Nombre { get; private set; } = null!;
    public string? Descripcion { get; private set; }

    private Competencia() { } // requerido por EF Core

    private Competencia(Guid id, Guid usuarioId, string nombre) : base(id)
    {
        UsuarioId = usuarioId;
        Nombre = nombre;
    }

    public static Competencia Crear(Guid usuarioId, string nombre)
    {
        if (usuarioId == Guid.Empty)
            throw new ArgumentException("La Competencia debe pertenecer a un usuario válido.", nameof(usuarioId));

        if (string.IsNullOrWhiteSpace(nombre))
            throw new ArgumentException("El nombre de la Competencia no puede estar vacío.", nameof(nombre));

        return new Competencia(Guid.CreateVersion7(), usuarioId, nombre.Trim());
    }

    public void CambiarNombre(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ArgumentException("El nombre de la Competencia no puede estar vacío.", nameof(nombre));

        Nombre = nombre.Trim();
    }

    public void ActualizarDescripcion(string? descripcion) => Descripcion = descripcion;
}
