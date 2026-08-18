using Aprendizaje.Dominio.Comun;
using Aprendizaje.Dominio.Roadmap.Eventos;
using Aprendizaje.Dominio.Roadmap.ValueObjects;

namespace Aprendizaje.Dominio.Roadmap;

/// <summary>
/// Aggregate Root central del Roadmap engine. El límite transaccional de este agregado
/// incluye a CriterioTema (entidad interna); Fase, TemaPadre, Competencia, Certificacion y
/// TemaDependencia se referencian solo por Guid, nunca por navegación de objeto completo.
/// </summary>
public sealed class Tema : AggregateRoot, IEliminableLogicamente
{
    private const int MinimoCriteriosRelevantes = 2;
    private const int MaximoCriteriosRelevantes = 5;

    private readonly List<CriterioTema> _criterios = new();
    private readonly List<string> _objetivos = new();

    public Guid UsuarioId { get; private set; }
    public Guid? FaseId { get; private set; }
    public Guid? TemaPadreId { get; private set; }
    public string Nombre { get; private set; } = null!;
    public string? Descripcion { get; private set; }
    public IReadOnlyList<string> Objetivos => _objetivos.AsReadOnly();
    public TipoConocimiento TipoConocimiento { get; private set; }
    public DateOnly? FechaInicio { get; private set; }
    public DateOnly? FechaFin { get; private set; }
    public NivelPercepcion? DificultadPercibida { get; private set; }
    public NivelPercepcion? Confianza { get; private set; }
    public IntervaloRepaso? IntervaloRepaso { get; private set; }

    /// <summary>Token de concurrencia optimista — ver convención 11: solo en las entidades que lo requieren.</summary>
    public byte[]? VersionFila { get; private set; }

    public DateTime? FechaEliminacionUtc { get; private set; }

    public IReadOnlyCollection<CriterioTema> Criterios => _criterios.AsReadOnly();

    private Tema() { } // requerido por EF Core

    private Tema(Guid id, Guid usuarioId, string nombre, TipoConocimiento tipoConocimiento) : base(id)
    {
        UsuarioId = usuarioId;
        Nombre = nombre;
        TipoConocimiento = tipoConocimiento;
    }

    public static Tema Crear(Guid usuarioId, string nombre, TipoConocimiento tipoConocimiento)
    {
        if (usuarioId == Guid.Empty)
            throw new ArgumentException("El Tema debe pertenecer a un usuario válido.", nameof(usuarioId));

        if (string.IsNullOrWhiteSpace(nombre))
            throw new ArgumentException("El nombre del Tema no puede estar vacío.", nameof(nombre));

        return new Tema(Guid.CreateVersion7(), usuarioId, nombre.Trim(), tipoConocimiento);
    }

    // ---------- Datos descriptivos ----------

    public void CambiarNombre(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ArgumentException("El nombre del Tema no puede estar vacío.", nameof(nombre));

        Nombre = nombre.Trim();
    }

    public void ActualizarDescripcion(string? descripcion) => Descripcion = descripcion;

    /// <summary>
    /// Reemplaza la lista completa de objetivos de aprendizaje. Son contenido descriptivo y
    /// estático del Tema — no tienen estado ni historia propia (ver convención 17, punto 3
    /// de la revisión de artefactos: no se promueven a entidad separada).
    /// </summary>
    public void EstablecerObjetivos(IEnumerable<string> objetivos)
    {
        _objetivos.Clear();
        _objetivos.AddRange(objetivos.Where(o => !string.IsNullOrWhiteSpace(o)).Select(o => o.Trim()));
    }

    // ---------- Ubicación en el roadmap ----------

    public void AsignarFase(Guid? faseId) => FaseId = faseId;

    /// <summary>
    /// Solo impide la autorreferencia directa (equivalente a CK_Tema_NoAutoPadre en el esquema).
    /// La detección de ciclos más profundos en la jerarquía (A padre de B, B padre de A) es
    /// responsabilidad de un servicio de aplicación con acceso al repositorio — mismo criterio
    /// ya aplicado a IValidadorDependencias para el grafo de TemaDependencia.
    /// </summary>
    public void AsignarTemaPadre(Guid? temaPadreId)
    {
        if (temaPadreId.HasValue && temaPadreId.Value == Id)
            throw new InvalidOperationException("Un Tema no puede ser subtema de sí mismo.");

        TemaPadreId = temaPadreId;
    }

    // ---------- Percepción y planificación ----------

    public void ActualizarDificultadPercibida(NivelPercepcion? dificultad) => DificultadPercibida = dificultad;

    public void ActualizarConfianza(NivelPercepcion? confianza) => Confianza = confianza;

    public void ConfigurarIntervaloRepaso(IntervaloRepaso? intervalo) => IntervaloRepaso = intervalo;

    public void IniciarEstudio(DateOnly fecha)
    {
        if (FechaFin.HasValue && fecha > FechaFin.Value)
            throw new InvalidOperationException("La fecha de inicio no puede ser posterior a la fecha de fin ya registrada.");

        FechaInicio = fecha;
    }

    public void FinalizarEstudio(DateOnly fecha)
    {
        if (FechaInicio.HasValue && fecha < FechaInicio.Value)
            throw new InvalidOperationException("La fecha de fin no puede ser anterior a la fecha de inicio.");

        FechaFin = fecha;
    }

    // ---------- Criterios de dominio ----------

    /// <summary>
    /// Define qué criterios son relevantes para este Tema (entre 2 y 5 — no todos los temas
    /// cargan los 5 obligatoriamente, ver diseño del sistema de dominio multicriterio).
    /// Solo se permite mientras ningún criterio tenga progreso registrado, para no descartar
    /// silenciosamente avance ya hecho.
    /// </summary>
    public void DefinirCriteriosRelevantes(IReadOnlyCollection<TipoCriterio> tipos)
    {
        if (_criterios.Any(c => c.Cumplido))
            throw new InvalidOperationException(
                "No se pueden redefinir los criterios relevantes mientras exista progreso registrado.");

        var distintos = tipos.Distinct().ToList();

        if (distintos.Count is < MinimoCriteriosRelevantes or > MaximoCriteriosRelevantes)
            throw new ArgumentException(
                $"Un Tema debe tener entre {MinimoCriteriosRelevantes} y {MaximoCriteriosRelevantes} criterios relevantes.",
                nameof(tipos));

        _criterios.Clear();
        _criterios.AddRange(distintos.Select(tipo => new CriterioTema(Id, tipo)));
    }

    /// <summary>
    /// Marca un criterio como cumplido. Operación idempotente: si ya estaba cumplido, no hace
    /// nada — así nunca se sobrescribe silenciosamente la fecha original en que se demostró
    /// por primera vez. Si esta marca completa el 100% de los criterios relevantes, emite
    /// TemaDominadoEvento — solo en la transición, nunca de forma repetida.
    /// </summary>
    public void MarcarCriterio(TipoCriterio tipo)
    {
        var criterio = ObtenerCriterioRelevante(tipo);

        if (criterio.Cumplido)
            return;

        var yaDominadoAntes = EstaDominado();

        criterio.Marcar();

        if (!yaDominadoAntes && EstaDominado())
            RegistrarEvento(new TemaDominadoEvento(Id, DateTime.UtcNow));
    }

    public void DesmarcarCriterio(TipoCriterio tipo) => ObtenerCriterioRelevante(tipo).Desmarcar();

    private CriterioTema ObtenerCriterioRelevante(TipoCriterio tipo) =>
        _criterios.FirstOrDefault(c => c.Tipo == tipo)
        ?? throw new InvalidOperationException(
            $"{tipo} no es un criterio relevante para este Tema. Defínelo primero con DefinirCriteriosRelevantes.");

    public bool EstaDominado() =>
        _criterios.Count > 0 && _criterios.All(c => c.Cumplido);

    /// <summary>
    /// Estado calculado, nunca persistido. Recibe tanto la última fecha de práctica como el
    /// intervalo de repaso *efectivo* desde afuera: Tema no conoce SesionEstudio ni Usuario
    /// (agregados de otros módulos), así que ni la fecha ni la resolución de "intervalo propio
    /// del Tema vs. intervalo por defecto del Usuario" son responsabilidad de este método —
    /// esa resolución ocurre en el caso de uso de Aplicación que orquesta ambos repositorios.
    /// Debe mantenerse en paridad exacta con roadmap.vw_TemaEstado (convención 20).
    /// </summary>
    public EstadoTema CalcularEstado(DateTime? ultimaPracticaUtc, DateTime ahoraUtc, IntervaloRepaso intervaloEfectivo)
    {
        ArgumentNullException.ThrowIfNull(intervaloEfectivo);

        if (_criterios.Count == 0)
            return EstadoTema.NoIniciado;

        var cumplidos = _criterios.Count(c => c.Cumplido);

        if (cumplidos == 0)
            return EstadoTema.NoIniciado;

        if (cumplidos < _criterios.Count)
            return EstadoTema.EnPractica;

        if (ultimaPracticaUtc.HasValue && intervaloEfectivo.HaVencido(ultimaPracticaUtc.Value, ahoraUtc))
            return EstadoTema.EnRepaso;

        return EstadoTema.Dominado;
    }

    // ---------- Borrado lógico ----------

    public void MarcarComoEliminado() => FechaEliminacionUtc = DateTime.UtcNow;
}
