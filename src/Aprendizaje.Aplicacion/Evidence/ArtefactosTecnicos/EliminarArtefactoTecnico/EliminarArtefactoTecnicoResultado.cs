namespace Aprendizaje.Aplicacion.Evidence.ArtefactosTecnicos.EliminarArtefactoTecnico;

public enum EliminarArtefactoTecnicoEstado
{
    Eliminado,
    UsuarioNoEncontrado,
    ArtefactoTecnicoNoEncontrado,
    UsuarioNoCoincide
}

public sealed record EliminarArtefactoTecnicoResultado(EliminarArtefactoTecnicoEstado Estado)
{
    public static EliminarArtefactoTecnicoResultado Eliminado() =>
        new(EliminarArtefactoTecnicoEstado.Eliminado);

    public static EliminarArtefactoTecnicoResultado UsuarioNoEncontrado() =>
        new(EliminarArtefactoTecnicoEstado.UsuarioNoEncontrado);

    public static EliminarArtefactoTecnicoResultado ArtefactoTecnicoNoEncontrado() =>
        new(EliminarArtefactoTecnicoEstado.ArtefactoTecnicoNoEncontrado);

    public static EliminarArtefactoTecnicoResultado UsuarioNoCoincide() =>
        new(EliminarArtefactoTecnicoEstado.UsuarioNoCoincide);
}
