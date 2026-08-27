using Aprendizaje.Dominio.Comun;

namespace Aprendizaje.Dominio.Roadmap;

/// <summary>
/// Agrupador visual/curricular sobre el grafo de Temas. No tiene entidades internas,
/// Value Objects ni Domain Events — sus únicas invariantes son de validación simple.
/// </summary>
public sealed class Fase : AggregateRoot
{
    private List<string> _objetivos = new();
    private List<string> _criteriosAvance = new();

    public Guid UsuarioId { get; private set; }
    public string Nombre { get; private set; } = null!;
    public int Orden { get; private set; }
    public string? Color { get; private set; }
    public string? Descripcion { get; private set; }
    public IReadOnlyList<string> Objetivos => ObtenerObjetivosInternos().AsReadOnly();
    public IReadOnlyList<string> CriteriosAvance => ObtenerCriteriosAvanceInternos().AsReadOnly();
    public int? MesInicioRecomendado { get; private set; }
    public int? MesFinRecomendado { get; private set; }
    public string? CargaSemanalRecomendada { get; private set; }

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

    public void ConfigurarMetadataPedagogica(
        IEnumerable<string> objetivos,
        IEnumerable<string> criteriosAvance,
        int? mesInicioRecomendado,
        int? mesFinRecomendado,
        string? cargaSemanalRecomendada)
    {
        ArgumentNullException.ThrowIfNull(objetivos);
        ArgumentNullException.ThrowIfNull(criteriosAvance);

        if (mesInicioRecomendado.HasValue && mesInicioRecomendado.Value < 1)
            throw new ArgumentOutOfRangeException(
                nameof(mesInicioRecomendado),
                mesInicioRecomendado,
                "El mes de inicio recomendado debe ser un entero positivo.");

        if (mesFinRecomendado.HasValue && mesFinRecomendado.Value < 1)
            throw new ArgumentOutOfRangeException(
                nameof(mesFinRecomendado),
                mesFinRecomendado,
                "El mes de fin recomendado debe ser un entero positivo.");

        if (mesInicioRecomendado.HasValue
            && mesFinRecomendado.HasValue
            && mesFinRecomendado.Value < mesInicioRecomendado.Value)
        {
            throw new ArgumentException(
                "El mes de fin recomendado no puede ser anterior al mes de inicio recomendado.",
                nameof(mesFinRecomendado));
        }

        var objetivosInternos = ObtenerObjetivosInternos();
        objetivosInternos.Clear();
        objetivosInternos.AddRange(objetivos.Where(o => !string.IsNullOrWhiteSpace(o)).Select(o => o.Trim()));

        var criteriosInternos = ObtenerCriteriosAvanceInternos();
        criteriosInternos.Clear();
        criteriosInternos.AddRange(criteriosAvance.Where(c => !string.IsNullOrWhiteSpace(c)).Select(c => c.Trim()));

        MesInicioRecomendado = mesInicioRecomendado;
        MesFinRecomendado = mesFinRecomendado;
        CargaSemanalRecomendada = string.IsNullOrWhiteSpace(cargaSemanalRecomendada)
            ? null
            : cargaSemanalRecomendada.Trim();
    }

    private List<string> ObtenerObjetivosInternos()
    {
        if (_objetivos is null)
            _objetivos = new List<string>();

        return _objetivos;
    }

    private List<string> ObtenerCriteriosAvanceInternos()
    {
        if (_criteriosAvance is null)
            _criteriosAvance = new List<string>();

        return _criteriosAvance;
    }
}
