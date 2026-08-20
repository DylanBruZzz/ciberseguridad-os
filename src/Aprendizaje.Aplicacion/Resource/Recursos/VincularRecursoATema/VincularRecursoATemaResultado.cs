namespace Aprendizaje.Aplicacion.Resource.Recursos.VincularRecursoATema;

public enum VincularRecursoATemaEstado
{
    Actualizado,
    RecursoNoEncontrado,
    TemaNoEncontrado,
    UsuarioNoCoincide
}

public sealed record VincularRecursoATemaResultado(VincularRecursoATemaEstado Estado)
{
    public static VincularRecursoATemaResultado Actualizado() => new(VincularRecursoATemaEstado.Actualizado);

    public static VincularRecursoATemaResultado RecursoNoEncontrado() =>
        new(VincularRecursoATemaEstado.RecursoNoEncontrado);

    public static VincularRecursoATemaResultado TemaNoEncontrado() =>
        new(VincularRecursoATemaEstado.TemaNoEncontrado);

    public static VincularRecursoATemaResultado UsuarioNoCoincide() =>
        new(VincularRecursoATemaEstado.UsuarioNoCoincide);
}
