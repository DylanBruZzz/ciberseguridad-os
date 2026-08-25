namespace Aprendizaje.Aplicacion.Evidence.Laboratorios.VincularLaboratorioATema;

public enum VincularLaboratorioATemaEstado
{
    Actualizado,
    LaboratorioNoEncontrado,
    TemaNoEncontrado,
    UsuarioNoCoincide
}

public sealed record VincularLaboratorioATemaResultado(VincularLaboratorioATemaEstado Estado)
{
    public static VincularLaboratorioATemaResultado Actualizado() =>
        new(VincularLaboratorioATemaEstado.Actualizado);

    public static VincularLaboratorioATemaResultado LaboratorioNoEncontrado() =>
        new(VincularLaboratorioATemaEstado.LaboratorioNoEncontrado);

    public static VincularLaboratorioATemaResultado TemaNoEncontrado() =>
        new(VincularLaboratorioATemaEstado.TemaNoEncontrado);

    public static VincularLaboratorioATemaResultado UsuarioNoCoincide() =>
        new(VincularLaboratorioATemaEstado.UsuarioNoCoincide);
}
