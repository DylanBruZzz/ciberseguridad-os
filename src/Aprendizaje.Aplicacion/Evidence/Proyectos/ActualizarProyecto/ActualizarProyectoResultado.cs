namespace Aprendizaje.Aplicacion.Evidence.Proyectos.ActualizarProyecto;

public enum ActualizarProyectoEstado
{
    Actualizado,
    UsuarioNoEncontrado,
    ProyectoNoEncontrado,
    UsuarioNoCoincide
}

public sealed record ActualizarProyectoResultado(ActualizarProyectoEstado Estado)
{
    public static ActualizarProyectoResultado Actualizado() => new(ActualizarProyectoEstado.Actualizado);

    public static ActualizarProyectoResultado UsuarioNoEncontrado() =>
        new(ActualizarProyectoEstado.UsuarioNoEncontrado);

    public static ActualizarProyectoResultado ProyectoNoEncontrado() =>
        new(ActualizarProyectoEstado.ProyectoNoEncontrado);

    public static ActualizarProyectoResultado UsuarioNoCoincide() =>
        new(ActualizarProyectoEstado.UsuarioNoCoincide);
}
