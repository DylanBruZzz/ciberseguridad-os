namespace Aprendizaje.Aplicacion.Evidence.Proyectos.VincularProyectoATema;

public enum VincularProyectoATemaEstado
{
    Actualizado,
    ProyectoNoEncontrado,
    TemaNoEncontrado,
    UsuarioNoCoincide
}

public sealed record VincularProyectoATemaResultado(VincularProyectoATemaEstado Estado)
{
    public static VincularProyectoATemaResultado Actualizado() =>
        new(VincularProyectoATemaEstado.Actualizado);

    public static VincularProyectoATemaResultado ProyectoNoEncontrado() =>
        new(VincularProyectoATemaEstado.ProyectoNoEncontrado);

    public static VincularProyectoATemaResultado TemaNoEncontrado() =>
        new(VincularProyectoATemaEstado.TemaNoEncontrado);

    public static VincularProyectoATemaResultado UsuarioNoCoincide() =>
        new(VincularProyectoATemaEstado.UsuarioNoCoincide);
}
