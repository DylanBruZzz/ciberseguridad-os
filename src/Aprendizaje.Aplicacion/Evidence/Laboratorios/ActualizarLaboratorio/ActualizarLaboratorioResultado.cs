namespace Aprendizaje.Aplicacion.Evidence.Laboratorios.ActualizarLaboratorio;

public enum ActualizarLaboratorioEstado
{
    Actualizado,
    UsuarioNoEncontrado,
    LaboratorioNoEncontrado,
    UsuarioNoCoincide
}

public sealed record ActualizarLaboratorioResultado(ActualizarLaboratorioEstado Estado)
{
    public static ActualizarLaboratorioResultado Actualizado() => new(ActualizarLaboratorioEstado.Actualizado);

    public static ActualizarLaboratorioResultado UsuarioNoEncontrado() =>
        new(ActualizarLaboratorioEstado.UsuarioNoEncontrado);

    public static ActualizarLaboratorioResultado LaboratorioNoEncontrado() =>
        new(ActualizarLaboratorioEstado.LaboratorioNoEncontrado);

    public static ActualizarLaboratorioResultado UsuarioNoCoincide() =>
        new(ActualizarLaboratorioEstado.UsuarioNoCoincide);
}
