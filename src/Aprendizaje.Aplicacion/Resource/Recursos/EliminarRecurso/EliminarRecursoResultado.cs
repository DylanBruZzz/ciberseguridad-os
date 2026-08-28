namespace Aprendizaje.Aplicacion.Resource.Recursos.EliminarRecurso;

public enum EliminarRecursoEstado
{
    Eliminado,
    UsuarioNoEncontrado,
    RecursoNoEncontrado,
    UsuarioNoCoincide
}

public sealed record EliminarRecursoResultado(EliminarRecursoEstado Estado)
{
    public static EliminarRecursoResultado Eliminado() =>
        new(EliminarRecursoEstado.Eliminado);

    public static EliminarRecursoResultado UsuarioNoEncontrado() =>
        new(EliminarRecursoEstado.UsuarioNoEncontrado);

    public static EliminarRecursoResultado RecursoNoEncontrado() =>
        new(EliminarRecursoEstado.RecursoNoEncontrado);

    public static EliminarRecursoResultado UsuarioNoCoincide() =>
        new(EliminarRecursoEstado.UsuarioNoCoincide);
}
