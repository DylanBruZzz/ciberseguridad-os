namespace Aprendizaje.Aplicacion.Roadmap.Temas.AsignarTemaPadre;

public enum AsignarTemaPadreEstado
{
    Actualizado,
    TemaNoEncontrado,
    TemaPadreNoEncontrado,
    UsuarioNoCoincide,
    ConflictoJerarquia
}

public sealed record AsignarTemaPadreResultado(AsignarTemaPadreEstado Estado)
{
    public static AsignarTemaPadreResultado Actualizado() => new(AsignarTemaPadreEstado.Actualizado);

    public static AsignarTemaPadreResultado TemaNoEncontrado() => new(AsignarTemaPadreEstado.TemaNoEncontrado);

    public static AsignarTemaPadreResultado TemaPadreNoEncontrado() => new(AsignarTemaPadreEstado.TemaPadreNoEncontrado);

    public static AsignarTemaPadreResultado UsuarioNoCoincide() => new(AsignarTemaPadreEstado.UsuarioNoCoincide);

    public static AsignarTemaPadreResultado ConflictoJerarquia() => new(AsignarTemaPadreEstado.ConflictoJerarquia);
}
