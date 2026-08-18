using Aprendizaje.Dominio.Comun;

namespace Aprendizaje.Dominio.Evidence;

/// <summary>
/// Aggregate Root independiente, no entidad interna de ninguno de sus 5 posibles padres —
/// decisión ya tomada en la revisión de implementación, precisamente para que borrar un
/// Proyecto/Laboratorio/Writeup/Artefacto no arrastre silenciosamente las notas que documentan
/// la evolución del entendimiento sobre ellos. Append-only: no se expone ningún método para
/// editar Texto una vez creada, a diferencia de SesionEstudio/EntradaBitacora.
/// </summary>
public sealed class Nota : AggregateRoot, IEliminableLogicamente
{
    public Guid UsuarioId { get; private set; }
    public Guid? TemaId { get; private set; }
    public Guid? ProyectoId { get; private set; }
    public Guid? LaboratorioId { get; private set; }
    public Guid? WriteupId { get; private set; }
    public Guid? ArtefactoTecnicoId { get; private set; }
    public DateTime Fecha { get; private set; }
    public string Texto { get; private set; } = null!;
    public TipoNota Tipo { get; private set; }

    public DateTime? FechaEliminacionUtc { get; private set; }

    private Nota() { } // requerido por EF Core

    private Nota(
        Guid id, Guid usuarioId, string texto, TipoNota tipo,
        Guid? temaId, Guid? proyectoId, Guid? laboratorioId, Guid? writeupId, Guid? artefactoTecnicoId)
        : base(id)
    {
        UsuarioId = usuarioId;
        Texto = texto;
        Tipo = tipo;
        TemaId = temaId;
        ProyectoId = proyectoId;
        LaboratorioId = laboratorioId;
        WriteupId = writeupId;
        ArtefactoTecnicoId = artefactoTecnicoId;
        Fecha = DateTime.UtcNow;
    }

    private static Nota Crear(
        Guid usuarioId, string texto, TipoNota tipo,
        Guid? temaId, Guid? proyectoId, Guid? laboratorioId, Guid? writeupId, Guid? artefactoTecnicoId)
    {
        if (usuarioId == Guid.Empty)
            throw new ArgumentException("La Nota debe pertenecer a un usuario válido.", nameof(usuarioId));

        if (string.IsNullOrWhiteSpace(texto))
            throw new ArgumentException("El texto de la Nota no puede estar vacío.", nameof(texto));

        return new Nota(
            Guid.CreateVersion7(), usuarioId, texto.Trim(), tipo,
            temaId, proyectoId, laboratorioId, writeupId, artefactoTecnicoId);
    }

    public static Nota SobreTema(Guid usuarioId, Guid temaId, string texto, TipoNota tipo = TipoNota.Nota)
    {
        if (temaId == Guid.Empty)
            throw new ArgumentException("El Tema referenciado no puede ser vacío.", nameof(temaId));

        return Crear(usuarioId, texto, tipo, temaId, null, null, null, null);
    }

    public static Nota SobreProyecto(Guid usuarioId, Guid proyectoId, string texto, TipoNota tipo = TipoNota.Nota)
    {
        if (proyectoId == Guid.Empty)
            throw new ArgumentException("El Proyecto referenciado no puede ser vacío.", nameof(proyectoId));

        return Crear(usuarioId, texto, tipo, null, proyectoId, null, null, null);
    }

    public static Nota SobreLaboratorio(Guid usuarioId, Guid laboratorioId, string texto, TipoNota tipo = TipoNota.Nota)
    {
        if (laboratorioId == Guid.Empty)
            throw new ArgumentException("El Laboratorio referenciado no puede ser vacío.", nameof(laboratorioId));

        return Crear(usuarioId, texto, tipo, null, null, laboratorioId, null, null);
    }

    public static Nota SobreWriteup(Guid usuarioId, Guid writeupId, string texto, TipoNota tipo = TipoNota.Nota)
    {
        if (writeupId == Guid.Empty)
            throw new ArgumentException("El Writeup referenciado no puede ser vacío.", nameof(writeupId));

        return Crear(usuarioId, texto, tipo, null, null, null, writeupId, null);
    }

    public static Nota SobreArtefactoTecnico(
        Guid usuarioId, Guid artefactoTecnicoId, string texto, TipoNota tipo = TipoNota.Nota)
    {
        if (artefactoTecnicoId == Guid.Empty)
            throw new ArgumentException("El Artefacto Técnico referenciado no puede ser vacío.", nameof(artefactoTecnicoId));

        return Crear(usuarioId, texto, tipo, null, null, null, null, artefactoTecnicoId);
    }

    public void MarcarComoEliminado() => FechaEliminacionUtc = DateTime.UtcNow;
}
