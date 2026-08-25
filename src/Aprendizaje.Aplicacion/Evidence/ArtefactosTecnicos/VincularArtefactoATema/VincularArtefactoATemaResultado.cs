namespace Aprendizaje.Aplicacion.Evidence.ArtefactosTecnicos.VincularArtefactoATema;

public sealed record VincularArtefactoATemaResultado(VincularArtefactoATemaEstado Estado)
{
    public static VincularArtefactoATemaResultado Actualizado() =>
        new(VincularArtefactoATemaEstado.Actualizado);

    public static VincularArtefactoATemaResultado ArtefactoNoEncontrado() =>
        new(VincularArtefactoATemaEstado.ArtefactoNoEncontrado);

    public static VincularArtefactoATemaResultado TemaNoEncontrado() =>
        new(VincularArtefactoATemaEstado.TemaNoEncontrado);

    public static VincularArtefactoATemaResultado UsuarioNoCoincide() =>
        new(VincularArtefactoATemaEstado.UsuarioNoCoincide);
}

public enum VincularArtefactoATemaEstado
{
    Actualizado,
    ArtefactoNoEncontrado,
    TemaNoEncontrado,
    UsuarioNoCoincide
}
