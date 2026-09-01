using Aprendizaje.Dominio.Comun;

namespace Aprendizaje.Dominio.Roadmap;

/// <summary>
/// Apuntes personales permanentes y editables de un Tema. No reemplaza Objetivos
/// ni Nota: es el texto vivo que el usuario conserva como memoria propia del Tema.
/// </summary>
public sealed class ApunteTema : Entidad
{
    public Guid UsuarioId { get; private set; }
    public Guid TemaId { get; private set; }
    public string Contenido { get; private set; } = string.Empty;

    private ApunteTema() { } // requerido por EF Core

    private ApunteTema(Guid id, Guid usuarioId, Guid temaId, string contenido) : base(id)
    {
        UsuarioId = usuarioId;
        TemaId = temaId;
        Contenido = NormalizarContenido(contenido);
    }

    public static ApunteTema Crear(Guid usuarioId, Guid temaId, string contenido)
    {
        if (usuarioId == Guid.Empty)
            throw new ArgumentException("El Apunte de Tema debe pertenecer a un usuario válido.", nameof(usuarioId));

        if (temaId == Guid.Empty)
            throw new ArgumentException("El Tema referenciado no puede ser vacío.", nameof(temaId));

        return new ApunteTema(Guid.CreateVersion7(), usuarioId, temaId, contenido);
    }

    public void ReemplazarContenido(string contenido) => Contenido = NormalizarContenido(contenido);

    private static string NormalizarContenido(string contenido)
    {
        ArgumentNullException.ThrowIfNull(contenido);

        return contenido.Trim();
    }
}
