namespace Aprendizaje.Aplicacion.Evidence.Laboratorios.EliminarLaboratorio;

public enum EliminarLaboratorioEstado
{
    Eliminado,
    UsuarioNoEncontrado,
    LaboratorioNoEncontrado,
    UsuarioNoCoincide
}

public sealed record EliminarLaboratorioResultado(EliminarLaboratorioEstado Estado)
{
    public static EliminarLaboratorioResultado Eliminado() => new(EliminarLaboratorioEstado.Eliminado);

    public static EliminarLaboratorioResultado UsuarioNoEncontrado() =>
        new(EliminarLaboratorioEstado.UsuarioNoEncontrado);

    public static EliminarLaboratorioResultado LaboratorioNoEncontrado() =>
        new(EliminarLaboratorioEstado.LaboratorioNoEncontrado);

    public static EliminarLaboratorioResultado UsuarioNoCoincide() =>
        new(EliminarLaboratorioEstado.UsuarioNoCoincide);
}
