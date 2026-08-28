namespace Aprendizaje.Aplicacion.Resource.Recursos.ActualizarRecurso;

public enum ActualizarRecursoEstado
{
    Actualizado,
    UsuarioNoEncontrado,
    RecursoNoEncontrado,
    UsuarioNoCoincide
}

public sealed record ActualizarRecursoResultado(ActualizarRecursoEstado Estado)
{
    public static ActualizarRecursoResultado Actualizado() =>
        new(ActualizarRecursoEstado.Actualizado);

    public static ActualizarRecursoResultado UsuarioNoEncontrado() =>
        new(ActualizarRecursoEstado.UsuarioNoEncontrado);

    public static ActualizarRecursoResultado RecursoNoEncontrado() =>
        new(ActualizarRecursoEstado.RecursoNoEncontrado);

    public static ActualizarRecursoResultado UsuarioNoCoincide() =>
        new(ActualizarRecursoEstado.UsuarioNoCoincide);
}
