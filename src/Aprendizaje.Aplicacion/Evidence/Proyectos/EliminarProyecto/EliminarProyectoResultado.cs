namespace Aprendizaje.Aplicacion.Evidence.Proyectos.EliminarProyecto;

public enum EliminarProyectoEstado
{
    Eliminado,
    UsuarioNoEncontrado,
    ProyectoNoEncontrado,
    UsuarioNoCoincide
}

public sealed record EliminarProyectoResultado(EliminarProyectoEstado Estado)
{
    public static EliminarProyectoResultado Eliminado() => new(EliminarProyectoEstado.Eliminado);

    public static EliminarProyectoResultado UsuarioNoEncontrado() =>
        new(EliminarProyectoEstado.UsuarioNoEncontrado);

    public static EliminarProyectoResultado ProyectoNoEncontrado() =>
        new(EliminarProyectoEstado.ProyectoNoEncontrado);

    public static EliminarProyectoResultado UsuarioNoCoincide() =>
        new(EliminarProyectoEstado.UsuarioNoCoincide);
}
