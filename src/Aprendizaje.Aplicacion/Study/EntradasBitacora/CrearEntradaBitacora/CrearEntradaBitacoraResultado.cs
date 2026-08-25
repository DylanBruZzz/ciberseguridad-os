using Aprendizaje.Dominio.Study;

namespace Aprendizaje.Aplicacion.Study.EntradasBitacora.CrearEntradaBitacora;

public enum CrearEntradaBitacoraEstado
{
    Creada,
    TemaNoEncontrado,
    UsuarioNoCoincide
}

public sealed record CrearEntradaBitacoraResultado(
    CrearEntradaBitacoraEstado Estado,
    Guid? Id = null,
    Guid? UsuarioId = null,
    Guid? TemaId = null,
    DateTime? Fecha = null,
    string? Texto = null)
{
    public static CrearEntradaBitacoraResultado Creada(EntradaBitacora entrada) =>
        new(
            CrearEntradaBitacoraEstado.Creada,
            entrada.Id,
            entrada.UsuarioId,
            entrada.TemaId,
            entrada.Fecha,
            entrada.Texto);

    public static CrearEntradaBitacoraResultado TemaNoEncontrado() =>
        new(CrearEntradaBitacoraEstado.TemaNoEncontrado);

    public static CrearEntradaBitacoraResultado UsuarioNoCoincide() =>
        new(CrearEntradaBitacoraEstado.UsuarioNoCoincide);
}
