using Aprendizaje.Dominio.Comun;

namespace Aprendizaje.Dominio.Nucleo;

/// <summary>
/// Shared kernel del que dependen transitivamente todos los demás Aggregate Roots.
/// Implementa IEliminableLogicamente: es el propietario lógico del historial del usuario, y
/// sus relaciones ya usan NO ACTION sin excepción — el borrado de una cuenta desactiva la
/// cuenta lógicamente sin destruir el historial de aprendizaje asociado. Impedir el acceso de
/// un Usuario eliminado es responsabilidad de la capa de aplicación/seguridad, no de este dominio.
/// VersionFila se conserva porque el SQL ya lo definía desde la Fase 1 — documentado como parte
/// de la convención 11 revisada (aplicación uniforme pendiente, no resuelta entidad por entidad).
/// </summary>
public sealed class Usuario : AggregateRoot, IEliminableLogicamente
{
    public string Nombre { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public DateTime FechaRegistro { get; private set; }
    public int IntervaloRepasoDefectoDias { get; private set; }
    public Guid? CertificacionObjetivoActivaId { get; private set; }

    public byte[]? VersionFila { get; private set; }

    public DateTime? FechaEliminacionUtc { get; private set; }

    private const int IntervaloRepasoDefectoInicial = 30;

    private Usuario() { } // requerido por EF Core

    private Usuario(Guid id, string nombre, string email) : base(id)
    {
        Nombre = nombre;
        Email = email;
        FechaRegistro = DateTime.UtcNow;
        IntervaloRepasoDefectoDias = IntervaloRepasoDefectoInicial;
    }

    public static Usuario Registrar(string nombre, string email)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ArgumentException("El nombre del Usuario no puede estar vacío.", nameof(nombre));

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("El email del Usuario no puede estar vacío.", nameof(email));

        return new Usuario(Guid.CreateVersion7(), nombre.Trim(), email.Trim());
    }

    public void CambiarNombre(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ArgumentException("El nombre del Usuario no puede estar vacío.", nameof(nombre));

        Nombre = nombre.Trim();
    }

    public void ActualizarEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("El email del Usuario no puede estar vacío.", nameof(email));

        Email = email.Trim();
    }

    public void ConfigurarIntervaloRepasoDefecto(int dias)
    {
        if (dias <= 0)
            throw new ArgumentOutOfRangeException(nameof(dias), dias, "El intervalo de repaso por defecto debe ser mayor a cero días.");

        IntervaloRepasoDefectoDias = dias;
    }

    public void EstablecerCertificacionObjetivo(Guid? certificacionId) => CertificacionObjetivoActivaId = certificacionId;

    public void MarcarComoEliminado() => FechaEliminacionUtc = DateTime.UtcNow;
}
