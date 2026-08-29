namespace Aprendizaje.Aplicacion.Evidence.ArtefactosTecnicos.ActualizarArtefactoTecnico;

public enum ActualizarArtefactoTecnicoEstado
{
    Actualizado,
    UsuarioNoEncontrado,
    ArtefactoTecnicoNoEncontrado,
    UsuarioNoCoincide
}

public sealed record ActualizarArtefactoTecnicoResultado(ActualizarArtefactoTecnicoEstado Estado)
{
    public static ActualizarArtefactoTecnicoResultado Actualizado() =>
        new(ActualizarArtefactoTecnicoEstado.Actualizado);

    public static ActualizarArtefactoTecnicoResultado UsuarioNoEncontrado() =>
        new(ActualizarArtefactoTecnicoEstado.UsuarioNoEncontrado);

    public static ActualizarArtefactoTecnicoResultado ArtefactoTecnicoNoEncontrado() =>
        new(ActualizarArtefactoTecnicoEstado.ArtefactoTecnicoNoEncontrado);

    public static ActualizarArtefactoTecnicoResultado UsuarioNoCoincide() =>
        new(ActualizarArtefactoTecnicoEstado.UsuarioNoCoincide);
}
